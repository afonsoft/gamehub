using Abp;
using Abp.Localization;
using Shouldly;
using System.Globalization;
using Xunit;

namespace GameHub.Tests.Localization
{
    // ReSharper disable once InconsistentNaming
    public class Localization_Tests : GameHubTestBase
    {
        [Theory]
        [InlineData("en")]
        [InlineData("pt-BR")]
        [InlineData("es")]
        public void Simple_Localization_Test(string cultureName)
        {
            // Configurar culture para o teste
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

            Resolve<ILanguageManager>().CurrentLanguage.Name.ShouldBe(cultureName);

            var localizationManager = Resolve<ILocalizationManager>();

            // Obter a string localizada e verificar se não está vazia
            var localizedString = localizationManager.GetString(AbpConsts.LocalizationSourceName, "Identity.UserNotInRole");
            localizedString.ShouldNotBeNullOrEmpty();
        }
    }
}