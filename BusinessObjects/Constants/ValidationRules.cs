namespace BusinessObjects.Constants;

//Ae trong quá trình làm có thể bổ sung thêm chớ đừng xóa nhé

public static class ValidationRules
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public const int UsernameMinLength = 4;
    public const int UsernameMaxLength = 50;
    public const int PasswordMinLength = 6;

    public const int FullNameMaxLength = 150;
    public const int EmailMaxLength = 255;
    public const int PhoneNumberMaxLength = 20;
    public const int IdentityCardMaxLength = 30;
    public const int AddressMaxLength = 255;
    public const int RoomNumberMaxLength = 20;
    public const int NoteMaxLength = 500;

    public const int MinCapacity = 1;
    public const int MinNumberOfNights = 1;
    public const int MinQuantity = 1;
    public const decimal MinMoneyAmount = 0.01m;
}
