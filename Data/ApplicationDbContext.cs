using cp5_d.Models;
using Microsoft.EntityFrameworkCore;

namespace cp5_d.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

    public DbSet<Carro> Carros => Set<Carro>();
    public DbSet<Loja> Lojas => Set<Loja>();
    public DbSet<Vendedor> Vendedores => Set<Vendedor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Loja>()
            .HasMany(l => l.Carros)
            .WithOne(c => c.Loja!)
            .HasForeignKey(c => c.LojaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Loja>()
            .HasMany(l => l.Vendedores)
            .WithOne(v => v.Loja!)
            .HasForeignKey(v => v.LojaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Carro>()
            .Property(c => c.Preco)
            .HasColumnType("decimal(18,2)");
    }
}
