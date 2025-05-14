using System.Collections.Generic;
using Escrow.Api.Application.Common.Helpers;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Common.Constants
{
    public enum Language
    {
        English,
        Arabic
    }

    public static class AppMessages
    {
        private static readonly Dictionary<string, (string En, string Ar)> _messages = new()
        {
            // General
            ["Success"] = ("Operation completed successfully.", "تمت العملية بنجاح."),
            ["Failed"] = ("Operation failed. Please try again.", "فشلت العملية. الرجاء المحاولة مرة أخرى."),
            ["NotFound"] = ("Requested resource was not found.", "المورد المطلوب غير موجود."),
            ["Unauthorized"] = ("You are not authorized to perform this action.", "أنت غير مصرح لك بتنفيذ هذا الإجراء."),
            ["Forbidden"] = ("Access to this resource is forbidden.", "الوصول إلى هذا المورد ممنوع."),

            // User
            ["UserCreated"] = ("User created successfully.", "تم إنشاء المستخدم بنجاح."),
            ["UserUpdated"] = ("User updated successfully.", "تم تحديث المستخدم بنجاح."),
            ["UserDeleted"] = ("User deleted successfully.", "تم حذف المستخدم بنجاح."),
            ["UserNotFound"] = ("User not found.", "المستخدم غير موجود."),
            ["EmailAlreadyExists"] = ("A user with this email already exists.", "يوجد مستخدم بهذا البريد الإلكتروني بالفعل."),

            // Contract
            ["ContractCreated"] = ("Contract has been created.", "تم إنشاء العقد."),
            ["ContractUpdated"] = ("Contract updated successfully.", "تم تحديث العقد بنجاح."),
            ["ContractDeleted"] = ("Contract deleted successfully.", "تم حذف العقد بنجاح."),
            ["ContractNotFound"] = ("Contract not found.", "العقد غير موجود."),

            // Validation
            ["InvalidData"] = ("Provided data is invalid.", "البيانات المقدمة غير صالحة."),
            ["RequiredFieldsMissing"] = ("Required fields are missing.", "الحقول المطلوبة مفقودة."),
            ["CommissionRateGreaterThanZero"] = ("Commission rate must be greater than zero.", "يجب أن تكون نسبة العمولة أكبر من صفر."),
            ["TransactionTypeRequired"] = ("Transaction type is required.", "نوع المعاملة مطلوب."),
            ["TaxRateCannotBeNegative"] = ("Tax rate cannot be negative.", "لا يمكن أن تكون نسبة الضريبة سالبة."),

            // Auth
            ["LoginSuccessful"] = ("Login successful.", "تم تسجيل الدخول بنجاح."),
            ["InvalidCredentials"] = ("Invalid email or password.", "البريد الإلكتروني أو كلمة المرور غير صحيحة."),
            ["LoginFailed"] = ("Login failed. Please check your credentials.", "فشل تسجيل الدخول. يرجى التحقق من بيانات الاعتماد الخاصة بك."),

            // Admin Auth & Account
            ["AdminNotFound"] = ("Admin not found.", "المسؤول غير موجود."),
            ["IncorrectOldPassword"] = ("Incorrect old password.", "كلمة المرور القديمة غير صحيحة."),
            ["PasswordChangedSuccessfully"] = ("Password changed successfully.", "تم تغيير كلمة المرور بنجاح."),
            ["OtpSentSuccessfully"] = ("OTP sent to registered email.", "تم إرسال OTP إلى البريد الإلكتروني المسجل."),
            ["OtpInvalidOrExpired"] = ("Invalid or expired OTP.", "OTP غير صالح أو منتهي الصلاحية."),
            ["OtpVerified"] = ("OTP verified successfully.", "تم التحقق من OTP بنجاح."),
            ["OtpVerificationRequired"] = ("OTP verification is required.", "يتطلب التحقق من OTP."),
            ["PasswordResetSuccessfully"] = ("Password has been reset.", "تم إعادة تعيين كلمة المرور."),
            ["AdminUpdated"] = ("Admin updated successfully.", "تم تحديث المسؤول بنجاح."),
            ["RoleMismatch"] = ("Role mismatch. Cannot update user.", "تعارض في الدور. لا يمكن تحديث المستخدم."),
            ["OldAndNew"] = ("Old password and new password are required.", "كلمة المرور القديمة والجديدة مطلوبة."),
            ["PasswordMismatch"] = ("New password and confirm password do not match.", "كلمة المرور الجديدة وكلمة المرور المؤكدة لا تتطابق."),
            ["OldPasswordIncorrect"] = ("Old password is incorrect.", "كلمة المرور القديمة غير صحيحة."),
            ["NewPasswordCannotTheSameasOldPassword"] = ("New password cannot be the same as the old password.", "لا يمكن أن تكون كلمة المرور الجديدة هي نفسها كلمة المرور القديمة."),
            ["EmailNotFound"] = ("Email not found.", "البريد الإلكتروني غير موجود."),
            ["UserDoesNotHaveValidEmailaddress"] = ("User does not have a valid email address.", "المستخدم ليس لديه عنوان بريد إلكتروني صالح."),
            ["EmailAndOtpRequired"] = ("Email and OTP are required.", "البريد الإلكتروني وOTP مطلوبان."),
            ["StatusUpdated"] = ("Status Updated Successfully.", "تم تحديث الحالة بنجاح."),
            ["Deleted"] = ("Deleted successfully.", "تم الحذف بنجاح."),
            ["ValidadminId"] = ("Valid Admin ID is required.", "يجب أن يكون معرف المسؤول صالحًا."),
            ["AdminDetailRetrieved"] = ("Admin details retrieved successfully.", "تم استرجاع تفاصيل المسؤول بنجاح."),
            ["EmailAndPasswordRequired"] = ("Email and Password are required.", "البريد الإلكتروني وكلمة المرور مطلوبان."),
            ["ListingFetched"] = ("Listings fetched successfully.", "تم جلب القوائم بنجاح."),
            ["CommissionDataRetrieved"] = ("Admin commission data retrieved.", "تم استرجاع بيانات عمولة المسؤول."),
            ["ContractWorth"] = ("Contracts worth fetched successfully.", "تم جلب عقود بقيمة بنجاح."),
            ["EscrowAmountFetch"] = ("Escrow amount fetched successfully.", "تم جلب مبلغ الأمان بنجاح."),
            ["UserCount"] = ("User count fetched successfully.", "تم جلب عدد المستخدمين بنجاح."),
            ["Transactionnotfound"] = ("Transaction not found.", "المعاملة غير موجودة."),
            ["InvalidAction"] = ("Invalid admin action.", "إجراء المسؤول غير صالح."),
            ["Transactionverified"] = ("Transaction verified successfully.", "تم التحقق من المعاملة بنجاح."),
            ["AMLsettingscreated"] = ("AML settings created successfully.", "تم إنشاء إعدادات مكافحة غسل الأموال بنجاح."),
            ["InvalidOtp"] = ("OTP Not Valid", "OTP غير صالح."),
            ["InvalidMobileNumber"] = ("Mobile Number Not Valid.", "رقم الهاتف المحمول غير صالح."),
            ["BankDetailsNotFound"] = ("Bank Details Not Found.", "لم يتم العثور على تفاصيل البنك."),
            ["Commissionratemustbegreaterthanzero"] = ("Commission rate must be greater than zero.", "يجب أن تكون نسبة العمولة أكبر من صفر."),
            ["Transactiontypeisrequired"] = ("Transaction type is required.", "نوع المعاملة مطلوب."),
            ["Taxratecannotbenegative"] = ("Tax rate cannot be negative.", "لا يمكن أن تكون نسبة الضريبة سالبة."),
            ["Commissioncreated"] = ("Commission created successfully.", "تم إنشاء العمولة بنجاح."),
            ["Commissionupdated"] = ("Commission updated successfully.", "تم تحديث العمولة بنجاح."),
            ["CommissionDataNotFound"] = ("No commission data found.", "لم يتم العثور على بيانات العمولة."),
            ["Commissioninformationnotfound"] = ("Commission information not found.", "لم يتم العثور على معلومات العمولة."),
            ["Unabletosavemessagetothedatabase"] = ("Unable to save message to the database.", "غير قادر على حفظ الرسالة في قاعدة البيانات."),
            ["Messagesent"] = ("Message sent successfully.", "تم إرسال الرسالة بنجاح."),
            ["Contractactivated"] = ("Contract activated successfully.", "تم تفعيل العقد بنجاح."),
            ["Contractdeactivated"] = ("Contract deactivated successfully.", "تم إلغاء تفعيل العقد بنجاح."),
            ["SpecifiedContract"] = ("The specified contract does not exist.", "العقد المحدد غير موجود."),
            ["MonthlyContractlimitexceeded"] = ("You have exceeded your monthly contract creation limit.", "لقد تجاوزت حد إنشاء العقود الشهري."),
            ["BuyerandSellermobilenumberscannotbethesame"] = ("Buyer and Seller mobile numbers cannot be the same.", "لا يمكن أن تكون أرقام هواتف المشتري والبائع هي نفسها."),
            ["Invitationnotfound"] = ("Invitation not found.", "الدعوة غير موجودة."),
            ["Contractretrievedsuccessfully"] = ("Contract retrieved successfully.", "تم استرجاع العقد بنجاح."),
            ["MilestoneInvaliddetails"] = ("Invalid request data. Milestone details are required.", "بيانات الطلب غير صالحة. تفاصيل المعلم مطلوبة."),
            ["MilestonescreatedAndupdated"] = ("Milestones created/updated successfully.", "تم إنشاء/تحديث المعالم بنجاح."),
            ["Milestonesupdated"] = ("Milestones updated successfully.", "تم تحديث المعالم بنجاح."),
            ["Relatedcontractnotfound"] = ("Related contract not found.", "العقد المرتبط غير موجود."),
            ["InvalidFeesPaidBy"] = ("Invalid FeesPaidBy value.", "قيمة المدفوعات للمصاريف غير صالحة."),
            ["BuyerDeductionexceeds"] = ("Deduction exceeds available buyer balance.", "التخفيض يتجاوز الرصيد المتاح للمشتري."),
            ["SellerDeductionexceeds"] = ("Deduction exceeds available buyer balance.", "التخفيض يتجاوز الرصيد المتاح للبائع."),
            ["BuyerAndSellerNotFound"] = ("Buyer or Seller not found.", "المشتري أو البائع غير موجود."),
            ["InvaliduserID"] = ("Invalid user ID.", "معرف المستخدم غير صالح."),
            ["UnauthorizedToReviewContract"] = ("You are not authorized to review this contract.", "أنت غير مصرح لك بمراجعة هذا العقد."),
            ["ReviewCreated"] = ("Review created successfully and contract marked as completed.", "تم إنشاء المراجعة بنجاح وتم وضع العقد كمكتمل."),
            ["Reviewnotfound"] = ("Review not found.", "المراجعة غير موجودة."),
            ["UnauthorizedToUpdateReview"] = ("You are not authorized to update this review.", "أنت غير مصرح لك بتحديث هذه المراجعة."),
            ["Reviewupdated"] = ("Review updated successfully.", "تم تحديث المراجعة بنجاح."),
            ["Reviewretrieved"] = ("Reviews retrieved successfully.", "تم استرجاع المراجعات بنجاح."),

            // Customer Support Messages
            ["Customernotfound"] = ("Customer not found.", "العميل غير موجود."),
            ["Customerstatus"] = ("Customer status updated successfully.", "تم تحديث حالة العميل بنجاح."),
            ["EmailOrMobileNumberalreadyexists"] = ("Email Or MobileNumber already exists.", "البريد الإلكتروني أو رقم الهاتف المحمول موجود بالفعل."),
            ["Customercreated"] = ("Customer created successfully.", "تم إنشاء العميل بنجاح."),
            ["Customerdeleted"] = ("Customer deleted successfully.", "تم حذف العميل بنجاح."),
            ["Customerupdated"] = ("Customer updated successfully.", "تم تحديث العميل بنجاح."),
            ["EmailExists"] = ("Email already exists.", "البريد الإلكتروني موجود بالفعل."),
            ["ContactMessageSent"] = ("Contact message sent successfully.", "تم إرسال رسالة الاتصال بنجاح."),

        };


        public static string Get(string key, Language language)
        {
            if (_messages.TryGetValue(key, out var value))
            {
                return language == Language.Arabic ? value.Ar : value.En;
            }

            // Fallback to key name if not found
            return key;
        }
       
    }
    public static class HttpContextExtensions
    {
        public static Language GetCurrentLanguage(this HttpContext context)
        {
            // Ensure that context is not null before proceeding
            if (context == null)
            {
                // Optionally, log this or handle the scenario appropriately
                return Language.English;  // Default to English if HttpContext is null
            }

            var languageValue = context.Items[LanguageMiddleware.LanguageKey];
            return languageValue as Language? ?? Language.English;  // Default to English if value is null
        }
    }

}

























