using cp5_d.Models;
using cp5_d.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace cp5_d.Controllers;

public class VendedoresController : Controller
{
    private readonly IRepository<Vendedor> _vendedores;
    private readonly IRepository<Loja> _lojas;

    public VendedoresController(IRepository<Vendedor> vendedores, IRepository<Loja> lojas)
    {
        _vendedores = vendedores;
        _lojas = lojas;
    }

    public IActionResult Index()
    {
        var items = _vendedores.GetAll().Select(v =>
        {
            v.Loja = _lojas.GetById(v.LojaId);
            return v;
        });
        return View(items);
    }

    public IActionResult Details(int id)
    {
        var vendedor = _vendedores.GetById(id);
        if (vendedor == null) return NotFound();
        vendedor.Loja = _lojas.GetById(vendedor.LojaId);
        return View(vendedor);
    }

    public IActionResult Create()
    {
        ViewBag.Lojas = GetLojasSelectList();
        return View(new Vendedor());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Vendedor vendedor)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Lojas = GetLojasSelectList();
            return View(vendedor);
        }
        _vendedores.Add(vendedor);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var vendedor = _vendedores.GetById(id);
        if (vendedor == null) return NotFound();
        ViewBag.Lojas = GetLojasSelectList();
        return View(vendedor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Vendedor vendedor)
    {
        if (id != vendedor.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            ViewBag.Lojas = GetLojasSelectList();
            return View(vendedor);
        }
        _vendedores.Update(vendedor);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var vendedor = _vendedores.GetById(id);
        if (vendedor == null) return NotFound();
        vendedor.Loja = _lojas.GetById(vendedor.LojaId);
        return View(vendedor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _vendedores.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    private List<SelectListItem> GetLojasSelectList() => _lojas.GetAll()
        .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.Nome })
        .ToList();
}
