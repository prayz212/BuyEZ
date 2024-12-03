using ClientManagementAPI.Application.Common;

namespace ClientManagementAPI.Application.Domain.Clients;

public class ImageBase : AuditableEntity
{
    public string Id { get; set; } = string.Empty;

    public string Filename { get; set; } = string.Empty;

    public string URL { get; set; } = string.Empty;

    public string AltText { get; set; } = string.Empty;

    public int Size { get; set; }
}