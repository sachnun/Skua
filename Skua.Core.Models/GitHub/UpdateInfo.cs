using Newtonsoft.Json;

namespace Skua.Core.Models.GitHub;

public class UpdateInfo
{
    [JsonProperty("html_url")]
    public string URL { get; set; } = string.Empty;

    [JsonProperty("tag_name")]
    public string Version { get; set; } = string.Empty;

    public Version ParsedVersion => System.Version.Parse(Version);

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("prerelease")]
    public bool Prerelease { get; set; }

    [JsonProperty("created_at")]
    public DateTime Time { get; set; }

    [JsonProperty("assets")]
    public List<Asset> Assets { get; set; } = new List<Asset>();

    public override string ToString()
    {
        return $"{Name} [{Version}]";
    }
}