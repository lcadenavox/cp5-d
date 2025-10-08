using cp5_d.Models;
using cp5_d.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace cp5_d.Controllers;

public class CarrosController : Controller
{
    private readonly IRepository<Carro> _carros;
    private readonly IRepository<Loja> _lojas;

    public CarrosController(IRepository<Carro> carros, IRepository<Loja> lojas)
    {
        _carros = carros;
        _lojas = lojas;
    }

    public IActionResult Index()
    {
        var items = _carros.GetAll().Select(c =>
        {
            c.Loja = _lojas.GetById(c.LojaId);
            return c;
        });
        return View(items);
    }

    public IActionResult Details(int id)
    {
        var carro = _carros.GetById(id);
        if (carro == null) return NotFound();
        carro.Loja = _lojas.GetById(carro.LojaId);
        return View(carro);
    }

    public IActionResult Create()
    {
        ViewBag.Lojas = GetLojasSelectList();
        return View(new Carro());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Carro carro)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Lojas = GetLojasSelectList();
            return View(carro);
        }
        _carros.Add(carro);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var carro = _carros.GetById(id);
        if (carro == null) return NotFound();
        ViewBag.Lojas = GetLojasSelectList();
        return View(carro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Carro carro)
    {
        if (id != carro.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            ViewBag.Lojas = GetLojasSelectList();
            return View(carro);
        }
        _carros.Update(carro);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var carro = _carros.GetById(id);
        if (carro == null) return NotFound();
        carro.Loja = _lojas.GetById(carro.LojaId);
        return View(carro);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _carros.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    private List<SelectListItem> GetLojasSelectList() => _lojas.GetAll()
        .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.Nome })
        .ToList();
}
