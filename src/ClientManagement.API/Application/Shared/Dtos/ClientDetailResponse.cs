using ClientManagementAPI.Application.Domain;
using Shared.Common.Enums;

namespace ClientManagementAPI.Application.Shared.Dtos;

public record ClientDetailResponse(string Id, string Name, string AliasName, string BriefDescription, SubscriptionType SubscriptionType, ProductType[] ProductTypes, DateTimeOffset ValidTo, bool IsActivated, ImageDetailResponse? Logo);