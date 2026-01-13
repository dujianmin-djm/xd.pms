using XD.Pms.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace XD.Pms.Features;

public class PmsFeatureDefinitionProvider : FeatureDefinitionProvider
{
	public override void Define(IFeatureDefinitionContext context)
	{
		var group = context.AddGroup(PmsFeatures.GroupName);
		group.AddFeature(
			name: PmsFeatures.MaxUserCount,
			defaultValue: "120",
			displayName: L("Feature:MaxUserCount"),
			description: L("Feature:MaxUserCountDescription"),
			valueType: new FreeTextStringValueType(new NumericValueValidator(1, 2000)),
			isAvailableToHost: false
		);
	}

	private static LocalizableString L(string name)
	{
		return LocalizableString.Create<PmsResource>(name);
	}
}
