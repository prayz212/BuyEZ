using ClientManagementAPI.Application.Domain.Clients;

namespace ClientManagementAPI.Application.Features.Clients.Shared.Dtos;

public record ClientDetailResponse(string Id, string Name, string AliasName, string BriefDescription, SubscriptionType SubscriptionType, ProductType[] ProductTypes, DateTimeOffset ValidTo, bool IsActivated, ImageDetailResponse? Logo);