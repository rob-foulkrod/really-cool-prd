using ReallyCoolPrd.Web.Models;

namespace ReallyCoolPrd.Web.Services;

public sealed class InMemoryTodoProvider : ITodoProvider
{
    private readonly object _gate = new();
    private readonly List<TodoItem> _items = [];
    private int _nextId = 1;

    public IReadOnlyList<TodoItem> GetAll()
    {
        lock (_gate)
        {
            return _items
                .OrderBy(item => item.IsDone)
                .ThenBy(item => item.Id)
                .ToList();
        }
    }

    public TodoItem Add(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Todo title is required.", nameof(title));
        }

        lock (_gate)
        {
            var item = new TodoItem
            {
                Id = _nextId++,
                Title = title.Trim(),
                IsDone = false
            };

            _items.Add(item);
            return item;
        }
    }

    public bool Toggle(int id)
    {
        lock (_gate)
        {
            var index = _items.FindIndex(item => item.Id == id);
            if (index < 0)
            {
                return false;
            }

            var existing = _items[index];
            _items[index] = new TodoItem
            {
                Id = existing.Id,
                Title = existing.Title,
                IsDone = !existing.IsDone
            };

            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_gate)
        {
            return _items.RemoveAll(item => item.Id == id) > 0;
        }
    }
}
