using Microsoft.AspNetCore.Mvc;

namespace condominioApi.Controllers
{
    [ApiController]
    public class IndexController : ControllerBase
    {
        [HttpGet("index")]
        public ContentResult Index()
        {
            var content = "<html><body><h1>Hello World</h1><p>Some text</p></body></html>";
            return new ContentResult()
            {
                Content = content,
                ContentType = "text/html",
            };
        }
    }
}