using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ControleEstacionamento.Data;
using ControleEstacionamento.Services;
using ControleEstacionamento.Models;
using System.Collections.Generic;

namespace ControleEstacionamento.Controllers
{
    public class EstacionamentoMvcController : Controller
    {
        private readonly EstacionamentoContext _context;
        private readonly TabelaPrecoService _tabelaPrecoService;
        private readonly EstacionamentoService _estacionamentoService;

        public EstacionamentoMvcController(EstacionamentoContext context, TabelaPrecoService tabelaPrecoService, EstacionamentoService estacionamentoService)
        {
            _context = context;
            _tabelaPrecoService = tabelaPrecoService;
            _estacionamentoService = estacionamentoService;
        }

        public IActionResult Index()
        {
            var estacionamentos = _context.Estacionamentos
                .OrderByDescending(e => e.DataEntrada)
                .ToList();

            var model = estacionamentos.Select(e =>
            {
                DateTime dataSaida = e.DataSaida ?? DateTime.Now;
                TimeSpan duracao = dataSaida - e.DataEntrada;

                var tabelaPreco = _tabelaPrecoService.GetTabelaPrecoPorData(e.DataEntrada);

                int tempoCobradoHoras = CalcularTempoCobradoHoras(e.DataEntrada, e.DataSaida);

                decimal? valorAPagar = null;
                if (e.DataSaida.HasValue && tabelaPreco != null)
                {
                    valorAPagar = CalcularValor(e.DataEntrada, e.DataSaida.Value, tabelaPreco);
                }

                return new EstacionamentoViewModel
                {
                    Id = e.Id,
                    Placa = e.PlacaVeiculo,
                    Modelo = e.Veiculo?.Modelo,
                    HorarioEntrada = e.DataEntrada,
                    HorarioSaida = e.DataSaida,
                    Duracao = duracao,
                    TempoCobradoHoras = tempoCobradoHoras,
                    PrecoHoraInicial = tabelaPreco?.ValorHoraInicial,
                    PrecoHoraAdicional = tabelaPreco?.ValorHoraAdicional,
                    ValorAPagar = valorAPagar
                };
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult RegistrarEntrada([FromForm] string placa, [FromForm] string modelo)
        {
            try
            {
                _estacionamentoService.RegistrarEntrada(placa, modelo);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErroEntrada"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult RegistrarSaida([FromForm] int estacionamentoId)
        {
            try
            {
                var estacionamento = _context.Estacionamentos.Find(estacionamentoId);
                if (estacionamento == null)
                {
                    TempData["ErroSaida"] = "Estacionamento não encontrado.";
                    return RedirectToAction("Index");
                }

                var valor = _estacionamentoService.RegistrarSaida(estacionamento.PlacaVeiculo);
                TempData["ValorPago"] = valor;
                TempData["PlacaSaida"] = estacionamento.PlacaVeiculo;
                TempData["ModeloSaida"] = estacionamento.Veiculo?.Modelo;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErroSaida"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        private int CalcularTempoCobradoHoras(DateTime entrada, DateTime? saida)
        {
            if (!saida.HasValue)
                saida = DateTime.Now;

            var tempoTotal = saida.Value - entrada;
            var minutos = tempoTotal.TotalMinutes;

            if (minutos <= 30)
                return 1;

            int horas = (int)tempoTotal.TotalHours;
            int minutosRestantes = tempoTotal.Minutes;

            int horasAdicionais = minutosRestantes > 10 ? 1 : 0;

            int totalHoras = horas + horasAdicionais;

            if (totalHoras == 0)
                totalHoras = 1;

            return totalHoras;
        }

        private decimal CalcularValor(DateTime entrada, DateTime saida, TabelaPreco tabela)
        {
            var tempoTotal = saida - entrada;
            var minutos = tempoTotal.TotalMinutes;

            if (minutos <= 30)
            {
                return tabela.ValorHoraInicial / 2;
            }

            int horas = (int)tempoTotal.TotalHours;
            int minutosRestantes = tempoTotal.Minutes;

            int horasAdicionais = minutosRestantes > 10 ? 1 : 0;

            int totalHorasCobradas = horas + horasAdicionais;

            if (totalHorasCobradas == 0)
                totalHorasCobradas = 1;

            decimal valor = tabela.ValorHoraInicial;
            if (totalHorasCobradas > 1)
            {
                valor += (totalHorasCobradas - 1) * tabela.ValorHoraAdicional;
            }

            return valor;
        }
    }

    public class EstacionamentoViewModel
    {
        public int Id { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public DateTime HorarioEntrada { get; set; }
        public DateTime? HorarioSaida { get; set; }
        public TimeSpan Duracao { get; set; }
        public int TempoCobradoHoras { get; set; }
        public decimal? PrecoHoraInicial { get; set; }
        public decimal? PrecoHoraAdicional { get; set; }
        public decimal? ValorAPagar { get; set; }
    }
}
