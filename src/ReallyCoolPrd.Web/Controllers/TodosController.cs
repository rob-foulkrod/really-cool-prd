using Microsoft.AspNetCore.Mvc;
using ReallyCoolPrd.Web.Services;

namespace ReallyCoolPrd.Web.Controllers;

public sealed class TodosController : Controller
{
    private readonly ITodoProvider _todoProvider;

    public TodosController(ITodoProvider todoProvider)
    {
        _todoProvider = todoProvider;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(_todoProvider.GetAll());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(string title)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            _todoProvider.Add(title);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(int id)
    {
        _todoProvider.Toggle(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _todoProvider.Delete(id);
        return RedirectToAction(nameof(Index));
    }
}
