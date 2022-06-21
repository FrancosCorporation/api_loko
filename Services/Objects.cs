using System;
using condominio_api.Models;
using MongoDB.Bson;
using Microsoft.AspNetCore.Http;
using condominio_api.DependencyService;
/*
    Gera token utilizando a chave Secret no arquivo Settings
*/
namespace condominio_api.Services
{
    public class ObjectsService
    {
        private readonly IUserService _userSevice;

        public ObjectsService() {}
        public ObjectsService(IUserService userService)
        {
            _userSevice = userService;
        }
        public BsonDocument RetornaAviso(Aviso texto)
        {

            return new BsonDocument{
                    {"_id", ObjectId.GenerateNewId()},
                    {"titulo", texto.titulo.ToLower()},
                    {"mensagem", texto.mensagem.ToLower()},
                    {"datacreate", DateTimeOffset.Now.ToUnixTimeSeconds()}
                };
        }
        public BsonDocument RetornaAgendamento(Agendamento agend)
        {
            if(agend.descricao == null){
                agend.descricao = "";
            }
            return new BsonDocument{
                    {"_id", ObjectId.GenerateNewId()},
                    {"itemNome", agend.itemNome.ToLower()},
                    {"ativo", true},
                    {"horaInicio", agend.horaInicio.ToLower()},
                    {"horaFim", agend.horaFim.ToLower()},
                    {"tempoUtilizacao", agend.tempoUtilizacao.ToLower()},
                    {"qntPessoas", agend.qntPessoas},
                    {"diasSemana", agend.diasSemana.ToLower()},
                    {"descricao", agend.descricao.ToLower()},
                    {"datacreate", DateTimeOffset.Now.ToUnixTimeSeconds()}
                };
        }
        public BsonDocument RetornaCriacaoAgendamento(CriacaoAgendamento agend, HttpRequest request)
        {
            return new BsonDocument{
                    {"_id", ObjectId.GenerateNewId()},
                    {"idUser", _userSevice.UnGenereteToken(request)["objectId"].ToString()},
                    {"dateAgendamento", agend.dateAgendamento},
                    {"datacreate", DateTimeOffset.Now.ToUnixTimeSeconds()}
                };
        }
       
    }
}