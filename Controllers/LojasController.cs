using cp5_d.Models;
using cp5_d.Services;
using Microsoft.AspNetCore.Mvc;

namespace cp5_d.Controllers;

public class LojasController : Controller
{
    private readonly IRepository<Loja> _lojas;

    public LojasController(IRepository<Loja> lojas)
    {
        _lojas = lojas;
    }

    public IActionResult Index() => View(_lojas.GetAll());

    public IActionResult Details(int id)
    {
        var loja = _lojas.GetById(id);
        if (loja == null) return NotFound();
        return View(loja);
    }

    public IActionResult Create() => View(new Loja());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Loja loja)
    {
        if (!ModelState.IsValid) return View(loja);
        _lojas.Add(loja);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var loja = _lojas.GetById(id);
        if (loja == null) return NotFound();
        return View(loja);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Loja loja)
    {
        if (id != loja.Id) return BadRequest();
        if (!ModelState.IsValid) return View(loja);
        _lojas.Update(loja);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var loja = _lojas.GetById(id);
        if (loja == null) return NotFound();
        return View(loja);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _lojas.Delete(id);
        return RedirectToAction(nameof(Index));
    }
}
