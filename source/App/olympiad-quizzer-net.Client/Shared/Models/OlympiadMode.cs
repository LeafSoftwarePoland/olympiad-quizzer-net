namespace OlympiadQuizzer.Client.Shared.Models;

public sealed class OlympiadMode
{
    public string OlympiadId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GoverningBody { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public List<int> SeasonsAvailable { get; set; } = new();
    public List<StageDefinition> Stages { get; set; } = new();
}
