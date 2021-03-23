using System;
using first_api.Models;
using MongoDB.Bson;
using Microsoft.AspNetCore.Http;
/*
    Gera token utilizando a chave Secret no arquivo Settings
*/
namespace first_api.Services
{
    public class ObjectsService
    {
        private readonly UserService userService = new UserService();
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
                    {"idUser", userService.UnGenereteToken(request)["objectId"].ToString()},
                    {"dateAgendamento", agend.dateAgendamento},
                    {"datacreate", DateTimeOffset.Now.ToUnixTimeSeconds()}
                };
        }
        public String RetornaImage(){
            string bits_imgage = "";

            return bits_imgage;
        }
       
    }
}