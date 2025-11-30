namespace Skua.Core.Messaging;
public record LoadScriptMessage(string? Path, string? Name = null);

public record EditScriptMessage(string? Path);

public record StartScriptMessage(string? Path, string? Name = null);

public record ToggleScriptMessage();

public record CloseScriptRepoMessage();