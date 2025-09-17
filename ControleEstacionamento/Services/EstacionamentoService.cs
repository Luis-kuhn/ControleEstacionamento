using System;
using System.Linq;
using ControleEstacionamento.Data;
using ControleEstacionamento.Models;
using Microsoft.EntityFrameworkCore;

namespace ControleEstacionamento.Services
{
    public class EstacionamentoService
    {
        private readonly EstacionamentoContext _context;
        private readonly TabelaPrecoService _tabelaPrecoService;

        public EstacionamentoService(EstacionamentoContext context, TabelaPrecoService tabelaPrecoService)
        {
            _context = context;
            _tabelaPrecoService = tabelaPrecoService;
        }

        // Registra a entrada do ve�culo
        public void RegistrarEntrada(string placa, string modelo)
        {
            // Verifica se ve�culo j� existe, se n�o, cria
            var veiculo = _context.Veiculos.Find(placa);
            if (veiculo == null)
            {
                veiculo = new Veiculo { Placa = placa, Modelo = modelo };
                _context.Veiculos.Add(veiculo);
            }

            // Verifica se j� existe uma entrada sem sa�da para o ve�culo
            var entradaAberta = _context.Estacionamentos
                .Where(e => e.PlacaVeiculo == placa && e.DataSaida == null)
                .FirstOrDefault();

            if (entradaAberta != null)
            {
                throw new InvalidOperationException("Ve�culo j� est� dentro do estacionamento.");
            }

            // Registra nova entrada
            var estacionamento = new Estacionamento
            {
                PlacaVeiculo = placa,
                DataEntrada = DateTime.Now
            };

            _context.Estacionamentos.Add(estacionamento);
            _context.SaveChanges();
        }

        public decimal RegistrarSaida(string placa)
        {
            var estacionamento = _context.Estacionamentos
                .Include(e => e.Veiculo)
                .Where(e => e.PlacaVeiculo == placa && e.DataSaida == null)
                .FirstOrDefault();

            if (estacionamento == null)
            {
                throw new InvalidOperationException("Ve�culo n�o est� dentro do estacionamento.");
            }

            estacionamento.DataSaida = DateTime.Now;

            var tabelaPreco = _tabelaPrecoService.GetTabelaPrecoPorData(estacionamento.DataEntrada);
            if (tabelaPreco == null)
            {
                throw new InvalidOperationException("Tabela de pre�os n�o configurada para a data de entrada.");
            }

            estacionamento.ValorPago = CalcularValor(estacionamento.DataEntrada, estacionamento.DataSaida.Value, tabelaPreco);

            _context.SaveChanges();

            return estacionamento.ValorPago.Value;
        }

        private decimal CalcularValor(DateTime entrada, DateTime saida, TabelaPreco tabela)
        {
            var tempoTotal = saida - entrada;
            var minutos = tempoTotal.TotalMinutes;

            if (minutos <= 30)
            {
                return tabela.ValorHoraInicial / 2;
            }

            // Calcula horas totais arredondando para cima considerando toler�ncia de 10 minutos por hora
            // Exemplo: 1h10min = 1 hora cobrada, 1h15min = 2 horas cobradas

            // Calcula horas inteiras e minutos restantes
            int horas = (int)tempoTotal.TotalHours;
            int minutosRestantes = tempoTotal.Minutes;

            // Aplica toler�ncia: se minutosRestantes <= 10, n�o conta hora adicional
            // se minutosRestantes > 10, conta como hora adicional
            int horasAdicionais = 0;
            if (minutosRestantes > 10)
            {
                horasAdicionais = 1;
            }

            int totalHorasCobradas = horas + horasAdicionais;

            // O valor �: valor da hora inicial + (horas adicionais * valor hora adicional)
            // Se totalHorasCobradas == 0 (menos de 1h), cobra valor da hora inicial
            if (totalHorasCobradas == 0)
            {
                totalHorasCobradas = 1;
            }

            decimal valor = tabela.ValorHoraInicial;
            if (totalHorasCobradas > 1)
            {
                valor += (totalHorasCobradas - 1) * tabela.ValorHoraAdicional;
            }

            return valor;
        }
    }
}