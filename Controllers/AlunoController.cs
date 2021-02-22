using first_api.Models;
using first_api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace first_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlunoController : ControllerBase
    {
        private readonly AlunoService _alunoService;

        public AlunoController(AlunoService alunoService)
        {
            _alunoService = alunoService;
        }

        [HttpGet]
        public ActionResult<List<Aluno>> Get() =>
            _alunoService.Get();

        [HttpGet("{id:length(24)}", Name = "GetAluno")]
        public ActionResult<Aluno> Get(string id)
        {
            var aluno = _alunoService.Get(id);

            if (aluno == null)
            {
                return NotFound();
            }

            return aluno;
        }

        [HttpPost]
        public ActionResult<Aluno> Create(Aluno aluno)
        {
            _alunoService.Create(aluno);

            return CreatedAtRoute("GetBook", new { id = aluno.Id.ToString() }, aluno);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Aluno alunoIn)
        {
            var aluno = _alunoService.Get(id);

            if (aluno == null)
            {
                return NotFound();
            }

            _alunoService.Update(id, alunoIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var aluno = _alunoService.Get(id);

            if (aluno == null)
            {
                return NotFound();
            }

            _alunoService.Remove(aluno.Id);

            return NoContent();
        }
    }
}