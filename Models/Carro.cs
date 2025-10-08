using System.ComponentModel.DataAnnotations;

namespace cp5_d.Models;

public class Carro : IEntity
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Modelo { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Marca { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int Ano { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Preco { get; set; }

    [Display(Name = "Loja")]
    public int LojaId { get; set; }

    public Loja? Loja { get; set; }
}
