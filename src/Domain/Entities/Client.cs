namespace Accounts.Domain.Entities;

public record Client
{
    public int ClientId { get; set; }
    public string Name { get; set; }
}