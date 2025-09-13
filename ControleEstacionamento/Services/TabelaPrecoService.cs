using System;
using System.Linq;
using ControleEstacionamento.Data;
using ControleEstacionamento.Models;

namespace ControleEstacionamento.Services
{
    public class TabelaPrecoService
    {
        private readonly EstacionamentoContext _context;

        public TabelaPrecoService(EstacionamentoContext context)
        {
            _context = context;
        }

        // Busca a tabela de preços válida para a data informada
        public TabelaPreco GetTabelaPrecoPorData(DateTime data)
        {
            return _context.TabelaPrecos
                .Where(t => t.DataInicio <= data && t.DataFim >= data)
                .OrderByDescending(t => t.DataInicio)
                .FirstOrDefault();
        }
    }
}