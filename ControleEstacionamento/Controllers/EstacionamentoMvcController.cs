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

        public EstacionamentoMvcController(EstacionamentoContext context, TabelaPrecoService tabelaPrecoService)
        {
            _context = context;
            _tabelaPrecoService = tabelaPrecoService;
        }

        public IActionResult Index()
        {
            // Busca todas as entradas (abertas e fechadas)
            var estacionamentos = _context.Estacionamentos
                .OrderByDescending(e => e.DataEntrada)
                .ToList();

            // Monta uma lista de view models para a view
            var model = estacionamentos.Select(e =>
            {
                DateTime dataSaida = e.DataSaida ?? DateTime.Now;
                TimeSpan duracao = dataSaida - e.DataEntrada;

                // Busca tabela de preço válida para a data de entrada
                var tabelaPreco = _tabelaPrecoService.GetTabelaPrecoPorData(e.DataEntrada);

                // Calcula tempo cobrado em horas conforme regra
                int tempoCobradoHoras = CalcularTempoCobradoHoras(e.DataEntrada, e.DataSaida);

                // Calcula valor a pagar conforme regra
                decimal? valorAPagar = null;
                if (e.DataSaida.HasValue && tabelaPreco != null)
                {
                    valorAPagar = CalcularValor(e.DataEntrada, e.DataSaida.Value, tabelaPreco);
                }

                return new EstacionamentoViewModel
                {
                    Placa = e.PlacaVeiculo,
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

        // Método para calcular tempo cobrado em horas conforme regra da tolerância
        private int CalcularTempoCobradoHoras(DateTime entrada, DateTime? saida)
        {
            if (!saida.HasValue)
                saida = DateTime.Now;

            var tempoTotal = saida.Value - entrada;
            var minutos = tempoTotal.TotalMinutes;

            if (minutos <= 30)
                return 1; // cobra meia hora, mas para exibir consideramos 1 hora cobrada

            int horas = (int)tempoTotal.TotalHours;
            int minutosRestantes = tempoTotal.Minutes;

            int horasAdicionais = minutosRestantes > 10 ? 1 : 0;

            int totalHoras = horas + horasAdicionais;

            if (totalHoras == 0)
                totalHoras = 1;

            return totalHoras;
        }

        // Método para calcular valor conforme regra (mesmo da service)
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

    // ViewModel para a view
    public class EstacionamentoViewModel
    {
        public string Placa { get; set; }
        public DateTime HorarioEntrada { get; set; }
        public DateTime? HorarioSaida { get; set; }
        public TimeSpan Duracao { get; set; }
        public int TempoCobradoHoras { get; set; }
        public decimal? PrecoHoraInicial { get; set; }
        public decimal? PrecoHoraAdicional { get; set; }
        public decimal? ValorAPagar { get; set; }
    }
}