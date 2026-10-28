namespace CosmicWiki.Auth.Infrastructure.Security;

public sealed class PasswordResetEmailOptions
{
    public const string SectionName = "PasswordResetEmail";

    public bool Enabled { get; set; }
    public bool ExposeDevelopmentLink { get; set; }
    public string FrontendUrl { get; set; } = "http://localhost:4200";
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "CosmicWiki";
}
