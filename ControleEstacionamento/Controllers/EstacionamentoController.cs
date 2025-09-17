using System;
using Microsoft.AspNetCore.Mvc;
using ControleEstacionamento.Services;
using ControleEstacionamento.Models;
using System.Linq;

namespace ControleEstacionamento.Controllers
{
    [ApiController]
    [Route("api/estacionamento")]
    public class EstacionamentoController : ControllerBase
    {
        private readonly EstacionamentoService _estacionamentoService;
        private readonly ControleEstacionamento.Data.EstacionamentoContext _context;

        public EstacionamentoController(EstacionamentoService estacionamentoService,
                                        ControleEstacionamento.Data.EstacionamentoContext context)
        {
            _estacionamentoService = estacionamentoService;
            _context = context;
        }

        // POST api/estacionamento/entrada
        [HttpPost("entrada")]
        public IActionResult RegistrarEntrada([FromBody] Veiculo veiculo)
        {
            if (veiculo == null || string.IsNullOrEmpty(veiculo.Placa))
                return BadRequest("Placa do veículo é obrigatória.");

            try
            {
                _estacionamentoService.RegistrarEntrada(veiculo.Placa, veiculo.Modelo);
                return Ok("Entrada registrada com sucesso.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/estacionamento/saida/{placa}
        [HttpPut("saida/{placa}")]
        public IActionResult RegistrarSaida(string placa)
        {
            if (string.IsNullOrEmpty(placa))
                return BadRequest("Placa do veículo é obrigatória.");

            try
            {
                var valor = _estacionamentoService.RegistrarSaida(placa);
                return Ok(new { Mensagem = "Saída registrada com sucesso.", ValorPago = valor });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/estacionamento/veiculos
        [HttpGet("veiculos")]
        public IActionResult ListarVeiculosEstacionados()
        {
            var veiculos = _context.Estacionamentos
                .Where(e => e.DataSaida == null)
                .Select(e => new
                {
                    e.PlacaVeiculo,
                    e.DataEntrada
                })
                .ToList();

            return Ok(veiculos);
        }
    }
}