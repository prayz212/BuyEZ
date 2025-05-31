using Shared.Common;

namespace Shared.Domain;

public class ImageBase : AuditableEntity
{
    public string Id { get; init; } = string.Empty;

    public string Filename { get; protected set; } = string.Empty;

    public string URL { get; protected set; } = string.Empty;

    public string AltText { get; protected set; } = string.Empty;

    public int Size { get; protected set; }
}