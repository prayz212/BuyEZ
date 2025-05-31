using ClientManagementAPI.Application.Domain;

using Shared.Common.Enums;

namespace ClientManagementAPI.Application.Shared.Dtos;

public record ClientDetailResponse(string Id, string Name, string AliasName, string BriefDescription, SubscriptionType SubscriptionType, ProductType[] ProductTypes, DateTimeOffset ValidTo, bool IsActivated, ImageDetailResponse? Logo);

public static partial class ResponseExtensions
{
    public static ClientDetailResponse ToDto(this Client client) => new 
    (
        client.Id,
        client.Name,
        client.AliasName,
        client.BriefDescription,
        client.SubscriptionType,
        client.RegisteredProductType,
        client.ValidUntil,
        client.IsActivated,
        client.Logo?.ToDto()
    );
}