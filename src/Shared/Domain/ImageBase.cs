using Shared.Common;

using Newtonsoft.Json;

namespace Shared.Domain;

public class ImageBase : AuditableEntity
{
    [JsonProperty("id")]
    public string Id { get; init; } = string.Empty;

    [JsonProperty("filename")]
    public string Filename { get; protected set; } = string.Empty;

    [JsonProperty("url")]
    public string URL { get; protected set; } = string.Empty;

    [JsonProperty("altText")]
    public string AltText { get; protected set; } = string.Empty;

    [JsonProperty("size")]
    public int Size { get; protected set; }
}