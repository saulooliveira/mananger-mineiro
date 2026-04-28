using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data;

public class DatabaseContext : DbContext
{
    public DbSet<Produto> Produtos { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).IsRequired();
            entity.Property(e => e.Descricao).IsRequired();
            entity.Property(e => e.Valor).HasColumnType("REAL");
            entity.Property(e => e.QuantidadeImpresa).HasDefaultValue(0);
        });
    }
}
