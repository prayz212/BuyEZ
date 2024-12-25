using Shared.Common;

namespace Shared.Domain;

public class ImageBase : AuditableEntity
{
    public string Id { get; set; } = string.Empty;

    public string Filename { get; set; } = string.Empty;

    public string URL { get; set; } = string.Empty;

    public string AltText { get; set; } = string.Empty;

    public int Size { get; set; }
}