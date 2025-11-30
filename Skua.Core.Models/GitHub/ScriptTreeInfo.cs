using Newtonsoft.Json;

namespace Skua.Core.Models.GitHub;

public class ScriptTree
{
    [JsonProperty("sha")]
    public string Sha { get; set; } = string.Empty;

    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;

    [JsonProperty("tree")]
    public List<ScriptTreeInfo> TreeInfo { get; set; } = new List<ScriptTreeInfo>();
}

public class ScriptTreeInfo
{
    [JsonProperty("path")]
    public string Path { get; set; } = string.Empty;

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
}