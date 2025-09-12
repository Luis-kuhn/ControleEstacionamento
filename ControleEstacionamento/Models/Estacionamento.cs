using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleEstacionamento.Models
{
    public class Estacionamento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PlacaVeiculo { get; set; }

        [ForeignKey("PlacaVeiculo")]
        public Veiculo Veiculo { get; set; }

        public DateTime DataEntrada { get; set; }
        public DateTime? DataSaida { get; set; }
        public decimal? ValorPago { get; set; }
    }
}