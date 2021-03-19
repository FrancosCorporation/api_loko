using Microsoft.AspNetCore.Mvc;
using first_api.Services;
using first_api.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace first_api.Controllers
{
    [Route("payment")]
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

    }
}