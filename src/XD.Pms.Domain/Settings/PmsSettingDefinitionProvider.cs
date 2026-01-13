using Volo.Abp.Identity.Localization;
using Volo.Abp.Identity.Settings;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace XD.Pms.Settings;

public class PmsSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
		//context.Add(
		//	new SettingDefinition(
		//		MesSettings.Session.PreventConcurrentLogin,//防止并发登录
		//		defaultValue: "2",
		//		L("DisplayName:Session.PreventConcurrentLoginValue"),
		//		L("Description:Session.PreventConcurrentLoginValue"),
		//		isVisibleToClients: true)
		//	.WithProperty("Type", "select")
		//	.WithProperty("Options", Enum.GetValues<PreventConcurrentLoginType>()
		//		.Select(x => new { name = x.ToString(), value = (int)x }).ToArray()),

		//	new SettingDefinition(
		//		MesSettings.SessionCleanup.IsEnabled,//是否启用会话清理
		//		defaultValue: "true",
		//		L("DisplayName:SessionCleanup.AutoExitInactiveUsers"),
		//		L("Description:SessionCleanup.AutoExitInactiveUsers"),
		//		isVisibleToClients: true),
		//	new SettingDefinition(
		//		MesSettings.SessionCleanup.NeverAccessedTimeSpan,//未访问时间间隔
		//		defaultValue: "21600",//6小时
		//		L("DisplayName:SessionCleanup.WhenWantUserLogOut"),
		//		L("Description:SessionCleanup.WhenWantUserLogOut"),
		//		isVisibleToClients: true)
		//);

		var requireNonAlphanumeric = context.GetOrNull(IdentitySettingNames.Password.RequireNonAlphanumeric);
		requireNonAlphanumeric?.DefaultValue = true.ToString();

		context.Add(
			new SettingDefinition(
				IdentitySettingNames.Password.RequiredLength,//密码的最小长度
				6.ToString(),
				L("DisplayName:Abp.Identity.Password.RequiredLength"),
				L("Description:Abp.Identity.Password.RequiredLength"),
				true),

			new SettingDefinition(
				IdentitySettingNames.Password.RequiredUniqueChars,//要求唯一字符数量
				1.ToString(),
				L("DisplayName:Abp.Identity.Password.RequiredUniqueChars"),
				L("Description:Abp.Identity.Password.RequiredUniqueChars"),
				true),

			new SettingDefinition(
				IdentitySettingNames.Password.RequireNonAlphanumeric,//密码是否必须包含非字母数字。
				false.ToString(),
				L("DisplayName:Abp.Identity.Password.RequireNonAlphanumeric"),
				L("Description:Abp.Identity.Password.RequireNonAlphanumeric"),
				true),

			new SettingDefinition(
				IdentitySettingNames.Password.RequireLowercase,// 密码是否必须包含小写字母。
				false.ToString(),
				L("DisplayName:Abp.Identity.Password.RequireLowercase"),
				L("Description:Abp.Identity.Password.RequireLowercase"),
				true),

			new SettingDefinition(
				IdentitySettingNames.Password.RequireUppercase,//密码是否必须包含大写字母。
				false.ToString(),
				L("DisplayName:Abp.Identity.Password.RequireUppercase"),
				L("Description:Abp.Identity.Password.RequireUppercase"),
				true),

			new SettingDefinition(
				IdentitySettingNames.Password.RequireDigit,//密码是否必须包含数字
				false.ToString(),
				L("DisplayName:Abp.Identity.Password.RequireDigit"),
				L("Description:Abp.Identity.Password.RequireDigit"),
				true),

			new SettingDefinition(
				IdentitySettingNames.Password.ForceUsersToPeriodicallyChangePassword, //强制用户定期更改密码
				false.ToString(),
				L("DisplayName:Abp.Identity.Password.ForceUsersToPeriodicallyChangePassword"),
				L("Description:Abp.Identity.Password.ForceUsersToPeriodicallyChangePassword"),
				true),

			new SettingDefinition(
				IdentitySettingNames.Password.PasswordChangePeriodDays, //密码更改周期(天)
				0.ToString(),
				L("DisplayName:Abp.Identity.Password.PasswordChangePeriodDays"),
				L("Description:Abp.Identity.Password.PasswordChangePeriodDays"),
				true),

			new SettingDefinition(
				IdentitySettingNames.Lockout.AllowedForNewUsers,//允许新用户被锁定。
				false.ToString(),
				L("DisplayName:Abp.Identity.Lockout.AllowedForNewUsers"),
				L("Description:Abp.Identity.Lockout.AllowedForNewUsers"),
				true),

			new SettingDefinition(
				IdentitySettingNames.Lockout.LockoutDuration,//锁定时间(秒)
				(5 * 60).ToString(),
				L("DisplayName:Abp.Identity.Lockout.LockoutDuration"),
				L("Description:Abp.Identity.Lockout.LockoutDuration"),
				true),

			new SettingDefinition(
				IdentitySettingNames.Lockout.MaxFailedAccessAttempts,//最大失败访问尝试次数
				5.ToString(),
				L("DisplayName:Abp.Identity.Lockout.MaxFailedAccessAttempts"),
				L("Description:Abp.Identity.Lockout.MaxFailedAccessAttempts"),
				true),

			new SettingDefinition(
				IdentitySettingNames.SignIn.RequireConfirmedEmail,// 登录时是否需要验证的电子邮箱。用户可以创建账户但在验证邮箱地址之前无法登录
				false.ToString(),
				L("DisplayName:Abp.Identity.SignIn.RequireConfirmedEmail"),
				L("Description:Abp.Identity.SignIn.RequireConfirmedEmail"),
				true),
			new SettingDefinition(
				IdentitySettingNames.SignIn.EnablePhoneNumberConfirmation,//用户是否可以确认电话号码
				true.ToString(),
				L("DisplayName:Abp.Identity.SignIn.EnablePhoneNumberConfirmation"),
				L("Description:Abp.Identity.SignIn.EnablePhoneNumberConfirmation"),
				true),
			new SettingDefinition(
				IdentitySettingNames.SignIn.RequireEmailVerificationToRegister,//强制要求验证电子邮件才能注册
				false.ToString(),
				L("DisplayName:Abp.Identity.SignIn.RequireEmailVerificationToRegister"),
				L("Description:Abp.Identity.SignIn.RequireEmailVerificationToRegister"),
				false),
			new SettingDefinition(
				IdentitySettingNames.SignIn.RequireConfirmedPhoneNumber,//登录时是否需要验证的手机号码。用户可以创建账户但在验证手机号码之前无法登录
				false.ToString(),
				L("DisplayName:Abp.Identity.SignIn.RequireConfirmedPhoneNumber"),
				L("Description:Abp.Identity.SignIn.RequireConfirmedPhoneNumber"),
				true),

			new SettingDefinition(
				IdentitySettingNames.User.IsUserNameUpdateEnabled,//启用用户名更新
				true.ToString(),
				L("DisplayName:Abp.Identity.User.IsUserNameUpdateEnabled"),
				L("Description:Abp.Identity.User.IsUserNameUpdateEnabled"),
				true),

			new SettingDefinition(
				IdentitySettingNames.User.IsEmailUpdateEnabled,//启用电子邮箱更新
				true.ToString(),
				L("DisplayName:Abp.Identity.User.IsEmailUpdateEnabled"),
				L("Description:Abp.Identity.User.IsEmailUpdateEnabled"),
				true),

			new SettingDefinition(
				IdentitySettingNames.OrganizationUnit.MaxUserMembershipCount,//组织单位最大允许的成员资格计数
				int.MaxValue.ToString(),
				L("Identity.OrganizationUnit.MaxUserMembershipCount"),
				L("Identity.OrganizationUnit.MaxUserMembershipCount"),
				true
			)
		);
	}

	private static LocalizableString L(string name)
	{
		return LocalizableString.Create<IdentityResource>(name);
	}
}
