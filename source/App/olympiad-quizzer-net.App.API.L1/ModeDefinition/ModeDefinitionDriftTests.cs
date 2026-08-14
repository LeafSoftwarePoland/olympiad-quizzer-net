using System.Text.Json.Nodes;
using OlympiadQuizzer.App.Api.L1.Harness;
using OlympiadQuizzer.Core.Tests.Common;

namespace OlympiadQuizzer.App.Api.L1.ModeDefinition;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class ModeDefinitionDriftTests
{
    [Fact]
    public void OijJson_ParsedContent_DoesMatchMdBlock()
    {
        string repoRoot = FixturePath.RepoRoot();
        string oijMdPath = Path.Combine(repoRoot, "docs", "rules", "oij.md");
        string oijJsonPath = Path.Combine(
            repoRoot, "source", "App", "olympiad-quizzer-net.App.Client", "wwwroot", "modes", "oij.json");

        string mdContent = File.ReadAllText(oijMdPath);
        string fileContent = File.ReadAllText(oijJsonPath);

        string jsonFromMd = ExtractJsonBlock(mdContent);

        JsonNode mdNode = JsonNode.Parse(jsonFromMd);
        JsonNode fileNode = JsonNode.Parse(fileContent);

        Assert.True(
            JsonNode.DeepEquals(mdNode, fileNode),
            "oij.json does not match the machine-readable block in docs/rules/oij.md — " +
            "update the JSON file after editing the markdown.");
    }

    private static string ExtractJsonBlock(string mdContent)
    {
        string[] lines = mdContent.Split('\n');
        bool inSection = false;
        bool inBlock = false;
        List<string> jsonLines = [];

        foreach (string rawLine in lines)
        {
            string line = rawLine.TrimEnd('\r');

            if (line == "## Machine-readable mode definition")
            {
                inSection = true;
                continue;
            }

            if (inSection && line.StartsWith("```json"))
            {
                inBlock = true;
                continue;
            }

            if (inBlock && line.StartsWith("```"))
            {
                break;
            }

            if (inBlock)
            {
                jsonLines.Add(line);
            }
        }

        return string.Join("\n", jsonLines);
    }
}
