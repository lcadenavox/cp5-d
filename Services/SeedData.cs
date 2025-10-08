using cp5_d.Models;

namespace cp5_d.Services;

public static class SeedData
{
    public static void Seed(IRepository<Loja> lojas, IRepository<Carro> carros, IRepository<Vendedor> vendedores)
    {
        if (lojas.GetAll().Any()) return;

        var l1 = lojas.Add(new Loja { Nome = "Loja Centro", Endereco = "Rua A, 123", Telefone = "(11) 1111-1111" });
        var l2 = lojas.Add(new Loja { Nome = "Loja Norte", Endereco = "Av. B, 456", Telefone = "(11) 2222-2222" });

        carros.Add(new Carro { Marca = "Toyota", Modelo = "Corolla", Ano = 2020, Preco = 95000, LojaId = l1.Id });
        carros.Add(new Carro { Marca = "Honda", Modelo = "Civic", Ano = 2019, Preco = 88000, LojaId = l1.Id });
        carros.Add(new Carro { Marca = "Ford", Modelo = "Fiesta", Ano = 2018, Preco = 45000, LojaId = l2.Id });

        vendedores.Add(new Vendedor { Nome = "Ana Silva", Email = "ana@loja.com", Telefone = "(11) 9999-0001", LojaId = l1.Id });
        vendedores.Add(new Vendedor { Nome = "Carlos Souza", Email = "carlos@loja.com", Telefone = "(11) 9999-0002", LojaId = l2.Id });
    }
}
