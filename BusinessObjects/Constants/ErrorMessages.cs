namespace BusinessObjects.Constants;

//Error message demo, ae trong quá trình làm có thể bổ sung thêm chớ đừng xóa nhé

public static class ErrorMessages
{
    public const string UnexpectedError = "An unexpected error occurred. Please try again.";
    public const string ValidationFailed = "The provided data is not valid.";
    public const string Unauthorized = "You need to log in before performing this action.";
    public const string Forbidden = "You do not have permission to perform this action.";
    public const string RecordNotFound = "The requested record was not found.";
    public const string DuplicateRecord = "The record already exists.";

    public const string InvalidCredentials = "Username or password is incorrect.";
    public const string AccountInactive = "This account is inactive.";
    public const string AccountLocked = "This account is locked.";

    public const string InvalidDateRange = "Check-out date must be later than check-in date.";
    public const string InvalidNumberOfNights = "Number of nights must be greater than zero.";
    public const string InvalidQuantity = "Quantity must be greater than zero.";
    public const string InvalidAmount = "Amount must be greater than zero.";

    public const string RoomNotAvailable = "The selected room is not available for the chosen dates.";
    public const string RoomUnderMaintenance = "The selected room is under maintenance.";
    public const string RoomInactive = "The selected room is inactive.";

    public const string BookingConflict = "The selected room already has an active booking in the chosen date range.";
    public const string DuplicateInvoice = "This booking already has an invoice.";
    public const string PaymentExceedsRemainingAmount = "Payment amount cannot exceed the remaining invoice amount.";

    // Additional error messages for service operations
    public const string SystemError = "A system error occurred. Please try again.";
    public const string InvalidInput = "Invalid input provided.";
    public const string NotFound = "Record not found.";
    public const string BusinessRuleViolation = "Business rule violation.";
    public const string DuplicateKey = "Duplicate record found.";
}
