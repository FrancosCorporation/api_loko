using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Newtonsoft.Json;  
using System.Collections.Generic;
using condominioApi.Models;
using System.Net;
using System;

namespace condominioApi.Services
{
    public class PaymentService : ControllerBase
    {
        //Access token
        //Gerar id card
        //Verificar plano
        //Assinar plano

        private JunoAccessToken _validateAccessToken = new JunoAccessToken() {dateTimeGenerateAccessToken = new DateTime(1970)};

        private string _baseUrl = "https://sandbox.boletobancario.com";

        private string _planId = "pln_6950A5BBD2696FB2";

        public bool isTokenExpiration()
        {
            if(DateTimeOffset.UtcNow.ToUnixTimeSeconds()-_validateAccessToken.dateTimeGenerateAccessToken.ToUnixTimeSeconds() > 3600) return true;
            return false;
        }

        public dynamic getAccessToken()
        {
            if(isTokenExpiration()) {
                var client = new RestClient($"{_baseUrl}/authorization-server/oauth/token");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Authorization", "Basic a2MxajR6bEdWM1FSdkFYbTpTZ2oyVzdGMXF3MWxhNWRHOytQVkVwak1lJHRqKSxkaA==");
                request.AddParameter("grant_type", "client_credentials");
                
                IRestResponse response = client.Execute(request);
                if(response.StatusCode == HttpStatusCode.OK) {
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
                {"X-Resource-Token", "85D4CF242645507CEE7332F4451BCBF398027EEA65B36135EA64B99036DD90D4"},
                {"Content-Type", "application/json;charset=UTF-8"}
            });
            request.AddJsonBody(new { creditCardHash = cardHash});

            IRestResponse response = client.Execute(request);

            if(response.StatusCode == HttpStatusCode.OK) {
                JunoTokenizacao _junoTokenizacao = JsonConvert.DeserializeObject<JunoTokenizacao>(response.Content);
                return _junoTokenizacao;
            }
            JunoError _junoError = JsonConvert.DeserializeObject<JunoError>(response.Content);
            return _junoError;
        }

        public dynamic subscription(JunoAccessToken junoAccessToken ,JunoTokenizacao junoTokenizacao)
        {
            var client = new RestClient($"{_baseUrl}/api-integration/subscriptions");
            var request = new RestRequest(Method.POST);
            request.AddHeaders(new Dictionary<string, string>() {
                {"Authorization", $"Bearer {junoAccessToken.access_token}"},
                {"X-Api-Version", "2"},
                {"X-Resource-Token", "85D4CF242645507CEE7332F4451BCBF398027EEA65B36135EA64B99036DD90D4"},
                {"Content-Type", "application/json;charset=UTF-8"}
            });
            request.AddJsonBody(new {
                dueDay = DateTime.Now.Day,
                planId = _planId,
                chargeDescription = "Inscrição plano Condominio",
                creditCardDetails = new {
                    creditCardId = junoTokenizacao.creditCardId,
                },
                billing = new {
                    name = "Matheus",
                    document = "51776993071",
                    email = "mathlouly@gmail.com",
                    address = new {
                        street = "jamel Cecilio",
                        number = "N/A",
                        city = "Goiania",
                        state = "GO",
                        postCode = "74840540"
                    },
                }
            });

            IRestResponse response = client.Execute(request);

            if(response.StatusCode == HttpStatusCode.OK) {
                JunoSubscription _junoSubscription = JsonConvert.DeserializeObject<JunoSubscription>(response.Content);
                return _junoSubscription;
            }
            JunoError _junoError = JsonConvert.DeserializeObject<JunoError>(response.Content);
            return _junoError;

        }

        public ActionResult modelResponse(dynamic modelJuno)
        {
            switch(modelJuno.status) {
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

        public dynamic consultCharges(string idSubscription)
        {
            dynamic _getAccessToken = getAccessToken();
            if(_getAccessToken is JunoError) return _getAccessToken;

            var client = new RestClient($"{_baseUrl}/api-integration/charges");
            var request = new RestRequest(Method.GET);
            request.AddHeaders(new Dictionary<string, string>() {
                {"Authorization", $"Bearer {_getAccessToken.access_token}"},
                {"X-Api-Version", "2"},
                {"X-Resource-Token", "85D4CF242645507CEE7332F4451BCBF398027EEA65B36135EA64B99036DD90D4"},
                {"Content-Type", "application/json;charset=UTF-8"}
            });

            IRestResponse response = client.Execute(request);

            if(response.StatusCode == HttpStatusCode.OK)
            {
                JunoEmbedded _junoEmbedded = JsonConvert.DeserializeObject<JunoEmbedded>(response.Content);
                JunoCharges _junoCharges = _junoEmbedded._embedded;

                foreach (var charge in _junoCharges.charges)
                {
                    if(idSubscription == charge.subscription.id)
                    {
                        return Ok(charge.status);
                    }
                }

                return Ok(_junoCharges.charges);
            }
            
            JunoError _junoError = JsonConvert.DeserializeObject<JunoError>(response.Content);

            return modelResponse(_junoError);
        }

        public ActionResult Payment(string cardHash)
        {
            dynamic _modelJuno;

            dynamic _getAccessToken = getAccessToken();
            _modelJuno = _getAccessToken;

            if(!(_modelJuno is JunoError)) {
                dynamic _tokenizacao = tokenizacao(cardHash, _getAccessToken);
                _modelJuno = _tokenizacao;

                if(!(_modelJuno is JunoError)) {
                    dynamic _subscription = subscription(_getAccessToken, _tokenizacao);
                    _modelJuno = _subscription;
                }
            }

            return modelResponse(_modelJuno);
        }
    }
}