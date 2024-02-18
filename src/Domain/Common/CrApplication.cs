namespace Accounts.Domain.Common;

public class CrApplication
{
    private readonly string m_key;

    public CrApplication(string key)
    {
        m_key = key;
    }

    public string Key => m_key;
}