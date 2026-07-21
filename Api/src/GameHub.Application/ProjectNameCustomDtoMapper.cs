using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GameHub.Airplanes;
using GameHub.Airplanes.Dtos;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Catalog.Dto;
using GameHub.Configuration;
using GameHub.Developer.Dto;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Gameplay.Dto;
using GameHub.Moderation;
using GameHub.Admin.Dto;
using Abp.Auditing;

namespace GameHub
{
    internal static class ProjectNameCustomDtoMapper
    {
        public static void CreateMappings(IMapperConfigurationExpression configuration)
        {
            /* ADD YOUR OWN CUSTOM AUTOMAPPER MAPPINGS HERE */

            configuration.CreateMap<CreateOrEditAirplaneDto, Airplane>();
            configuration.CreateMap<Airplane, AirplaneDto>();

            // Catalog
            configuration.CreateMap<Category, CategoryDto>();
            configuration.CreateMap<Tag, TagDto>();

            configuration.CreateMap<GameCategory, CategoryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Category.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Category.Slug))
                .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.Category.SortOrder));

            configuration.CreateMap<GameTag, TagDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Tag.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Tag.Name))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Tag.Slug));

            configuration.CreateMap<Game, GameCardDto>()
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.GameCategories))
                .ForMember(dest => dest.SupportsMobile, opt => opt.MapFrom(src => src.SupportsMobile))
                .ForMember(dest => dest.SupportsDesktop, opt => opt.MapFrom(src => src.SupportsDesktop));

            configuration.CreateMap<Game, GameDetailDto>()
                .ForMember(dest => dest.Orientation, opt => opt.MapFrom(src => src.Orientation.ToString()))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.GameTags))
                .ForMember(dest => dest.DeveloperName, opt => opt.MapFrom(src => src.DeveloperProfile != null ? src.DeveloperProfile.DisplayName : string.Empty))
                .ForMember(dest => dest.PublishedBuildUrl, opt => opt.MapFrom(src => BuildUrl(src.PublishedBuild)))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => (decimal)(src.AverageRating ?? 0)));

            configuration.CreateMap<Game, GameSummaryDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PublishedBuildVersion, opt => opt.MapFrom(src => src.PublishedBuild != null ? src.PublishedBuild.Version : string.Empty))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastModificationTime ?? src.CreationTime));

            configuration.CreateMap<Game, AdminGameListItemDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DeveloperName, opt => opt.MapFrom(src => src.DeveloperProfile != null ? src.DeveloperProfile.DisplayName : string.Empty));

            configuration.CreateMap<Game, AdminGameDetailDto>()
                .ForMember(dest => dest.Orientation, opt => opt.MapFrom(src => src.Orientation.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DeveloperName, opt => opt.MapFrom(src => src.DeveloperProfile != null ? src.DeveloperProfile.DisplayName : string.Empty))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => (decimal)(src.AverageRating ?? 0)))
                .ForMember(dest => dest.BuildHistory, opt => opt.MapFrom(src => src.GameBuilds))
                .ForMember(dest => dest.ModerationHistory, opt => opt.MapFrom(src => src.ModerationReviews))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.GameCategories))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.GameTags))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationTime));

            // Developer / Build
            configuration.CreateMap<GameBuild, BuildDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationTime))
                .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src => src.PublishedTime));

            configuration.CreateMap<CreateGameDraftInput, Game>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.AgeRating, opt => opt.MapFrom(src => src.AgeRating))
                .ForMember(dest => dest.Orientation, opt => opt.MapFrom(src => ParseOrientation(src.Orientation)))
                .ForMember(dest => dest.TotalPlays, opt => opt.Ignore());

            configuration.CreateMap<UpdateGameMetadataInput, Game>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Orientation, opt => opt.MapFrom(src => ParseOrientation(src.Orientation)));

            configuration.CreateMap<DeveloperProfile, DeveloperProfileDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            configuration.CreateMap<CreateOrUpdateDeveloperProfileInput, DeveloperProfile>();
            configuration.CreateMap<DeveloperProfileStatus, string>().ConvertUsing(s => s.ToString());

            // Gameplay
            configuration.CreateMap<PlaySession, PlaySessionDto>()
                .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.Id));
            configuration.CreateMap<LeaderboardEntry, LeaderboardEntryDto>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            // Moderation
            configuration.CreateMap<ModerationReview, ModerationReviewDto>()
                .ForMember(dest => dest.ReviewId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.GameBuildId, opt => opt.MapFrom(src => src.GameBuildId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Decision, opt => opt.MapFrom(src => src.Decision.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationTime))
                .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.LastModificationTime))
                .ForMember(dest => dest.GameTitle, opt => opt.MapFrom(src => src.Game != null ? src.Game.Title : string.Empty));

            configuration.CreateMap<UserReport, UserReportDto>()
                .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationTime))
                .ForMember(dest => dest.GameTitle, opt => opt.MapFrom(src => src.Game != null ? src.Game.Title : string.Empty));

            // Configuration / Audit
            configuration.CreateMap<FeatureFlag, FeatureFlagDto>();

            configuration.CreateMap<Abp.Auditing.AuditLog, AuditLogDto>()
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => $"{src.ServiceName}.{src.MethodName}"))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => TruncateAuditDetails(src.Parameters)));
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
    }
}
