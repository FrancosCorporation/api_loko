using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Newtonsoft.Json;  
using System.Collections.Generic;
using first_api.Models;
using System.Net;

namespace first_api.Services
{
    public class PaymentService : ControllerBase
    {
        //Access token
        //Gerar id card
        //Verificar plano
        //Assinar plano

        private string _accessToken;

        public PaymentService() {
            _accessToken = getAccessToken().access_token;
        }
        private string _baseUrl = "https://sandbox.boletobancario.com";
        private string _planId = "pln_6950A5BBD2696FB2";

        public JunoAccessToken getAccessToken() {
            var client = new RestClient(_baseUrl+"/authorization-server/oauth/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Authorization", "Basic a2MxajR6bEdWM1FSdkFYbTpTZ2oyVzdGMXF3MWxhNWRHOytQVkVwak1lJHRqKSxkaA==");
            request.AddParameter("grant_type", "client_credentials");
            IRestResponse response = client.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK) {
                JunoAccessToken json = JsonConvert.DeserializeObject<JunoAccessToken>(response.Content);
                return json;
            }
            return new JunoAccessToken();
        }

        public JunoTokenizacao tokenizacao(string cardHash)
        {
            var client = new RestClient(_baseUrl+"/api-integration/credit-cards/tokenization");
            var request = new RestRequest(Method.POST);
            request.AddHeaders(new Dictionary<string, string>() {
                {"Authorization", "Bearer "+_accessToken},
                {"X-Api-Version", "2"},
                {"X-Resource-Token", "85D4CF242645507CEE7332F4451BCBF398027EEA65B36135EA64B99036DD90D4"},
                {"Content-Type", "application/json;charset=UTF-8"}
            });
            request.AddJsonBody(new { creditCardHash = cardHash});

            JunoTokenizacao response = JsonConvert.DeserializeObject<JunoTokenizacao>(client.Execute(request).Content);
            return response;
        }

        public dynamic subscription(string cardHash) {
            var client = new RestClient(_baseUrl+"/api-integration/subscriptions");
            var request = new RestRequest(Method.POST);
            JunoTokenizacao junoTokenizacao = tokenizacao(cardHash);
            if(junoTokenizacao.creditCardId != null) {
                request.AddHeaders(new Dictionary<string, string>() {
                    {"Authorization", "Bearer "+_accessToken},
                    {"X-Api-Version", "2"},
                    {"X-Resource-Token", "85D4CF242645507CEE7332F4451BCBF398027EEA65B36135EA64B99036DD90D4"},
                    {"Content-Type", "application/json;charset=UTF-8"}
                });
                request.AddJsonBody(new {
                    dueDay = 21,
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

                JunoSubscription response = JsonConvert.DeserializeObject<JunoSubscription>(client.Execute(request).Content);
                return response;
            }
            return new JunoSubscription();
        }

        public ActionResult Payment(string cardHash)
        {
            var res = subscription(cardHash);
            return Ok(res);
        }
    }
}