using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BlobStoring.Database;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;
using XD.Pms.Localization;

namespace XD.Pms;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpFeatureManagementDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpSettingManagementDomainSharedModule),
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule),
    typeof(BlobStoringDatabaseDomainSharedModule)
    )]
public class PmsDomainSharedModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PmsGlobalFeatureConfigurator.Configure();
        PmsModuleExtensionConfigurator.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<PmsDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<PmsResource>("zh-Hans")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/Pms");

			options.DefaultResourceType = typeof(PmsResource);

			options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "¼òÌåÖÐÎÄ"));
			options.Languages.Add(new LanguageInfo("en", "en", "English"));
        });
        
        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Pms", typeof(PmsResource));
        });
    }
}
