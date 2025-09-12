using System.ComponentModel.DataAnnotations;

namespace ControleEstacionamento.Models
{
    public class Veiculo
    {
        [Key]
        public string Placa { get; set; }
        public string Modelo { get; set; }
    }
}