using Microsoft.AspNetCore.Mvc;
using condominioApi.Models;
using condominioApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace condominioApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class CondominioControllerSite : ControllerBase
    {
        private readonly CondominioService _condominioService;
        public CondominioControllerSite(CondominioService condominioService)
        {
            _condominioService = condominioService;
        }

        // UsuarioAdm vai se cadastrar e cadastrar o banco
        [HttpPost("cadastroCondominio")]
        public ActionResult<dynamic> RegisterCondominio([FromForm] UserAdm user) =>
           _condominioService.RegisterCondominio(user, Request);

        // o user adm vai logar por aqui
        [HttpPost("loginCondominio")]
        public ActionResult<dynamic> LoginCondominio([FromForm] UserGeneric user) => _condominioService.LoginCondominio(user, Request);

        [HttpPost("cadastroPorteiro")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<dynamic> RegisterPorteiro([FromForm] UserPorteiro user) =>
           _condominioService.RegisterPorteiro(user, Request);

        //adm vai cadastrar por essa rota
        [HttpPost("cadastroMorador")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<dynamic> RegisterMorador([FromForm] UserMorador user) => _condominioService.RegisterMorador(user, Request);

        [HttpPost("cadastroAvisos")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<dynamic> CadastroAvisos([FromForm] Aviso texto) => _condominioService.CadastroAvisos(texto, Request);

        [HttpPost("criarAgendamento")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<dynamic> CriarAgendamento([FromForm] Agendamento agend) => _condominioService.CriarAgendamento(agend, Request);
        
        [HttpPost("enviarfoto")]
        public ActionResult<dynamic> EnviarFoto() => _condominioService.EnviarFoto(Request);



    }
}