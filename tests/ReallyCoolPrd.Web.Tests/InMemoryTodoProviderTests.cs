using ReallyCoolPrd.Web.Services;

namespace ReallyCoolPrd.Web.Tests;

public class InMemoryTodoProviderTests
{
    [Fact]
    public void Add_AssignsIncrementingIds_AndTrimsTitle()
    {
        var provider = new InMemoryTodoProvider();

        var first = provider.Add("  First task  ");
        var second = provider.Add("Second task");

        Assert.Equal(1, first.Id);
        Assert.Equal("First task", first.Title);
        Assert.Equal(2, second.Id);
        Assert.Equal("Second task", second.Title);
    }

    [Fact]
    public void Toggle_FlipsCompletionState()
    {
        var provider = new InMemoryTodoProvider();
        var item = provider.Add("Task");

        var toggled = provider.Toggle(item.Id);

        var current = provider.GetAll().Single();
        Assert.True(toggled);
        Assert.True(current.IsDone);
    }

    [Fact]
    public void Delete_RemovesItem()
    {
        var provider = new InMemoryTodoProvider();
        var item = provider.Add("Task");

        var removed = provider.Delete(item.Id);

        Assert.True(removed);
        Assert.Empty(provider.GetAll());
    }

    [Fact]
    public void GetAll_ListsIncompleteBeforeCompleted()
    {
        var provider = new InMemoryTodoProvider();
        var first = provider.Add("First");
        var second = provider.Add("Second");

        provider.Toggle(first.Id);

        var orderedIds = provider.GetAll().Select(item => item.Id).ToArray();

        Assert.Equal([second.Id, first.Id], orderedIds);
    }
}
