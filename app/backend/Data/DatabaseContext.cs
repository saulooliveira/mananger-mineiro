using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data;

public class DatabaseContext : DbContext
{
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<PrintHistory> PrintHistories { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("Produtos");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Codigo)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Descricao)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Valor)
                .HasColumnType("REAL");

            entity.Property(e => e.Yield)
                .HasMaxLength(100);

            entity.Property(e => e.Categoria)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<PrintHistory>(entity =>
        {
            entity.ToTable("PrintHistories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DataHora)
                .IsRequired();

            entity.Property(e => e.ProdutoIds)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.NumeroFolhas)
                .IsRequired();

            entity.Property(e => e.QuantidadeProdutos)
                .IsRequired();
        });
    }
}
