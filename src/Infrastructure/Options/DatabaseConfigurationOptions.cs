namespace Accounts.Infrastructure.Options;

public class DataBaseConfigurationOptions
{
    public string Name { get; set; }
    public string SecretName { get; set; }
    public bool ConfigureDatabase { get; set; }

    public bool SwitchToPostgresDB { get; set; }
}