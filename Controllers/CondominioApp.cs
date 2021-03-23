using Microsoft.AspNetCore.Mvc;
using condominioApp.Models;
using condominioApp.Services;
using Microsoft.AspNetCore.Authorization;

namespace condominioApp.Controllers
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
        public ActionResult<string> LoginMorador([FromForm] UserGenericLogin user) =>
           _condominioService.LoginMorador(user, Request);

        [HttpGet("listacondominios")]
        public ActionResult<dynamic> GetListNameDatabase() => _condominioService.GetListNameDatabase();
    }
}