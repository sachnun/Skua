using Newtonsoft.Json;

namespace Skua.Core.Models.GitHub;

public class DeviceCodeResponse
{
    [JsonProperty("device_code")]
    public string DeviceCode { get; set; } = string.Empty;

    [JsonProperty("user_code")]
    public string UserCode { get; set; } = string.Empty;

    [JsonProperty("verification_uri")]
    public string VerificationUri { get; set; } = string.Empty;

    [JsonProperty("expires_in")]
    public int ExpireTime { get; set; }

    [JsonProperty("interval")]
    public int Interval { get; set; }
}

public class TokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonProperty("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonProperty("scope")]
    public string Scope { get; set; } = string.Empty;
}