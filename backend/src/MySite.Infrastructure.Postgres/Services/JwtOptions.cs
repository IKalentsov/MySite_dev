namespace MySite.Infrastructure.Postgres.Services;

public class JwtOptions
{
    public string SecretKey { get; set; } = string.Empty;

    public int ExpitesHours { get; set; }
}