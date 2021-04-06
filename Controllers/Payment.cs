using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using condominioApi.DependencyService;

namespace condominioApi.Controllers
{
    [Route("subscription")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult<dynamic> payment([FromForm] [Required] string cardHash) => _paymentService.Payment(cardHash,Request);

        [HttpGet("Consult")]
        public ActionResult<dynamic> consult() => _paymentService.consultCharges();

    }
}