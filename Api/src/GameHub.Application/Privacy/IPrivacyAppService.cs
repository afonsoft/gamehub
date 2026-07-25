using GameHub.Privacy.Dto;
using System.Threading.Tasks;

namespace GameHub.Privacy
{
    /// <summary>
    /// LGPD privacy operations for user data export and deletion/anonymization.
    /// </summary>
    public interface IPrivacyAppService
    {
        /// <summary>
        /// Exports all personal data for the given user.
        /// </summary>
        Task<UserDataExportDto> ExportUserDataAsync(long userId);

        /// <summary>
        /// Anonymizes or deletes all personal data for the given user.
        /// </summary>
        Task DeleteUserDataAsync(long userId);

        /// <summary>Returns the privacy policy for the given game.</summary>
        Task<PrivacyPolicyDto> GetForGameAsync(string gameSlug);

        /// <summary>Returns the current player's consent for the given game.</summary>
        Task<PrivacyConsentDto> GetConsentAsync(GetPrivacyConsentInput input);

        /// <summary>Records the current player's consent to the game's privacy policy.</summary>
        Task SaveConsentAsync(SavePrivacyConsentInput input);
    }
}