//public static class AppMessages
//{
//    // General Messages
//    public const string Success = "Operation completed successfully.";
//    public const string Failed = "Operation failed. Please try again.";
//    public const string NotFound = "Requested resource was not found.";
//    public const string Unauthorized = "You are not authorized to perform this action.";
//    public const string Forbidden = "Access to this resource is forbidden.";

//    // User Messages
//    public const string UserCreated = "User created successfully.";
//    public const string UserUpdated = "User updated successfully.";
//    public const string UserDeleted = "User deleted successfully.";
//    public const string UserNotFound = "User not found.";
//    public const string EmailAlreadyExists = "A user with this email already exists.";

//    // Contract Messages
//    public const string ContractCreated = "Contract has been created.";
//    public const string ContractUpdated = "Contract updated successfully.";
//    public const string ContractDeleted = "Contract deleted successfully.";
//    public const string ContractNotFound = "Contract not found.";

//    // Validation Messages
//    public const string InvalidData = "Provided data is invalid.";
//    public const string RequiredFieldsMissing = "Required fields are missing.";

//    // Auth Messages
//    public const string LoginSuccessful = "Login successful.";
//    public const string InvalidCredentials = "Invalid email or password.";
//    public const string LoginFailed = "Login failed. Please check your credentials.";

//    // File Upload Messages
//    public const string FileUploadSuccess = "File uploaded successfully.";
//    public const string FileUploadFailed = "File upload failed.";
//    public const string InvalidFileType = "Invalid file type.";


