using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OlympiadQuizzer.Client.Shared.Models;

namespace OlympiadQuizzer.Client.Shared.Services;

public sealed class ModeCatalog
{
    private readonly HttpClient _httpClient;
    private OlympiadMode _mode;

    public ModeCatalog([FromKeyedServices("static")] HttpClient httpClient)
    {
        _httpClient = httpClient;
        _mode = new OlympiadMode();
    }

    public async Task LoadAsync()
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        OlympiadMode loaded = await _httpClient.GetFromJsonAsync<OlympiadMode>("modes/oij.json", options);
        _mode = loaded;
    }

    public List<StageDefinition> GetStages()
    {
        return _mode.Stages;
    }

    public StageDefinition GetDefaultStage()
    {
        return _mode.Stages.First(stage => stage.StageId == "E1");
    }

    public StageDefinition GetStageById(string stageId)
    {
        return _mode.Stages.FirstOrDefault(stage => stage.StageId == stageId);
    }
}
