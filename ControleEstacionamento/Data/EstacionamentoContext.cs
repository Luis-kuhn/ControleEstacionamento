using Microsoft.EntityFrameworkCore;
using ControleEstacionamento.Models;

namespace ControleEstacionamento.Data
{
    public class EstacionamentoContext : DbContext
    {
        public EstacionamentoContext(DbContextOptions<EstacionamentoContext> options) : base(options)
        {
        }

        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Estacionamento> Estacionamentos { get; set; }
        public DbSet<TabelaPreco> TabelaPrecos { get; set; }
    }
}