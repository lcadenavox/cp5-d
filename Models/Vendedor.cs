using System.ComponentModel.DataAnnotations;

namespace cp5_d.Models;

public class Vendedor : IEntity
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? Telefone { get; set; }

    [Display(Name = "Loja")]
    public int LojaId { get; set; }

    public Loja? Loja { get; set; }
}
