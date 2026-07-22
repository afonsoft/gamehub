using Abp.Application.Features;
using Abp.Localization;
using Abp.UI.Inputs;

namespace GameHub.Features
{
    public class GameHubFeatureProvider : FeatureProvider
    {
        public override void SetFeatures(IFeatureDefinitionContext context)
        {
            context.Create(
                GameHubFeatures.TestCheckFeature,
                defaultValue: "false",
                displayName: L("TestCheckFeature"),
                inputType: new CheckboxInputType()
            );
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, GameHubConsts.LocalizationSourceName);
        }
    }
}