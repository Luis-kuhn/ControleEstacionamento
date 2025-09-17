using System;
using Microsoft.AspNetCore.Mvc;
using ControleEstacionamento.Data;
using ControleEstacionamento.Models;
using System.Linq;

namespace ControleEstacionamento.Controllers
{
	[ApiController]
	[Route("api/tabelapreco")]
	public class TabelaPrecoController : ControllerBase
	{
		private readonly EstacionamentoContext _context;

		public TabelaPrecoController(EstacionamentoContext context)
		{
			_context = context;
		}

		// GET api/tabelapreco
		[HttpGet]
		public IActionResult Listar()
		{
			var tabelas = _context.TabelaPrecos.ToList();
			return Ok(tabelas);
		}

		// POST api/tabelapreco
		[HttpPost]
		public IActionResult Criar([FromBody] TabelaPreco tabelaPreco)
		{
			if (tabelaPreco == null)
				return BadRequest("Dados inválidos.");

			if (tabelaPreco.DataInicio >= tabelaPreco.DataFim)
				return BadRequest("Data de início deve ser anterior à data de fim.");

			_context.TabelaPrecos.Add(tabelaPreco);
			_context.SaveChanges();

			return Ok("Tabela de preços criada com sucesso.");
		}

		// PUT api/tabelapreco/{id}
		[HttpPut("{id}")]
		public IActionResult Atualizar(int id, [FromBody] TabelaPreco tabelaPreco)
		{
			var tabelaExistente = _context.TabelaPrecos.Find(id);
			if (tabelaExistente == null)
				return NotFound("Tabela de preços não encontrada.");

			if (tabelaPreco.DataInicio >= tabelaPreco.DataFim)
				return BadRequest("Data de início deve ser anterior à data de fim.");

			tabelaExistente.DataInicio = tabelaPreco.DataInicio;
			tabelaExistente.DataFim = tabelaPreco.DataFim;
			tabelaExistente.ValorHoraInicial = tabelaPreco.ValorHoraInicial;
			tabelaExistente.ValorHoraAdicional = tabelaPreco.ValorHoraAdicional;

			_context.SaveChanges();

			return Ok("Tabela de preços atualizada com sucesso.");
		}
	}
}