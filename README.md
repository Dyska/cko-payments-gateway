# Documentation for Payment Gateway Submission - Daniel Yska


## Running the solution

I developed the solution in Visual Studio Code with the C# Dev Kit extension. The projects in the solution have a target framework of `net8.0`, so a .NET SDK of 8.0 or higher is required. `dotnet --list-sdks` will tell you whether you have an appropriate SDK installed.

If running from the command line instead, use:
```bash
# Should find unit and integration test projects and run both
dotnet test
```

```bash
# Run the API
dotnet run --project src/PaymentGateway.Api/PaymentGateway.Api.csproj
```

When the project is running, the swagger docs should be available at `https://localhost:7092/swagger/index.html`

## How to hit the endpoints

### Create a payment
```bash
curl -X 'POST' \
  'https://localhost:7092/api/v1/payments' \
  -H 'Content-Type: application/json' \
  -H 'Idempotency-Token: 3f8b5394-02f5-45c6-a6c0-61ce927b1417' \
  -d '{
  "card": {
    "expiryMonth": "04",
    "expiryYear": "2025",
    "cardNumber": "2222405343248877",
    "cvv": "123"
  },
  "isoCurrencyCode": "GBP",
  "amount": 100
}'
```

Note that `Idempotency-Token` is a required header, and requests with duplicate values will be rejected.

### Get a payment
```bash
curl -X 'GET' \
  'https://localhost:7092/api/v1/payments/7ea57a54-4c7c-4db1-8239-864243b23ae4'
```

## Key design considerations

### Separation of API and domain logic
I implemented two `.csproj` files - `PaymentGateway.Api` and `PaymentGateway.Api.Domain`. This allowed separation of API/presentation concerns from core domain logic concerns. `PaymentGateway.Api` has a dependency on `PaymentGateway.Api.Domain` in order to map to the domain models, but `PaymentGateway.Api.Domain` has no dependency on the API project. Through distinct projects and namespacing, API models were intentionally kept distinct from domain models. This allows them to stay loosely coupled, so that each can change independently without system-wide refactoring required.

`PaymentGateway.Api.Domain` contains the logic to interface with the Bank client. I elected to contain all of the models, mapping, and client logic inside the `Clients\Bank` directory to keep these separate.

### Code architecture
A three - tier API architecture pattern was used, where projects are separated into presentation layer (API), application/business logic layer (services) and data access layer (repositories). 

### Use of database
An `IPaymentRepository` was used to represent the abstraction of the database. I elected to persist payments before submission to the bank, and then update the payment afterwards. 

Saving before submitting to the bank has the advantage of saving the request in case the service crashes. If requests to the bank are safe to retry, then payments with `Pending` states could be retried.

In a production application obviously an in-memory database wouldn't be used. A SQL solution is a good fit for payments data as it allows strong consistency, ACID transactions and referential integrity. For a SQL database, I would move the Authorization token provided by the Bank into its own table, and provide a foreign key back to the Payments table. This could be wrapped in a transaction with the update request.

### Input models
The `POST api/v1/payments` endpoint accepts strings for every field but the amount. This was intentional to preserve the exact values sent by the merchant. This is particularly important for fields such as the card number, cvv, and expiry month. All of these fields may contain leading 0's, and these should be preserved. Instead of letting the `[FromBody]` perform the bulk of validation, I moved this input validation to the constructors of the individual `Card` and `Payment` models.

### Idempotency header filter
I added an idempotency header filter to the `ProcessPost` endpoint. This requires requests to this endpoint to provide an `Idempotency-Token` header, rejecting requests without it. This token value will be checked in a cache, and if the token value is present it's assumed the request is duplicated, and it is rejected.

I believe this is an important non-functional requirement because it makes the `ProcessPost` endpoint idempotent, which POST endpoints aren't by default. This allows requests to the endpoint to be safely retried, as duplicate requests will be rejected, instead of creating duplicate payments. 

The naive implementation for the cache is a simple in-memory dictionary. Assuming the `PaymentGateway.Api` is deployed and replicated, the in-memory approach will only work if duplicate requests are made to the same container (possible through sticky sessions). A better approach would be moving to an external cache like Redis, making the architecture stateless. Multiple nodes in Redis would be required to make the cluster highly available, ensuring the cache doesn't become a single point of failure.

## Assumptions made

### Bank is validating payments comprehensively

The validation provided by the payment gateway is very light touch. Completely incorrect requests are rejected by the API without calling the bank, but the current validation will certainly allow invalid data to be sent to the bank. Depending on the relationship of the API connection with the bank, this may be an area of improvement. 

If for example, each request to the bank costs money, then time would be well spent building more comprehensive payment validation in the payment gateway.

### Slight additions to payment validation
These would need to be checked with a product person/SME to ensure these assumptions are valid. 

- Regarding Card month/year expiry - I've assumed Card is valid until UTC midnight in expiry month + year
- Amounts - I've assumed negative and 0 amounts are not permitted
