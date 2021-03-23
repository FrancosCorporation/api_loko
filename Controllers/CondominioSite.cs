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
        public ActionResult<string> RegisterCondominio([FromForm] UserAdm user) =>
           _condominioService.RegisterCondominio(user, Request);

        // o user adm vai logar por aqui
        [HttpPost("loginCondominio")]
        public ActionResult<dynamic> LoginCondominio([FromForm] UserGeneric user) => _condominioService.LoginCondominio(user, Request);

        [HttpPost("cadastroPorteiro")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<string> RegisterPorteiro([FromForm] UserPorteiro user) =>
           _condominioService.RegisterPorteiro(user, Request);

        //adm vai cadastrar por essa rota
        [HttpPost("cadastroMorador")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<dynamic> RegisterMorador([FromForm] UserMorador user) => _condominioService.RegisterMorador(user, Request);


        //[HttpGet("listacondominios")]
        //public ActionResult<dynamic> ListCollections() => _condominioService.GetListNameCollections();


        //[HttpGet("usercollection")]
        //public ActionResult<dynamic> UserCollection(string nameCondominio) => _condominioService.GetCondominios(nameCondominio);



        // o user do app vai logar por aqui
        //[HttpPost("loginapp")]
        //public ActionResult<dynamic> LoginUser([FromForm]string username, [FromForm]string password, [FromForm]string nameCondominio) => 
        // _condominioService.RegisterAdmw2(username: username, password: password, nameCondominio: nameCondominio);

        //aonde vai puxar para listar os condominios e os moradores vao poder escolher qual o condominio
        

    }
}