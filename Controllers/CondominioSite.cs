using Microsoft.AspNetCore.Mvc;
using condominio_api.Models;
using condominio_api.Services;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace condominio_api.Controllers
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

        [HttpPost("cadastroComunicado")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<dynamic> CadastroComunicado([FromForm] Aviso texto) => _condominioService.CadastroComunicado(texto, Request);

        [HttpPost("criarAgendamento")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<dynamic> CriarAgendamento([FromForm] Agendamento agend) => _condominioService.CriarAgendamento(agend, Request);

        [HttpPost("editFoto")]
        public ActionResult<dynamic> EnviarFoto([FromForm] byte[] foto) => _condominioService.EnviarFoto(foto, Request);

        [HttpPut("editAgendamento")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<dynamic> EditarAgendamento([FromForm] Agendamento agend) => _condominioService.EditarAgendamento(agend, Request);

        [HttpGet("listaAgendamentos")]
        public ActionResult<dynamic> ListaAgendamentos() => _condominioService.ListaAgendamentos(Request);

        [HttpGet("listaItensAgendamentos")]
        public ActionResult<dynamic> ListaItensAgendamentos([FromForm] ObjectBase obj) => _condominioService.ListaItensAgendamentos(obj, Request);

        [HttpGet("configAgendamentos")]
        public ActionResult<dynamic> GetConfigAgendamentos([FromForm] ObjectBase obj) => _condominioService.GetConfigAgendamentos(obj, Request);

        [HttpGet("comunicados")]
        public ActionResult<dynamic> GetAgendamento() => _condominioService.GetAgendamento(Request);

        [HttpGet("confirmacaoEmail")]
        public ActionResult<dynamic> ConfirmacaoEmail([FromQuery][Required] string token) => _condominioService.ConfirmacaoEmail(token, Request);

        [HttpPost("EmailNaoConfirmado")]
        public ActionResult<dynamic> EmailNaoConfirmado([FromForm][Required] ConfirmacaoEmail confirm) => _condominioService.EmailNaoConfirmado(confirm, Request);


        [HttpGet("recuperarSenhaCondominio")]
        public ActionResult<dynamic> EditarSenha([FromQuery][Required] string token, [FromQuery][Required] string senha) => _condominioService.EditarSenha(token, senha, Request);

        [HttpPut("alterarSenha")]
        public ActionResult<dynamic> AlterarSenha([FromQuery][Required] string senha) => _condominioService.AlterarSenha(senha, Request);


        [HttpPost("esqueciMinhaSenha")]
        public ActionResult<dynamic> RedefinirSenha([FromForm][Required] RedefinirSenha red) => _condominioService.RedefinirSenha(red, Request);

    }
}