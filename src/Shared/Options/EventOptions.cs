namespace Shared.Options;

public class EventOptions
{
    public required string Topic { get; set; }

    public string? GroupId { get; set; }
}