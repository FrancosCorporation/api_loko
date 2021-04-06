using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using System;
using condominioApi.Models;
using MongoDB.Driver;
using RestSharp;
using System.Collections.Generic;
using Newtonsoft.Json;
using condominioApi.DependencyService;

namespace condominioApi.Services
{
    public class CheckPayment : IHostedService
    {
        private Timer _timer;
        private readonly MongoClient _clientMongoDb;
        private readonly IPaymentService _paymentService;
        private string _baseUrl = "https://sandbox.boletobancario.com";

        public CheckPayment(ICondominioDatabaseSetting setting, IPaymentService paymentService)
        {
            _paymentService = paymentService;
            var client = new MongoClient(setting.ConnectionString);
            _clientMongoDb = client;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(consultCharges, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
            return Task.CompletedTask;
        }


        public void changeIsPayment(IMongoDatabase mongoDatabase, bool isPayment)
        {
            IMongoCollection<UserAdm> userAdmCollection = mongoDatabase.GetCollection<UserAdm>("usersAdm");
            UserAdm userAdm = userAdmCollection.Find<UserAdm>(userAdm => true).ToList()[0];

            var update = Builders<UserAdm>.Update.Set("isPayment", isPayment);
            var result = userAdmCollection.UpdateOne(userAdm => true, update);
        }
        public void insertHistoricoDocument(JunoCharge charge, IMongoDatabase mongoDatabase)
        {
            IMongoCollection<JunoHistorico> historicoCollection = mongoDatabase.GetCollection<JunoHistorico>("historicoPagamento");
            DateTimeOffset dueDateCharge = new DateTimeOffset(new DateTime(int.Parse(charge.dueDate.Split("-")[0]), int.Parse(charge.dueDate.Split("-")[1]), int.Parse(charge.dueDate.Split("-")[2])));
            List<JunoHistorico> historicoDocument = historicoCollection.Find<JunoHistorico>(JunoHistorico => true).ToList();

            if(historicoDocument.Count > 0)
            {
                if(dueDateCharge.ToUnixTimeSeconds() > historicoDocument[0].dueData)
                {
                    if(charge.status == "PAID")
                    {
                        JunoHistorico doc =  new JunoHistorico() {
                            idCharge = charge.subscription.id,
                            amount = charge.amount,
                            dueData = dueDateCharge.ToUnixTimeSeconds(),
                            status = charge.status,
                        };
                        historicoCollection.InsertOne(doc);

                        changeIsPayment(mongoDatabase, true);
                    } else {
                        changeIsPayment(mongoDatabase, false);
                    }
                }
            } else {
                if(charge.status == "PAID")
                {
                    JunoHistorico doc =  new JunoHistorico() {
                    idCharge = charge.subscription.id,
                    amount = charge.amount,
                    dueData = dueDateCharge.ToUnixTimeSeconds(),
                    status = charge.status,
                    };
                    historicoCollection.InsertOne(doc);
                    changeIsPayment(mongoDatabase, true);
                } else {
                    changeIsPayment(mongoDatabase, false);
                }
            }
        }

        public void consultCharges(Object state)
        {
            dynamic _getAccessToken = _paymentService.getAccessToken();

            var client = new RestClient($"{_baseUrl}/api-integration/charges");
            var request = new RestRequest(Method.GET);
            request.AddHeaders(new Dictionary<string, string>() {
                {"Authorization", $"Bearer {_getAccessToken.access_token}"},
                {"X-Api-Version", "2"},
                {"X-Resource-Token", "85D4CF242645507CEE7332F4451BCBF398027EEA65B36135EA64B99036DD90D4"},
                {"Content-Type", "application/json;charset=UTF-8"}
            });

            IRestResponse response = client.Execute(request);

            JunoEmbedded _junoEmbedded = JsonConvert.DeserializeObject<JunoEmbedded>(response.Content);
            JunoCharges _junoCharges = _junoEmbedded._embedded;

            try
            {
                IAsyncCursor<string> _listDatabaseNames = _clientMongoDb.ListDatabaseNames();
                foreach (var databaseName in _listDatabaseNames.ToList())
                {
                    if(databaseName != "admin" && databaseName != "config" && databaseName != "local" && databaseName != "userscondominio")
                    {
                        IMongoDatabase mongoDatabase = _clientMongoDb.GetDatabase(databaseName);
                        IMongoCollection<UserAdm> userAdmCollection = mongoDatabase.GetCollection<UserAdm>("usersAdm");
                        UserAdm userAdm = userAdmCollection.Find<UserAdm>(user => true).ToList()[0];

                        foreach(JunoCharge charge in _junoCharges.charges)
                        {
                            if(charge.subscription != null)
                            {
                                if(userAdm.idSubscription == charge.subscription.id)
                                {
                                    insertHistoricoDocument(charge, mongoDatabase);
                                }
                            }
                        }

                    }
                }
            }
            catch (System.TimeoutException)
            {
                Console.Write("Empty Database\n");
            }

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }
}