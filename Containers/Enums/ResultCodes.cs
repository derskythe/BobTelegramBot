using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Containers.Enums
{
    [Serializable]
    [DataContract(Name = "ResultCodes")]
    public enum ResultCodes
    {
        [Description("OK")]
        [EnumMember]
        Ok = 0,

        [Description("Invalid number")]
        [EnumMember]
        InvalidNumber = 1,

        [Description("Invalid parameters")]
        [EnumMember]
        InvalidParameters = 2,

        [Description("Invalid terminal")]
        [EnumMember]
        InvalidTerminal = 3,

        [Description("Invalid terminal key")]
        [EnumMember]
        InvalidKey = 4,

        [Description("Invalid signature of message")]
        [EnumMember]
        InvalidSignature = 5,

        [Description("Invalid username or password")]
        [EnumMember]
        InvalidUsernameOrPassword = 6,

        [Description("Invalid session")]
        [EnumMember]
        InvalidSession = 7,

        [Description("Insufficient privileges")]
        [EnumMember]
        NoPriv = 8,

        [Description("Login failed")]
        [EnumMember]
        LoginFailed = 9,

        [Description("Invalid verify code")]
        [EnumMember]
        InvalidVerifyCode = 10,

        [Description("Order not payeed")]
        [EnumMember]
        NotPayeed = 11,

        [Description("Too many requests")]
        [EnumMember]
        TooManyRequests = 12,

        [Description("Reversal")]
        [EnumMember]
        Reversal = 13,

        [Description("Transaction Id Already Exists")]
        [EnumMember]
        TransactionIdAlreadyExists = 14,

        [Description("Customer not found")]
        [EnumMember]
        CustomerNotFound = 15,

        [Description("Invalid amount")]
        [EnumMember]
        InvalidAmount = 16,

        [Description("Invalid currency")]
        [EnumMember]
        InvalidCurrency = 17,

        [Description("Transaction Id not found")]
        [EnumMember]
        TransactionIdNotFound = 18,

        [Description("Record Not Belongs to current user")]
        [EnumMember]
        AccessDeniedForDbItem = 19,

        [Description("Email is still in use on system")]
        [EnumMember]
        EmailStillInUse = 20,

        [Description("Phone is still in use on system")]
        [EnumMember]
        PhoneStillInUse = 21,

        [Description("User is temporary blocked")]
        [EnumMember]
        UserTempBlocked = 22,

        [Description("Device hash code not correctly decrypted")]
        [EnumMember]
        DeviceHashCodeNotCorrectlyDecrypted = 23,

        [Description("Operation failed")]
        [EnumMember]
        OperationFailed = 24,

        [Description("User not activated yet")]
        [EnumMember]
        UserNotActivated = 25,

        [Description("User still activated")]
        [EnumMember]
        UserStillActivate = 26,



        [Description("Mobile user status is Locked")]
        [EnumMember]
        MobileUserLocked = 27,

        [Description("Device revoked")]
        [EnumMember]
        MobileDeviceRevoked = 28,

        [Description("Password changed from portal")]
        [EnumMember]
        PasswordChanged = 29,

        [Description("Mobile Otp send limit exceeded")]
        [EnumMember]
        MobileOtpSendLimitExceeded = 30,

        [Description("Tried to approve email again while it is approved")]
        [EnumMember]
        EmailApproved = 31,

        [Description("Email not found to continue")]
        [EnumMember]
        NoEmailFound = 32,

        [Description("Asan session not belongs to user or not found")]
        [EnumMember]
        AsanSessionError = 33,

        [Description("Only Asan login allowed")]
        [EnumMember]
        OnlyAsanLoginAllowed = 34,

        [Description("Otp Check Limit Exceeded")]
        [EnumMember]
        OtpCheckLimitExceeded = 35,

        [Description("Otp send limit exceeded")]
        [EnumMember]
        OtpSendLimitExceeded = 36,

        [Description("Not Found")]
        [EnumMember]
        NotFound = 37,

        [Description("Email send limit exceeded")]
        [EnumMember]
        EmailSendLimitExceeded = 38,


        [Description("Payment info service item is not selected")]
        [EnumMember]
        PaymentInfoServiceItemNotSelected = 40,


        [Description("This gift is not available in stock")]
        [EnumMember]
        NoGiftInStock = 41,

        [Description("Receiver Wallet Limit Exceeded")]
        [EnumMember]
        ReceiverWalletLimitExceeded = 42,

        [Description("Wrong PromoCode")]
        [EnumMember]
        WrongPromoCode = 43,



        [Description("Unknown Error")]
        [EnumMember]
        UnknownError = 128,

        [Description("System Error")]
        [EnumMember]
        SystemError = 256,
    }
}
