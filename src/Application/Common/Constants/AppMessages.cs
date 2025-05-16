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

            ["ArbitratorAssigned"] = ("Arbitrator assigned successfully.", "تم تعيين المحكم بنجاح."),
            ["Disputenotfound"] = ("Dispute not found.", "النزاع غير موجود."),
            ["ContractsRetrieved"] = ("Contracts retrieved successfully.", "تم استرجاع العقود بنجاح."),


            ["UserNotAuthenticated"] = ("User is not authenticated.", "المستخدم غير مصادق عليه."),
            ["ContractNotFound"] = ("Contract not found.", "العقد غير موجود."),
            ["DisputeInvalidStatus"] = ("Dispute can only be raised when contract is in 'Escrow' status.", "يمكن رفع النزاع فقط عندما يكون العقد في حالة 'عهدة'."),
            ["EscrowTimestampUnavailable"] = ("Escrow timestamp not available to verify dispute window.", "طابع الوقت للعهدة غير متوفر للتحقق من نافذة النزاع."),
            ["DisputeWindowExpired"] = ("Dispute can only be raised within 48 hours of contract entering 'Escrow'.", "يمكن رفع النزاع فقط خلال 48 ساعة من دخول العقد حالة 'عهدة'."),
            ["DisputeCreated"] = ("Dispute created and contract status updated successfully.", "تم إنشاء النزاع وتحديث حالة العقد بنجاح."),


            ["InvalidReleaseRecipient"] = ("Invalid release recipient. Must be 'Buyer' or 'Seller'.", "المستلم غير صالح. يجب أن يكون 'المشتري' أو 'البائع'."),
            ["EscrowDecisionApplied"] = ("Escrow decision applied successfully.", "تم تطبيق قرار العهدة بنجاح."),

            ["EscrowDecisionAppliedFormatted"] = ("Escrow decision applied successfully. {0} released to {1}.", "تم تطبيق قرار العهدة بنجاح. تم تحرير {0} إلى {1}."),





            ["DisputeAlreadyResolved"] = ("Dispute has already been resolved.", "تم حل النزاع بالفعل."),
            ["AssociatedContractNotFound"] = ("Associated contract not found.", "العقد المرتبط غير موجود."),
            ["ReleaseToRequired"] = ("ReleaseTo is required when resolving a dispute.", "مطلوب 'ReleaseTo' عند حل النزاع."),
            ["ReleaseAmountRequired"] = ("ReleaseAmount is required when resolving a dispute.", "مطلوب 'ReleaseAmount' عند حل النزاع."),
            ["DisputeStatusUpdatedSubject"] = ("Dispute Status Updated", "تم تحديث حالة النزاع"),
            ["DisputeStatusUpdatedBody"] = ("Dear {0},\n\nThe dispute for contract #{1} has been updated to '{2}'.\n\nThank you.", "عزيزي {0},\n\nتم تحديث النزاع للعقد رقم #{1} إلى الحالة '{2}'.\n\nشكراً لك."),
            ["NotifyUserFailed"] = ("Failed to notify {0}: {1}", "فشل في إعلام {0}: {1}"),
            ["DisputeStatusUpdatedSuccess"] = ("Dispute status successfully updated to '{0}'.", "تم تحديث حالة النزاع إلى '{0}' بنجاح."),
            ["WithNotificationErrors"] = ("However, the following notification errors occurred:", "ومع ذلك، حدثت أخطاء إشعار التالية:"),





            ["EmailTemplateNotFound"] = ("Email Template not found.", "قالب البريد الإلكتروني غير موجود."),
            ["EmailTemplateUpdatedSuccessfully"] = ("Email Template updated successfully.", "تم تحديث قالب البريد الإلكتروني بنجاح."),
            ["EmailTemplateRetrievedSuccessfully"] = ("Email template retrieved successfully.", "تم استرجاع قالب البريد الإلكتروني بنجاح."),


            ["ManualNotificationCreatedAndSent"] = ("Manual notification created and sent successfully.", "تم إنشاء الإشعار اليدوي وإرساله بنجاح."),
            ["InvalidRequestData"] = ("Invalid request data.", "بيانات الطلب غير صالحة."),

            ["ManualNotificationCreatedSuccessfully"] = ("Manual notification created and sent successfully.", "تم إنشاء الإشعار اليدوي وإرساله بنجاح."),
            ["ManualNotificationValidationFailed"] = ("Invalid request data.", "بيانات الطلب غير صالحة."),
            ["NotificationCreatedSuccessfully"] = ("Notification created successfully (including admin).", "تم إنشاء الإشعار بنجاح (بما في ذلك المسؤول)."),
            ["NotificationUnauthorizedUser"] = ("Unauthorized user context.", "سياق المستخدم غير مصرح به."),
            ["NotificationContractIdInvalid"] = ("Invalid ContractId.", "معرّف العقد غير صالح."),
            ["NotificationPhoneNumbersRequired"] = ("Phone numbers are required.", "أرقام الهواتف مطلوبة."),
            ["NotificationServerError"] = ("Unexpected Server Error.", "خطأ غير متوقع في الخادم."),


            ["PhoneNumberRequired"] = ("Phone numbers are required.", "أرقام الهاتف مطلوبة."),
            ["InvalidContractId"] = ("Invalid contract ID.", "معرّف العقد غير صالح."),
            ["NotificationSentSuccessfully"] = ("Notification sent successfully.", "تم إرسال الإشعار بنجاح."),

            ["InvalidNotificationId"] = ("Invalid notification ID. ID must be greater than zero.", "معرّف الإشعار غير صالح. يجب أن يكون أكبر من صفر."),
            ["NotificationNotFound"] = ("Notification not found.", "الإشعار غير موجود."),
            ["NotificationDeletedSuccessfully"] = ("Notification deleted successfully.", "تم حذف الإشعار بنجاح."),
            ["NoNotificationsFound"] = ("No notifications found for this user.", "لا توجد إشعارات لهذا المستخدم."),
            ["AllNotificationsDeletedSuccessfully"] = ("All notifications deleted successfully.", "تم حذف جميع الإشعارات بنجاح."),



            ["InvalidNotificationId"] = ("Invalid notification ID. ID must be greater than zero.", "معرف الإشعار غير صالح. يجب أن يكون أكبر من الصفر."),
            ["NotificationNotFound"] = ("Notification not found.", "الإشعار غير موجود."),
            ["NotificationDeletedSuccessfully"] = ("Notification deleted successfully.", "تم حذف الإشعار بنجاح."),
            ["NoNotificationsFound"] = ("No notifications found for this user.", "لا توجد إشعارات لهذا المستخدم."),
            ["AllNotificationsDeletedSuccessfully"] = ("All notifications deleted successfully.", "تم حذف جميع الإشعارات بنجاح."),


            ["NotificationNotFound"] = ("Notification not found.", "الإشعار غير موجود."),
            ["NotificationMarkedAsReadSuccessfully"] = ("Notification marked as read successfully.", "تم وضع الإشعار كمقروء بنجاح."),
            ["NotificationMarkedAsUnreadSuccessfully"] = ("Notification marked as unread successfully.", "تم وضع الإشعار كغير مقروء بنجاح."),
            ["NoNotificationsFoundToUpdate"] = ("No notifications found to update.", "لا توجد إشعارات للتحديث."),
            ["AllNotificationsMarkedAsReadSuccessfully"] = ("All notifications marked as read successfully.", "تم وضع جميع الإشعارات كمقروءة بنجاح."),
            ["AllNotificationsMarkedAsUnreadSuccessfully"] = ("All notifications marked as unread successfully.", "تم وضع جميع الإشعارات كغير مقروءة بنجاح."),

            ["InvalidInputIdsMustBeGreaterThanZero"] = ("Invalid input. IDs must be greater than zero.", "المدخلات غير صالحة. يجب أن تكون المعرفات أكبر من الصفر."),
            ["NotificationNotFound"] = ("Notification not found.", "الإشعار غير موجود."),
            ["NotificationUpdatedSuccessfully"] = ("Notification updated successfully.", "تم تحديث الإشعار بنجاح."),

            ["InvalidInputIdsMustBeGreaterThanZero"] = ("Invalid input. IDs must be greater than zero.", "المدخلات غير صالحة. يجب أن تكون المعرفات أكبر من الصفر."),
            ["NotificationNotFound"] = ("Notification not found.", "الإشعار غير موجود."),
            ["NotificationUpdatedSuccessfully"] = ("Notification updated successfully.", "تم تحديث الإشعار بنجاح."),
            ["FailedToRetrieveNotifications"] = ("Failed to retrieve notifications.", "فشل في استرداد الإشعارات."),
            ["NotificationsRetrievedSuccessfully"] = ("Notifications retrieved successfully. Unread count: {0}", "تم استرداد الإشعارات بنجاح. عدد الغير مقروءة: {0}"),

            ["ManualNotificationsRetrievedSuccessfully"] = ("Manual notifications retrieved successfully.", "تم استرداد الإشعارات اليدوية بنجاح."),

            ["NotificationNotFound"] = ("Notification not found.", "الإشعار غير موجود."),
            ["NotificationRetrievedSuccessfully"] = ("Notification retrieved successfully.", "تم استرداد الإشعار بنجاح."),

            ["PermissionsAssignedSuccessfully"] = ("Permissions assigned successfully", "تم تعيين الأذونات بنجاح"),

            ["RolesFetchedSuccessfully"] = ("Roles fetched successfully.", "تم جلب الأدوار بنجاح."),
            ["NoRolesFound"] = ("No roles found.", "لم يتم العثور على أدوار."),

            ["MenusFetchedSuccessfully"] = ("Menus fetched successfully.", "تم جلب القوائم بنجاح."),
            ["NoMenusFound"] = ("No menus found.", "لم يتم العثور على قوائم."),


            ["PermissionsFetchedSuccessfully"] = ("Permissions fetched successfully.", "تم جلب الأذونات بنجاح."),
            ["NoPermissionsFound"] = ("No permissions found.", "لم يتم العثور على أذونات."),

            ["NoPagesProvided"] = ("No pages provided for update.", "لم يتم تقديم صفحات للتحديث."),
            ["NoValidPageIds"] = ("No valid page IDs provided.", "لم يتم تقديم معرفات صفحات صحيحة."),
            ["NoMatchingPagesFound"] = ("No matching pages found for update.", "لم يتم العثور على صفحات مطابقة للتحديث."),
            ["PagesUpdatedSuccessfully"] = ("Pages updated successfully.", "تم تحديث الصفحات بنجاح."),

            ["ConfigurationUpdatedSuccessfully"] = ("Configuration updated successfully.", "تم تحديث الإعدادات بنجاح."),
            ["ConfigurationCreatedSuccessfully"] = ("Configuration created successfully.", "تم إنشاء الإعدادات بنجاح."),

            ["ConfigurationUpdatedSuccessfully"] = ("Configuration updated successfully.", "تم تحديث الإعدادات بنجاح."),
            ["ConfigurationCreatedSuccessfully"] = ("Configuration created successfully.", "تم إنشاء الإعدادات بنجاح."),


            ["TeamNameRequired"] = ("Team name is required.", "اسم الفريق مطلوب."),
            ["RoleTypeRequired"] = ("Role type is required.", "نوع الدور مطلوب."),
            ["Unauthorized"] = ("Unauthorized request.", "طلب غير مصرح به."),
            ["EmailAndPhoneMismatch"] = ("A user with this email and another with this phone number already exist. Please verify the details.",
                             "يوجد مستخدم بهذا البريد الإلكتروني وآخر برقم الهاتف هذا. يرجى التحقق من التفاصيل."),
            ["EmailAlreadyExists"] = ("A user with this email already exists.", "يوجد مستخدم بهذا البريد الإلكتروني بالفعل."),
            ["PhoneAlreadyExists"] = ("A user with this phone number already exists.", "يوجد مستخدم بهذا رقم الهاتف بالفعل."),
            ["UserIdRetrievalFailed"] = ("Failed to retrieve UserId.", "فشل في الحصول على معرف المستخدم."),
            ["AlreadyTeamMember"] = ("This user is already a team member.", "هذا المستخدم عضو في الفريق بالفعل."),
            ["TeamSuccess"] = ("Team member created successfully.", "تم إنشاء عضو الفريق بنجاح."),

            ["TeamDeletedSuccessfully"] = ("Team deleted successfully.", "تم حذف الفريق بنجاح."),
            ["TeamNotFoundOrUnauthorized"] = ("Team not found or you do not have permission to delete it.", "لم يتم العثور على الفريق أو لا تملك صلاحية الحذف."),

            ["UserIdAndTeamIdRequired"] = ("User ID and Team ID are required.", "مطلوب معرف المستخدم ومعرف الفريق."),
            ["UserNotFound"] = ("User not found.", "المستخدم غير موجود."),
            ["TeamNotFound"] = ("Team member not found for the given Team ID.", "لم يتم العثور على عضو الفريق بمعرف الفريق المحدد."),
            ["TeamUpdatedSuccessfully"] = ("Team updated successfully.", "تم تحديث الفريق بنجاح."),

            ["TeamNotFound"] = ("Team not found.", "الفريق غير موجود."),
            ["FailedToUpdateTeamStatus"] = ("Failed to update team status.", "فشل في تحديث حالة الفريق."),
            ["TeamStatusUpdatedSuccessfully"] = ("Team status updated successfully.", "تم تحديث حالة الفريق بنجاح."),


            ["UserNotAuthenticated"] = ("User is not authenticated.", "المستخدم غير مصرح به."),
            ["ContractNotFound"] = ("Contract not found.", "العقد غير موجود."),
            ["BuyerOrSellerNotFound"] = ("Buyer or Seller not found.", "المشتري أو البائع غير موجود."),
            ["UnauthorizedTransaction"] = ("Unauthorized transaction.", "عملية غير مصرح بها."),
            ["TransactionCreatedEmailSubject"] = ("Transaction Created", "تم إنشاء المعاملة"),
            ["TransactionCreatedEmailBody"] = ("Dear {0},\n\nA new transaction has been created for your contract (ID: {1}).", "عزيزي {0},\n\nتم إنشاء معاملة جديدة لعقدك (رقم: {1})."),
            ["FailedToNotifyUser"] = ("Failed to notify user", "فشل في إخطار المستخدم"),
            ["TransactionCreatedSuccessfully"] = ("Transaction created successfully.", "تم إنشاء المعاملة بنجاح."),
            ["WithWarnings"] = ("However, the following issues occurred:", "ومع ذلك، حدثت المشكلات التالية:"),


            ["UserNotAuthenticated"] = ("User is not authenticated.", "المستخدم غير مصادق عليه."),
            ["InvalidUserId"] = ("Invalid user ID.", "معرف المستخدم غير صالح."),
            ["UserRoleNotFound"] = ("User role not found.", "دور المستخدم غير موجود."),
            ["TransactionNotFound"] = ("Transaction not found.", "لم يتم العثور على المعاملة."),
            ["TransactionRetrievedSuccessfully"] = ("Transaction retrieved successfully.", "تم استرداد المعاملة بنجاح."),


            ["TransactionNotFound"] = ("Transaction not found.", "لم يتم العثور على المعاملة."),
            ["TransactionRetrievedSuccessfully"] = ("Transaction retrieved successfully.", "تم استرداد المعاملة بنجاح."),

            // Add the missing ones you used in the handler:
            ["PageNumberGreaterThanZero"] = ("Page number must be greater than zero.", "يجب أن يكون رقم الصفحة أكبر من صفر."),
            ["PageSizeGreaterThanZero"] = ("Page size must be greater than zero.", "يجب أن يكون حجم الصفحة أكبر من صفر."),
            ["InvalidRequest"] = ("Invalid request.", "طلب غير صالح."),


            ["UserDetailsNotFound"] = ("User details not found.", "لم يتم العثور على تفاصيل المستخدم."),
            ["UserDeletedSuccessfully"] = ("User deleted successfully.", "تم حذف المستخدم بنجاح."),

            ["UserUpdatedSuccessfully"] = ("User updated successfully.", "تم تحديث بيانات المستخدم بنجاح."),

            ["DeviceTokenUpdatedSuccessfully"] = ("Device token updated successfully.", "تم تحديث رمز الجهاز بنجاح."),

            ["InvalidRequestData"] = ("Invalid request data.", "بيانات الطلب غير صالحة."),
            ["UserNotFound"] = ("User not found.", "المستخدم غير موجود."),
            ["InvalidPhoneNumberFormat"] = ("Invalid phone number format.", "تنسيق رقم الهاتف غير صالح."),
            ["OtpSentToMobile"] = ("OTP has been sent to the mobile number.", "تم إرسال رمز التحقق إلى رقم الهاتف."),


            ["InvalidRequestData"] = ("Invalid request data.", "بيانات الطلب غير صالحة."),
            ["InvalidPhoneNumberFormat"] = ("Invalid phone number format.", "تنسيق رقم الهاتف غير صالح."),
            ["OtpSentToMobile"] = ("OTP has been sent to the mobile number.", "تم إرسال رمز التحقق إلى رقم الهاتف."),

            ["NotificationStatusUpdated"] = ("Notification status updated successfully.", "تم تحديث حالة الإشعار بنجاح."),


            ["LoginFailed"] = ("Login failed. Please check your credentials.", "فشل تسجيل الدخول. يرجى التحقق من بيانات الاعتماد الخاصة بك."),
            ["ForgotPasswordFailed"] = ("Failed to initiate password reset.", "فشل في بدء إعادة تعيين كلمة المرور."),
            ["VerifyOTPFailed"] = ("OTP verification failed.", "فشل التحقق من رمز التحقق."),
            ["ResetPasswordFailed"] = ("Failed to reset password.", "فشل في إعادة تعيين كلمة المرور."),
            ["ChangePasswordFailed"] = ("Failed to change password.", "فشل في تغيير كلمة المرور."),
            ["GetAdminDetailsFailed"] = ("Failed to retrieve admin details.", "فشل في استرجاع تفاصيل المسؤول."),
            ["UpdateDetailsFailed"] = ("Failed to update admin details.", "فشل في تحديث تفاصيل المسؤول."),
            ["UpdateDetailsSuccess"] = ("Admin details updated successfully.", "تم تحديث تفاصيل المسؤول بنجاح."),
            ["GetAdminListingsFailed"] = ("Failed to fetch admin listings.", "فشل في جلب قائمة المسؤولين."),
            ["NotificationStatusUpdated"] = ("Notification status updated successfully.", "تم تحديث حالة الإشعار بنجاح."),


            ["SubAdminCreated"] = ("Sub-admin created successfully.", "تم إنشاء المشرف الفرعي بنجاح."),
            ["SubAdminCreationFailed"] = ("Failed to create sub-admin.", "فشل في إنشاء المشرف الفرعي."),

            ["SubAdminDeleted"] = ("Sub-admin deleted successfully.", "تم حذف المشرف الفرعي بنجاح."),
            ["SubAdminDeletionFailed"] = ("Failed to delete sub-admin.", "فشل في حذف المشرف الفرعي."),

            ["SubAdminStatusUpdated"] = ("Sub-admin status updated successfully.", "تم تحديث حالة المشرف الفرعي بنجاح."),
            ["SubAdminStatusUpdateFailed"] = ("Failed to update sub-admin status.", "فشل في تحديث حالة المشرف الفرعي."),

            ["DeleteSuccess"] = ("Deleted successfully.", "تم الحذف بنجاح."),

            ["StatusUpdateSuccess"] = ("Status updated successfully.", "تم تحديث الحالة بنجاح."),
            ["StatusUpdateFailed"] = ("Failed to update status.", "فشل في تحديث الحالة."),

            ["NoFlaggedTransactionsFound"] = ("No flagged transactions found.", "لم يتم العثور على معاملات مميزة."),
            ["FlaggedTransactionsRetrieved"] = ("Flagged transactions retrieved successfully.", "تم استرجاع المعاملات المميزة بنجاح."),

            ["NoAMLSettingsFound"] = ("No AML settings found.", "لم يتم العثور على إعدادات مكافحة غسيل الأموال."),
            ["AMLSettingsUpdated"] = ("AML settings updated successfully.", "تم تحديث إعدادات مكافحة غسيل الأموال بنجاح."),
            ["TransactionVerificationUpdated"] = ("Transaction verification status updated.", "تم تحديث حالة التحقق من المعاملة."),
            ["NoAMLNotificationsFound"] = ("No AML notifications found.", "لم يتم العثور على إشعارات مكافحة غسيل الأموال."),
            ["AMLNotificationsRetrieved"] = ("AML notifications retrieved successfully.", "تم استرجاع إشعارات مكافحة غسيل الأموال بنجاح."),

            ["BankDetailsRetrieved"] = ("Bank details retrieved successfully.", "تم استرجاع تفاصيل البنك بنجاح."),
            ["BankDetailCreated"] = ("Bank detail created successfully.", "تم إنشاء تفاصيل البنك بنجاح."),
            ["BankDetailUpdated"] = ("Bank detail updated successfully.", "تم تحديث تفاصيل البنك بنجاح."),
            ["BankDetailDeleted"] = ("Bank detail deleted successfully.", "تم حذف تفاصيل البنك بنجاح."),

            ["CustomersRetrieved"] = ("Customers retrieved successfully.", "تم استرجاع العملاء بنجاح."),

            ["DisputeCreated"] = ("Dispute created and contract status updated successfully.", "تم إنشاء النزاع وتحديث حالة العقد بنجاح."),
            ["DisputeCreationFailed"] = ("Failed to create dispute.", "فشل في إنشاء النزاع."),


            ["DisputesRetrieved"] = ("Disputes retrieved successfully.", "تم استرجاع النزاعات بنجاح."),
            ["NoDisputesFound"] = ("No disputes found.", "لم يتم العثور على أي نزاعات."),

            ["EmailTemplatesRetrieved"] = ("Email templates retrieved successfully.", "تم استرجاع قوالب البريد الإلكتروني بنجاح."),
            ["NoEmailTemplatesFound"] = ("No email templates found.", "لم يتم العثور على أي قوالب بريد إلكتروني."),
            ["EmailTemplateRetrieved"] = ("Email template retrieved successfully.", "تم استرجاع قالب البريد الإلكتروني بنجاح."),
            ["EmailTemplateNotFound"] = ("Email template not found.", "لم يتم العثور على قالب البريد الإلكتروني."),

            ["NoDataFound"] = ("No data found.", "لم يتم العثور على بيانات."),
            ["AssignPermissionsFailed"] = ("Failed to assign permissions.", "فشل في تعيين الأذونات."),

            ["NoConfigurationsFound"] = ("No configurations found.", "لم يتم العثور على الإعدادات."),
            ["ConfigurationsRetrieved"] = ("Configurations retrieved successfully.", "تم استرجاع الإعدادات بنجاح."),
            ["ConfigurationUpdated"] = ("Configuration updated successfully.", "تم تحديث الإعداد بنجاح."),
            ["ConfigurationUpdateFailed"] = ("Failed to update configuration.", "فشل في تحديث الإعداد."),

            ["BankDetailsRetrieved"] = ("Bank details retrieved successfully.", "تم استرجاع تفاصيل البنك بنجاح."),
            ["BankDetailCreated"] = ("Bank detail created successfully.", "تم إنشاء تفاصيل البنك بنجاح."),
            ["BankDetailUpdated"] = ("Bank detail updated successfully.", "تم تحديث تفاصيل البنك بنجاح."),
            ["BankDetailDeleted"] = ("Bank detail deleted successfully.", "تم حذف تفاصيل البنك بنجاح."),

            ["NoCommissionDataFound"] = ("No commission data found.", "لم يتم العثور على بيانات العمولة."),
            ["CommissionDataRetrieved"] = ("Commission data retrieved successfully.", "تم استرجاع بيانات العمولة بنجاح."),
            ["CommissionUpsertFailed"] = ("Failed to add or update commission data.", "فشل في إضافة أو تحديث بيانات العمولة."),
            ["CommissionUpserted"] = ("Commission data added or updated successfully.", "تمت إضافة أو تحديث بيانات العمولة بنجاح."),

            ["InvalidContactRequest"] = ("Invalid contact request.", "طلب الاتصال غير صالح."),
            ["ContactMessageFailed"] = ("Failed to send contact message.", "فشل في إرسال رسالة الاتصال."),
            ["ContactMessageSent"] = ("Contact message sent successfully.", "تم إرسال رسالة الاتصال بنجاح."),
            ["NoContactMessagesFound"] = ("No contact messages found.", "لم يتم العثور على رسائل اتصال."),
            ["ContactMessagesRetrieved"] = ("Contact messages retrieved successfully.", "تم استرجاع رسائل الاتصال بنجاح."),

            ["ContractsRetrieved"] = ("Contracts retrieved successfully.", "تم استرجاع العقود بنجاح."),
            ["ContractUpdateFailed"] = ("Failed to update contract.", "فشل في تحديث العقد."),
            ["ContractStatusUpdated"] = ("Contract status updated successfully.", "تم تحديث حالة العقد بنجاح."),
            ["ContractDetailsUpdated"] = ("Contract details updated successfully.", "تم تحديث تفاصيل العقد بنجاح."),
            ["ContractCreated"] = ("Contract created successfully.", "تم إنشاء العقد بنجاح."),
            ["ContractNotFound"] = ("Contract not found.", "العقد غير موجود."),
            ["ContractRetrieved"] = ("Contract retrieved successfully.", "تم استرجاع العقد بنجاح."),
            ["AccessDenied"] = ("Access denied.", "تم رفض الوصول."),
            ["ContractModifyFailed"] = ("Failed to modify contract.", "فشل في تعديل العقد."),
            ["ContractModified"] = ("Contract modified successfully.", "تم تعديل العقد بنجاح."),

            ["InvalidContractId"] = ("Invalid contract ID.", "معرّف العقد غير صالح."),
            ["Success"] = ("Success.", "نجاح."),
            ["MilestoneDetailsRequired"] = ("Milestone details are required.", "تفاصيل المرحلة مطلوبة."),
            ["NoMilestonesCreatedOrUpdated"] = ("No milestones were created or updated.", "لم يتم إنشاء أو تحديث أي مراحل."),
            ["MilestonesCreatedSuccessfully"] = ("Milestones created successfully.", "تم إنشاء المراحل بنجاح."),
            ["UnexpectedServerError"] = ("Unexpected server error occurred.", "حدث خطأ غير متوقع في الخادم."),
            ["MilestoneUpdatesRequired"] = ("Milestone updates are required.", "تحديثات المرحلة مطلوبة."),
            ["MilestonesUpdatedSuccessfully"] = ("Milestones updated successfully.", "تم تحديث المراحل بنجاح."),
            ["NoMilestonesUpdated"] = ("No milestones were updated.", "لم يتم تحديث أي مراحل."),

            ["InvitationCreatedSuccessfully"] = ("Invitation created successfully.", "تم إنشاء الدعوة بنجاح."),

            ["NoReviewsFound"] = ("No reviews found.", "لم يتم العثور على مراجعات."),
            ["ReviewCreationFailed"] = ("Failed to create review.", "فشل في إنشاء المراجعة."),
            ["ReviewCreatedSuccessfully"] = ("Review created successfully.", "تم إنشاء المراجعة بنجاح."),
            ["ReviewUpdateFailed"] = ("Failed to update review.", "فشل في تحديث المراجعة."),
            ["ReviewUpdatedSuccessfully"] = ("Review updated successfully.", "تم تحديث المراجعة بنجاح."),

            ["InvalidPageNumberPageSize"] = ("Invalid page number or page size.", "رقم الصفحة أو حجم الصفحة غير صالح."),
            ["NoNotificationsFound"] = ("No notifications found.", "لم يتم العثور على إشعارات."),
            ["NotificationsRetrievedSuccessfully"] = ("Notifications retrieved successfully.", "تم استرداد الإشعارات بنجاح."),
            ["InvalidNotificationId"] = ("Invalid notification ID.", "معرف الإشعار غير صالح."),
            ["NotificationNotFound"] = ("Notification not found.", "الإشعار غير موجود."),
            ["NotificationRetrievedSuccessfully"] = ("Notification retrieved successfully.", "تم استرداد الإشعار بنجاح."),
            ["UnexpectedError"] = ("An unexpected error occurred.", "حدث خطأ غير متوقع."),
            ["NoManualNotificationsFound"] = ("No manual notifications found.", "لم يتم العثور على إشعارات يدوية."),
            ["ManualNotificationsRetrievedSuccessfully"] = ("Manual notifications retrieved successfully.", "تم استرداد الإشعارات اليدوية بنجاح."),


            ["NoTeamsFound"] = ("No teams found.", "لم يتم العثور على فرق."),
            ["TeamsRetrievedSuccessfully"] = ("Teams retrieved successfully.", "تم استرداد الفرق بنجاح."),
            ["InvalidRequestPayload"] = ("Invalid request payload.", "حمولة الطلب غير صالحة."),
            ["TeamCreationFailed"] = ("Failed to create team.", "فشل في إنشاء الفريق."),
            ["UnexpectedServerError"] = ("An unexpected server error occurred.", "حدث خطأ غير متوقع في الخادم."),
            ["InvalidTeamIdOrPayload"] = ("Invalid team ID or request payload.", "معرف الفريق أو حمولة الطلب غير صالحة."),
            ["TeamNotFoundOrStatusUpdateFailed"] = ("Team not found or failed to update status.", "الفريق غير موجود أو فشل تحديث الحالة."),
            ["TeamStatusUpdatedSuccessfully"] = ("Team status updated successfully.", "تم تحديث حالة الفريق بنجاح."),
            ["InvalidTeamId"] = ("Invalid team ID.", "معرف الفريق غير صالح."),
            ["TeamNotFound"] = ("Team not found.", "الفريق غير موجود."),
            ["TeamDeletedSuccessfully"] = ("Team deleted successfully.", "تم حذف الفريق بنجاح."),

            ["TransactionNotFound"] = ("Transaction not found.", "المعاملة غير موجودة."),
            ["NoTransactionsFound"] = ("No transactions found.", "لم يتم العثور على معاملات."),
            ["InvalidRequestPayload"] = ("Invalid request payload.", "حمولة الطلب غير صالحة."),
            ["TransactionCreationFailed"] = ("Failed to create transaction.", "فشل في إنشاء المعاملة."),
            ["UnexpectedServerError"] = ("An unexpected server error occurred.", "حدث خطأ غير متوقع في الخادم."),


            ["InvalidUserId"] = ("Invalid user ID.", "معرف المستخدم غير صالح."),
            ["UserNotFound"] = ("User not found.", "المستخدم غير موجود."),
            ["Success"] = ("Operation completed successfully.", "تمت العملية بنجاح."),
            ["UserDetailsUpdated"] = ("User details updated successfully.", "تم تحديث تفاصيل المستخدم بنجاح."),
            ["UserDeleted"] = ("User deleted successfully.", "تم حذف المستخدم بنجاح."),

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
