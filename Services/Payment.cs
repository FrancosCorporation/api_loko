using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Newtonsoft.Json;
using System.Collections.Generic;
using condominioApi.Models;
using System.Net;
using System;
using Microsoft.AspNetCore.Http;
using condominioApi.DependencyService;

namespace condominioApi.Services
{
    public class PaymentService : ControllerBase, IPaymentService
    {
        //Access token
        //Gerar id card
        //Verificar plano
        //Assinar plano
        private readonly IUserService _userSevice;

        public PaymentService(IUserService userService)
        {
            _userSevice = userService;
        }

        private JunoAccessToken _validateAccessToken = new JunoAccessToken() { dateTimeGenerateAccessToken = new DateTime(1970) };

        private string _baseUrl = "https://sandbox.boletobancario.com";

        public bool isTokenExpiration()
        {
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _validateAccessToken.dateTimeGenerateAccessToken.ToUnixTimeSeconds() > 3600) return true;
            return false;
        }

        public ActionResult Payment(string cardHash, HttpRequest request)
        {
            dynamic _modelJuno;

            dynamic _getAccessToken = getAccessToken();
            _modelJuno = _getAccessToken;

            if (!(_modelJuno is JunoError))
            {
                dynamic _tokenizacao = tokenizacao(cardHash, _getAccessToken);
                _modelJuno = _tokenizacao;

                if (!(_modelJuno is JunoError))
                {
                    dynamic _subscription = subscription(request, _getAccessToken, _tokenizacao);
                    _modelJuno = _subscription;
                }
            }

            return modelResponse(_modelJuno);
        }

        public dynamic getAccessToken()
        {
            if (isTokenExpiration())
            {
                var client = new RestClient($"{_baseUrl}/authorization-server/oauth/token");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Authorization", Settings.Authorization );
                request.AddParameter("grant_type", "client_credentials");

                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JunoAccessToken _junoAccessToken = JsonConvert.DeserializeObject<JunoAccessToken>(response.Content);
                    _junoAccessToken.dateTimeGenerateAccessToken = DateTimeOffset.UtcNow;
                    _validateAccessToken = _junoAccessToken;
                    return _junoAccessToken;
                }
                JunoError _junoError = JsonConvert.DeserializeObject<JunoError>(response.Content);
                return _junoError;
            }
            return _validateAccessToken;
        }

        public dynamic tokenizacao(string cardHash, JunoAccessToken junoAccessToken)
        {
            var client = new RestClient($"{_baseUrl}/api-integration/credit-cards/tokenization");
            var request = new RestRequest(Method.POST);
            request.AddHeaders(new Dictionary<string, string>() {
                {"Authorization", $"Bearer {junoAccessToken.access_token}"},
                {"X-Api-Version", "2"},
                {"X-Resource-Token", Settings.Token},
                {"Content-Type", "application/json;charset=UTF-8"}
            });
            request.AddJsonBody(new { creditCardHash = cardHash });

            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                JunoTokenizacao _junoTokenizacao = JsonConvert.DeserializeObject<JunoTokenizacao>(response.Content);
                return _junoTokenizacao;
            }
            JunoError _junoError = JsonConvert.DeserializeObject<JunoError>(response.Content);
            return _junoError;
        }

        public dynamic subscription(HttpRequest requestUser, JunoAccessToken junoAccessToken, JunoTokenizacao junoTokenizacao)
        {
            UserAdm user = _userSevice.RetornaUserAdmPorId(_userSevice.UnGenereteToken(requestUser)["nameCondominio"].ToString(), _userSevice.UnGenereteToken(requestUser)["objectId"].ToString());
            user.creditCardId = junoTokenizacao.creditCardId;
            var client = new RestClient($"{_baseUrl}/api-integration/subscriptions");
            var request = new RestRequest(Method.POST);
            request.AddHeaders(new Dictionary<string, string>() {
                {"Authorization", $"Bearer {junoAccessToken.access_token}"},
                {"X-Api-Version", "2"},
                {"X-Resource-Token", Settings.Token},
                {"Content-Type", "application/json;charset=UTF-8"}
            });
            request.AddJsonBody(new
            {
                dueDay = DateTime.Now.Day,
                planId = Settings.PlanId,
                chargeDescription = "Inscrição plano Condominio",
                creditCardDetails = new
                {
                    creditCardId = junoTokenizacao.creditCardId,
                },
                billing = new
                {
                    name = user.nome,
                    document = user.cnpj,
                    email = user.email,
                    address = new
                    {
                        street = user.rua,
                        number = user.numero,
                        city = user.cidade,
                        state = user.estado,
                        postCode = user.cep
                    },
                }
            });

            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                JunoSubscription _junoSubscription = JsonConvert.DeserializeObject<JunoSubscription>(response.Content);
                user.idSubscription = _junoSubscription.id;
                _userSevice.GravaUserAdm(user);
                return _junoSubscription;
            }
            JunoError _junoError = JsonConvert.DeserializeObject<JunoError>(response.Content);
            return _junoError;

        }

        public ActionResult modelResponse(dynamic modelJuno)
        {
            switch (modelJuno.status)
            {
                case 400:
                    return BadRequest(modelJuno);
                case 401:
                    return Unauthorized(modelJuno);
                case 403:
                    return Forbid(modelJuno.details[0].message);
                case 500:
                    return Problem(modelJuno.details[0].message);
                default:
                    return Ok(modelJuno);
            }
        }

        public ActionResult consultCharges()
        {
            dynamic _getAccessToken = getAccessToken();

            var client = new RestClient($"{_baseUrl}/api-integration/charges");
            var request = new RestRequest(Method.GET);
            request.AddHeaders(new Dictionary<string, string>() {
                {"Authorization", $"Bearer {_getAccessToken.access_token}"},
                {"X-Api-Version", "2"},
                {"X-Resource-Token", Settings.Token},
                {"Content-Type", "application/json;charset=UTF-8"}
            });

            IRestResponse response = client.Execute(request);

            JunoEmbedded _junoEmbedded = JsonConvert.DeserializeObject<JunoEmbedded>(response.Content);
            JunoCharges _junoCharges = _junoEmbedded._embedded;

            return Ok(_junoCharges);

        }

    }
}