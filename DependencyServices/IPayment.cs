using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using condominioApi.Models;

namespace condominioApi.DependencyService
{
    public interface IPaymentService
    {        
        bool isTokenExpiration();
        ActionResult Payment(string cardHash, HttpRequest request);
        dynamic getAccessToken();
        dynamic tokenizacao(string cardHash, JunoAccessToken junoAccessToken);
        dynamic subscription(HttpRequest requestUser, JunoAccessToken junoAccessToken, JunoTokenizacao junoTokenizacao);
        ActionResult modelResponse(dynamic modelJuno);
        ActionResult consultCharges();
    }
}