//    // Admin Auth & Account Messages
//    public const string AdminNotFound = "Admin not found.";
//    public const string IncorrectOldPassword = "Incorrect old password.";
//    public const string PasswordChangedSuccessfully = "Password changed successfully.";
//    public const string OtpSentSuccessfully = "OTP sent to registered email.";
//    public const string OtpInvalidOrExpired = "Invalid or expired OTP.";
//    public const string OtpVerified = "OTP verified successfully.";
//    public const string OtpVerificationRequired = "OTP verification is required.";
//    public const string PasswordResetSuccessfully = "Password has been reset.";
//    public const string AdminUpdated = "Admin updated successfully.";
//    public const string RoleMismatch = "Role mismatch. Cannot update user.";
//    public const string OldAndNew = "Old password and new password are required.";
//    public const string PasswordMismatch = "New password and confirm password do not match.";
//    public const string OldPasswordIncorrect = "Old password is incorrect.";
//    public const string NewPasswordCannotTheSameasOldPassword = "New password cannot be the same as the old password.";
//    public const string EmailNotFound = "Email not found.";
//    public const string UserDoesNotHaveValidEmailaddress = "User does not have a valid email address.";
//    public const string EmailAndOtpRequired = "Email and OTP are required.";
//    public const string StatusUpdated = "Status Updated Successfully.";
//    public const string Deleted = "Deleted successfully.";
//    public const string ValidadminId = "Valid Admin ID is required.";
//    public const string AdminDetailRetrieved = "Admin details retrieved successfully.";
//    public const string EmailAndPasswordRequired = "Email and Password are required.";
//    public const string ListingFetched = "Listings fetched successfully.";
//    public const string CommissionDataRetrieved = "Admin commission data retrieved.";
//    public const string ContractWorth = "Contracts worth fetched successfully.";
//    public const string EscrowAmountFetch = "Escrow amount fetched successfully.";
//    public const string UserCount = "User count fetched successfully.";
//    public const string Transactionnotfound = "Transaction not found.";
//    public const string InvalidAction = "Invalid admin action.";
//    public const string Transactionverified = "Transaction verified successfully.";
//    public const string AMLsettingscreated = "AML settings created successfully.";
//    public const string InvalidOtp = "OTP Not Valid";
//    public const string InvalidMobileNumber = "Mobile Number Not Valid.";
//    public const string BankDetailsNotFound = "Bank Details Not Found.";
//    public const string Commissionratemustbegreaterthanzero = "Commission rate must be greater than zero.";
//    public const string Transactiontypeisrequired = "Transaction type is required.";
//    public const string Taxratecannotbenegative = "Tax rate cannot be negative.";
//    public const string Commissioncreated = "Commission created successfully.";
//    public const string Commissionupdated = "Commission updated successfully.";
//    public const string CommissionDataNotFound = "No commission data found.";
//    public const string Commissioninformationnotfound = "Commission information not found.";
//    public const string Unabletosavemessagetothedatabase = "Unable to save message to the database.";
//    public const string Messagesent = "Message sent successfully.";
//    public const string Contractactivated = "Contract activated successfully.";
//    public const string Contractdeactivated = "Contract deactivated successfully.";
//    public const string SpecifiedContract = "The specified contract does not exist.";
//    public const string MonthlyContractlimitexceeded = "You have exceeded your monthly contract creation limit.";
//    public const string BuyerandSellermobilenumberscannotbethesame = "Buyer and Seller mobile numbers cannot be the same.";
//    public const string Invitationnotfound = "Invitation not found.";
//    public const string Contractretrievedsuccessfully = "Contract retrieved successfully.";
//    public const string MilestoneInvaliddetails = "Invalid request data. Milestone details are required.";
//    public const string MilestonescreatedAndupdated = "Milestones created/updated successfully.";
//    public const string Milestonesupdated = "Milestones updated successfully.";
//    public const string Relatedcontractnotfound = "Related contract not found.";
//    public const string InvalidFeesPaidBy = "Invalid FeesPaidBy value.";
//    public const string BuyerDeductionexceeds = "Deduction exceeds available buyer balance.";
//    public const string SellerDeductionexceeds = "Deduction exceeds available buyer balance.";
//    public const string BuyerAndSellerNotFound = "Buyer or Seller not found.";
//    public const string InvaliduserID = "Invalid user ID.";
//    public const string UnauthorizedToReviewContract = "You are not authorized to review this contract.";
//    public const string ReviewCreated = "Review created successfully and contract marked as completed.";
//    public const string Reviewnotfound = "Review not found.";
//    public const string UnauthorizedToUpdateReview = "You are not authorized to update this review.";
//    public const string Reviewupdated = "Review updated successfully.";
//    public const string Reviewretrieved = "Reviews retrieved successfully.";

//    /// <summary>
//    /// Customer Support Messages
//    /// </summary>
//    public const string Customernotfound = "Customer not found.";
//    public const string Customerstatus = "Customer status updated successfully.";
//    public const string EmailOrMobileNumberalreadyexists = "Email Or MobileNumber already exists.";
//    public const string Customercreated = "Customer created successfully.";
//    public const string Customerdeleted = "Customer deleted successfully.";
//    public const string Customerupdated = "Customer  successfully.";
//    public const string EmailExists = "Email already exists.";

//    ///






//}
