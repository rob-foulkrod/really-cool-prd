using ReallyCoolPrd.Web.Models;

namespace ReallyCoolPrd.Web.Services;

public interface ITodoProvider
{
    IReadOnlyList<TodoItem> GetAll();
    TodoItem Add(string title);
    bool Toggle(int id);
    bool Delete(int id);
}
