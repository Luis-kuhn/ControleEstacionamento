using System;
using System.ComponentModel.DataAnnotations;

namespace ControleEstacionamento.Models
{
    public class TabelaPreco
    {
        [Key]
        public int Id { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        public decimal ValorHoraInicial { get; set; }
        public decimal ValorHoraAdicional { get; set; }
    }
}