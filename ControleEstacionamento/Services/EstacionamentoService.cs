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

        // Registra a entrada do veículo
        public void RegistrarEntrada(string placa, string modelo)
        {
            // Verifica se veículo já existe, se não, cria
            var veiculo = _context.Veiculos.Find(placa);
            if (veiculo == null)
            {
                veiculo = new Veiculo { Placa = placa, Modelo = modelo };
                _context.Veiculos.Add(veiculo);
            }

            // Verifica se já existe uma entrada sem saída para o veículo
            var entradaAberta = _context.Estacionamentos
                .Where(e => e.PlacaVeiculo == placa && e.DataSaida == null)
                .FirstOrDefault();

            if (entradaAberta != null)
            {
                throw new InvalidOperationException("Veículo já está dentro do estacionamento.");
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
                throw new InvalidOperationException("Veículo não está dentro do estacionamento.");
            }

            estacionamento.DataSaida = DateTime.Now;

            var tabelaPreco = _tabelaPrecoService.GetTabelaPrecoPorData(estacionamento.DataEntrada);
            if (tabelaPreco == null)
            {
                throw new InvalidOperationException("Tabela de preços não configurada para a data de entrada.");
            }

            estacionamento.ValorPago = CalcularValor(estacionamento.DataEntrada, estacionamento.DataSaida.Value, tabelaPreco);

            _context.SaveChanges();

            return estacionamento.ValorPago.Value;
        }

        private decimal CalcularValor(DateTime entrada, DateTime saida, TabelaPreco tabela)
        {
            var tempoTotal = saida - entrada;
            var minutosTotais = (int)tempoTotal.TotalMinutes;

            // Até 30 minutos → metade da primeira hora
            if (minutosTotais <= 30)
            {
                return tabela.ValorHoraInicial / 2;
            }

            // Calcula total de horas inteiras e minutos restantes
            int horasInteiras = minutosTotais / 60;
            int minutosRestantes = minutosTotais % 60;

            // Aplica tolerância de 10 minutos
            int horasCobradas = horasInteiras;
            if (minutosRestantes > 10)
            {
                horasCobradas++;
            }

            // Se não chegou a 1h cheia mas passou de 30min, cobra 1h
            if (horasCobradas == 0)
            {
                horasCobradas = 1;
            }

            // Valor final = hora inicial + adicionais
            decimal valor = tabela.ValorHoraInicial;
            if (horasCobradas > 1)
            {
                valor += (horasCobradas - 1) * tabela.ValorHoraAdicional;
            }

            return valor;
        }
    }
}
