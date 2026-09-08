namespace FactFlow.Web.Authentication;

public sealed class DemoAuthOptions
{
    public const string SectionName = "DemoAuth";

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SupervisorUsername { get; set; } = string.Empty;
    public string SupervisorPassword { get; set; } = string.Empty;
}
