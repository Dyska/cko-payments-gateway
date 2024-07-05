namespace PaymentGateway.Api.Models;

public enum PaymentRequestStatus {
    Default = 0,
    Rejected = 1,
    Declined = 2,
    Authorized = 3,
}