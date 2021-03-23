using Microsoft.AspNetCore.Mvc;
using condominioApi.Models;
using condominioApi.Services;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace condominioApi.Controllers
{
    [Route("app")]
    [ApiController]
    public class CondominioControllerApp : ControllerBase
    {
        private readonly CondominioService _condominioService;
        public CondominioControllerApp(CondominioService condominioService)
        {
            _condominioService = condominioService;
        }

        // Usuariomorador vai se Logar
        [HttpPost("loginMorador")]
        public ActionResult<string> LoginMorador([FromForm] UserGenericLogin user) => _condominioService.LoginMorador(user, Request);

        [HttpGet("listacondominios")]
        public ActionResult<dynamic> GetListNameDatabase() => _condominioService.GetListNameDatabase();

        [HttpPost("listaagendamentos")]
        //[Authorize(Roles = "Morador")]
        public ActionResult<dynamic> ListaAgendamentos() => _condominioService.ListaAgendamentos(Request);

        [HttpPost("cadastrarAgendamento")]
        [Authorize(Roles = "Morador")]
        public ActionResult<dynamic> CadastrarAgendamento([FromForm] CriacaoAgendamento agend, [FromForm] [Required] string name) => _condominioService.CadastrarAgendamento(agend,name, Request);
    }
}