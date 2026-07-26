using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GameHub.Builds;
using GameHub.Builds.Dto;
using GameHub.Catalog;
using GameHub.Catalog.Dto;
using GameHub.Configuration;
using GameHub.Developer.Dto;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Gameplay.Dto;
using GameHub.Moderation;
using GameHub.Moderation.Dto;
using GameHub.Monetization;
using GameHub.Inspector;
using GameHub.Inspector.Dto;
using GameHub.Playtesting;
using GameHub.Playtesting.Dto;
using GameHub.Privacy;
using GameHub.Privacy.Dto;
using GameHub.Admin.Dto;
using GameHub.Storage;
using GameHub.Multiplayer;
using GameHub.Multiplayer.Dto;
using Abp.Auditing;

namespace GameHub
{
    internal static class GameHubCustomDtoMapper
    {
        public static void CreateMappings(IMapperConfigurationExpression configuration)
        {
            /* ADD YOUR OWN CUSTOM AUTOMAPPER MAPPINGS HERE */

            // Catalog
            configuration.CreateMap<Category, CategoryDto>();
            configuration.CreateMap<Tag, TagDto>();

            configuration.CreateMap<GameCategory, CategoryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Category.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Category.Slug))
                .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.Category.SortOrder))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Category.Description))
                .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => src.Category.Keywords));

            configuration.CreateMap<GameTag, TagDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Tag.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Tag.Name))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Tag.Slug));

            configuration.CreateMap<Game, GameCardDto>()
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.GameCategories))
                .ForMember(dest => dest.SupportsMobile, opt => opt.MapFrom(src => src.SupportsMobile))
                .ForMember(dest => dest.SupportsDesktop, opt => opt.MapFrom(src => src.SupportsDesktop))
                .ForMember(dest => dest.SupportsCloudSaves, opt => opt.MapFrom(src => src.SupportsCloudSaves))
                .ForMember(dest => dest.AnimatedThumbnailUrl, opt => opt.MapFrom(src => GetApprovedAnimatedThumbnailUrl(src)))
                .ForMember(dest => dest.ThumbnailStatus, opt => opt.MapFrom(src => src.ThumbnailStatus.ToString()))
                .ForMember(dest => dest.AspectRatio, opt => opt.MapFrom(src => src.AspectRatio.ToString()))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => (decimal)ComputeAverageRating(src)))
                .ForMember(dest => dest.TotalVotes, opt => opt.MapFrom(src => ComputeTotalVotes(src)))
                .ForMember(dest => dest.IsWebExclusive, opt => opt.MapFrom(src => IsWebExclusive(src)));

            configuration.CreateMap<Game, GameDetailDto>()
                .ForMember(dest => dest.Orientation, opt => opt.MapFrom(src => src.Orientation.ToString()))
                .ForMember(dest => dest.ControlScheme, opt => opt.MapFrom(src => src.ControlScheme.ToString()))
                .ForMember(dest => dest.CutscenesSkippable, opt => opt.MapFrom(src => src.CutscenesSkippable))
                .ForMember(dest => dest.DefaultLanguage, opt => opt.MapFrom(src => src.DefaultLanguage))
                .ForMember(dest => dest.SupportedLanguages, opt => opt.MapFrom(src => ParseSupportedLanguages(src.SupportedLanguages)))
                .ForMember(dest => dest.AnimatedThumbnailUrl, opt => opt.MapFrom(src => GetApprovedAnimatedThumbnailUrl(src)))
                .ForMember(dest => dest.ThumbnailStatus, opt => opt.MapFrom(src => src.ThumbnailStatus.ToString()))
                .ForMember(dest => dest.AspectRatio, opt => opt.MapFrom(src => src.AspectRatio.ToString()))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.GameCategories))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.GameTags))
                .ForMember(dest => dest.DeveloperName, opt => opt.MapFrom(src => src.DeveloperProfile != null ? src.DeveloperProfile.DisplayName : string.Empty))
                .ForMember(dest => dest.PublishedBuildUrl, opt => opt.MapFrom(src => BuildUrl(src.PublishedBuild)))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => (decimal)ComputeAverageRating(src)))
                .ForMember(dest => dest.TotalVotes, opt => opt.MapFrom(src => ComputeTotalVotes(src)))
                .ForMember(dest => dest.IsWebExclusive, opt => opt.MapFrom(src => IsWebExclusive(src)));

            configuration.CreateMap<Game, GameSummaryDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PublishedBuildVersion, opt => opt.MapFrom(src => src.PublishedBuild != null ? src.PublishedBuild.Version : string.Empty))
                .ForMember(dest => dest.LatestBuildStatus, opt => opt.MapFrom(src => src.GameBuilds != null ? src.GameBuilds.OrderByDescending(b => b.BuildNumber).Select(b => b.Status.ToString()).FirstOrDefault() : string.Empty))
                .ForMember(dest => dest.LatestBuildId, opt => opt.MapFrom(src => src.GameBuilds != null ? (Guid?)src.GameBuilds.OrderByDescending(b => b.BuildNumber).Select(b => b.Id).FirstOrDefault() : null))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastModificationTime ?? src.CreationTime));

            configuration.CreateMap<Game, AdminGameListItemDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DeveloperName, opt => opt.MapFrom(src => src.DeveloperProfile != null ? src.DeveloperProfile.DisplayName : string.Empty))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationTime));

            configuration.CreateMap<Game, AdminGameDetailDto>()
                .ForMember(dest => dest.Orientation, opt => opt.MapFrom(src => src.Orientation.ToString()))
                .ForMember(dest => dest.ControlScheme, opt => opt.MapFrom(src => src.ControlScheme.ToString()))
                .ForMember(dest => dest.CutscenesSkippable, opt => opt.MapFrom(src => src.CutscenesSkippable))
                .ForMember(dest => dest.DefaultLanguage, opt => opt.MapFrom(src => src.DefaultLanguage))
                .ForMember(dest => dest.SupportedLanguages, opt => opt.MapFrom(src => ParseSupportedLanguages(src.SupportedLanguages)))
                .ForMember(dest => dest.AnimatedThumbnailUrl, opt => opt.MapFrom(src => src.AnimatedThumbnailUrl))
                .ForMember(dest => dest.ThumbnailStatus, opt => opt.MapFrom(src => src.ThumbnailStatus.ToString()))
                .ForMember(dest => dest.AspectRatio, opt => opt.MapFrom(src => src.AspectRatio.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DeveloperName, opt => opt.MapFrom(src => src.DeveloperProfile != null ? src.DeveloperProfile.DisplayName : string.Empty))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => (decimal)ComputeAverageRating(src)))
                .ForMember(dest => dest.TotalVotes, opt => opt.MapFrom(src => ComputeTotalVotes(src)))
                .ForMember(dest => dest.BuildHistory, opt => opt.MapFrom(src => src.GameBuilds))
                .ForMember(dest => dest.ModerationHistory, opt => opt.MapFrom(src => src.ModerationReviews))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.GameCategories))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.GameTags))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationTime));

            // Moderation
            configuration.CreateMap<UserContent, UserContentDto>();

            // Inspector
            configuration.CreateMap<InspectorSession, InspectorSessionDto>();
            configuration.CreateMap<InspectorSession, InspectorSessionDetailDto>();
            configuration.CreateMap<InspectorSdkEvent, InspectorSdkEventDto>();
            configuration.CreateMap<InspectorWarning, InspectorWarningDto>();
            configuration.CreateMap<InspectorChecklistAnswer, InspectorChecklistAnswerDto>();
            configuration.CreateMap<SaveChecklistAnswerInput, InspectorChecklistAnswer>();

            // Developer / Build
            configuration.CreateMap<GameBuild, BuildDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationTime))
                .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src => src.PublishedTime));

            configuration.CreateMap<ExternalResourceExemption, ExternalResourceExemptionDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            configuration.CreateMap<CreateGameDraftInput, Game>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.AgeRating, opt => opt.MapFrom(src => src.AgeRating))
                .ForMember(dest => dest.Orientation, opt => opt.MapFrom(src => ParseOrientation(src.Orientation)))
                .ForMember(dest => dest.ControlScheme, opt => opt.MapFrom(src => ParseControlScheme(src.ControlScheme)))
                .ForMember(dest => dest.AspectRatio, opt => opt.MapFrom(src => ParseAspectRatio(src.AspectRatio)))
                .ForMember(dest => dest.TotalPlays, opt => opt.Ignore());

            configuration.CreateMap<UpdateGameMetadataInput, Game>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Orientation, opt => opt.MapFrom(src => ParseOrientation(src.Orientation)))
                .ForMember(dest => dest.ControlScheme, opt => opt.MapFrom(src => ParseControlScheme(src.ControlScheme)))
                .ForMember(dest => dest.AspectRatio, opt => opt.MapFrom(src => ParseAspectRatio(src.AspectRatio)));

            configuration.CreateMap<DeveloperProfile, DeveloperProfileDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            configuration.CreateMap<CreateOrUpdateDeveloperProfileInput, DeveloperProfile>();
            configuration.CreateMap<DeveloperProfileStatus, string>().ConvertUsing(s => s.ToString());

            configuration.CreateMap<DeveloperTeam, DeveloperTeamDto>();
            configuration.CreateMap<DeveloperTeam, DeveloperTeamGeneralSettingsDto>();
            configuration.CreateMap<CreateOrUpdateDeveloperTeamInput, DeveloperTeam>();
            configuration.CreateMap<UpdateTeamGeneralSettingsInput, DeveloperTeam>();
            configuration.CreateMap<DeveloperTeamMember, DeveloperTeamMemberDto>();
            configuration.CreateMap<DeveloperBillingProfile, DeveloperBillingProfileDto>();
            configuration.CreateMap<SaveDeveloperBillingProfileInput, DeveloperBillingProfile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.TeamId));

            configuration.CreateMap<PlaytestSession, PlaytestSessionDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            configuration.CreateMap<RequestPlaytestInput, PlaytestSession>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());
            configuration.CreateMap<PlaytestRecording, PlaytestRecordingDto>();

            configuration.CreateMap<StoredAsset, UploadImageResultDto>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url));

            // Gameplay
            configuration.CreateMap<PlaySession, PlaySessionDto>()
                .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.Id));
            configuration.CreateMap<LeaderboardEntry, LeaderboardEntryDto>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
            configuration.CreateMap<GameErrorLog, GameErrorLogDto>();

            // Moderation
            configuration.CreateMap<ModerationReview, ModerationReviewDto>()
                .ForMember(dest => dest.ReviewId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.GameBuildId, opt => opt.MapFrom(src => src.GameBuildId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Decision, opt => opt.MapFrom(src => src.Decision.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationTime))
                .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.LastModificationTime))
                .ForMember(dest => dest.GameTitle, opt => opt.MapFrom(src => src.Game != null ? src.Game.Title : string.Empty))
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.ValidationSummary, opt => opt.Ignore())
                .ForMember(dest => dest.History, opt => opt.Ignore());

            configuration.CreateMap<ModerationReview, ModerationReviewHistoryItemDto>()
                .ForMember(dest => dest.ReviewId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Decision, opt => opt.MapFrom(src => src.Decision.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationTime));

            configuration.CreateMap<UserReport, UserReportDto>()
                .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId ?? 0))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationTime))
                .ForMember(dest => dest.GameTitle, opt => opt.MapFrom(src => src.Game != null ? src.Game.Title : string.Empty));

            // Privacy
            configuration.CreateMap<PlayerPrivacyConsent, PrivacyConsentDto>();

            // Multiplayer
            configuration.CreateMap<MatchState, MatchDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            configuration.CreateMap<MatchParticipant, MatchParticipantDto>();

            // Configuration / Audit
            configuration.CreateMap<FeatureFlag, FeatureFlagDto>();

            configuration.CreateMap<Abp.Auditing.AuditLog, AuditLogDto>()
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => $"{src.ServiceName}.{src.MethodName}"))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => TruncateAuditDetails(src.Parameters)));
        }

        private static double ComputeAverageRating(Game game)
        {
            var totalVotes = ComputeTotalVotes(game);
            if (totalVotes == 0)
            {
                return game.AverageRating ?? 0;
            }

            if (game.AverageRating.HasValue && game.AverageRating.Value > 0)
            {
                return game.AverageRating.Value;
            }

            return (double)game.TotalLikes / totalVotes * 5;
        }

        private static long ComputeTotalVotes(Game game)
        {
            return game.TotalLikes + game.TotalDislikes;
        }

        private static bool IsWebExclusive(Game game)
        {
            return game.RevenueContracts?.Any(c => c.IsActive && c.ContractType == RevenueContractType.WebExclusive) == true;
        }

        private static string TruncateAuditDetails(string parameters)
        {
            if (string.IsNullOrEmpty(parameters))
            {
                return string.Empty;
            }

            const int maxLength = 4000;
            return parameters.Length > maxLength ? parameters.Substring(0, maxLength) : parameters;
        }

        private static string BuildUrl(GameBuild build)
        {
            if (build == null)
            {
                return string.Empty;
            }

            var baseUrl = build.PublicBaseUrl?.TrimEnd('/') ?? string.Empty;
            var path = build.IndexHtmlPath?.TrimStart('/') ?? string.Empty;
            return $"{baseUrl}/{path}";
        }

        private static GameOrientation ParseOrientation(string value)
        {
            if (Enum.TryParse<GameOrientation>(value, true, out var result))
            {
                return result;
            }

            return GameOrientation.Both;
        }

        private static GameControlScheme ParseControlScheme(string value)
        {
            if (Enum.TryParse<GameControlScheme>(value, true, out var result))
            {
                return result;
            }

            return GameControlScheme.Both;
        }

        private static List<string> ParseSupportedLanguages(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<string>();
            }

            return value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToList();
        }

        private static GameAspectRatio ParseAspectRatio(string value)
        {
            if (Enum.TryParse<GameAspectRatio>(value, true, out var result))
            {
                return result;
            }

            return GameAspectRatio.Aspect16x9;
        }

        private static string GetApprovedAnimatedThumbnailUrl(Game game)
        {
            if (game.ThumbnailStatus == GameThumbnailStatus.Approved)
            {
                return game.AnimatedThumbnailUrl ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
