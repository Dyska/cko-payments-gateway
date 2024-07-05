namespace PaymentGateway.Api.Domain.Models;

public enum PaymentStatus {
    Default = 0,
    Rejected = 1,
    Declined = 2,
    Authorized = 3,
}
