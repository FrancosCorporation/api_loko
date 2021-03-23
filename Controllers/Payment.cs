using Microsoft.AspNetCore.Mvc;
using condominioApi.Services;
using System.ComponentModel.DataAnnotations;

namespace condominioApi.Controllers
{
    [Route("subscription")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public ActionResult<dynamic> payment([FromForm] [Required] string cardHash) => _paymentService.Payment(cardHash);

        [HttpGet("consult")]
        public ActionResult<dynamic> consultSubscription([FromForm] [Required] string idSubscription) => _paymentService.consultCharges(idSubscription);

    }
}