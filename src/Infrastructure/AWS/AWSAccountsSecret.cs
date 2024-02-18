namespace Accounts.Infrastructure.AWS;

public class AWSAccountsSecret
{
    public string dbClusterIdentifier { get; set; }

    public string dbSecret { get; set; }

    public string dbname { get; set; }

    public string engine { get; set; }

    public int port { get; set; }

    public string host { get; set; }

    public string username { get; set; }
    public string password { get; set; }
}