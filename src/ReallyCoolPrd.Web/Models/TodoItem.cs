namespace ReallyCoolPrd.Web.Models;

public sealed class TodoItem
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public bool IsDone { get; init; }
}
