using System.ComponentModel.DataAnnotations;

namespace cp5_d.Models;

public class Loja : IEntity
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Endereco { get; set; }

    [Phone]
    public string? Telefone { get; set; }

    public ICollection<Carro> Carros { get; set; } = new List<Carro>();
    public ICollection<Vendedor> Vendedores { get; set; } = new List<Vendedor>();
}
