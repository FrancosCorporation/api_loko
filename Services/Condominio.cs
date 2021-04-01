using condominioApi.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using MongoDB.Bson;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace condominioApi.Services
{

    public class CondominioService : ControllerBase
    {
        private readonly UserService userService = new UserService();
        private readonly ObjectsService objectsService = new ObjectsService();
        private readonly IMongoDatabase _condominiosDatabase;
        private readonly MongoClient _clientMongoDb;

        public CondominioService(ICondominioDatabaseSetting setting)
        {
            var client = new MongoClient(setting.ConnectionString);
            _clientMongoDb = client;
            _condominiosDatabase = client.GetDatabase(setting.DatabaseName);
        }

        public List<string> GetListNameDatabase()
        {
            List<string> listNameDatabase = new List<string>();
            using (var cursor = _clientMongoDb.ListDatabaseNames())
            {
                while (cursor.MoveNext())
                {
                    foreach (var current in cursor.Current)
                    {
                        if (current != "admin" && current != "config" && current != "local" && current != "userscondominio") listNameDatabase.Add(current);
                    }
                }
            }

            return listNameDatabase;
        }
        public List<string> ListaAgendamentos(HttpRequest request)
        {
            JObject jsonClaim = userService.UnGenereteToken(request);
            string nameCondominio = jsonClaim["nameCondominio"].ToString();
            IMongoDatabase _newDatabase = _clientMongoDb.GetDatabase(userService.RemoverCaracterEspecial(nameCondominio));
            {

                List<string> listNameCollection = new List<string>();

                using (var cursor = _newDatabase.ListCollectionNames())
                {
                    while (cursor.MoveNext())
                    {
                        foreach (var current in cursor.Current)
                        {
                            if (current != "avisos" && current != "configApp" && current != "usersAdm" && current != "usersPorteiros" && current != "usersMoradores") listNameCollection.Add(current);
                        }
                    }
                }

                return listNameCollection;
            }

        }
        public dynamic GetInfoUser(HttpRequest request)
        {


            if (userService.ValidateToken(request))
            {
                JObject jsonClaim = userService.UnGenereteToken(request);
                IMongoDatabase _newDatabase = _clientMongoDb.GetDatabase(jsonClaim["database"].ToString());
                IMongoCollection<UserAdm> _users = _newDatabase.GetCollection<UserAdm>("users");
                UserAdm _user = _users.Find(_user => _user.id == jsonClaim["objectId"].ToString()).ToList()[0];
                _user.password = "Não veja";
                return Ok(_user);
            }
            else
            {
                return Unauthorized("Token Inválido");
            }
        }
        public dynamic LoginCondominio(UserGeneric user, HttpRequest request)
        {
            try
            {
                //puxar primeiro qual nome do condominio equivale o email e se existe
                IMongoDatabase _newDatabase = _clientMongoDb.GetDatabase("userscondominio");
                IMongoCollection<UserReferencia> _users2 = _newDatabase.GetCollection<UserReferencia>("users");
                UserReferencia userref = _users2.Find(userref => userref.email == user.email).ToList()[0];

                //password para hash
                string _passwordSHA256 = userService.passwordToHash(user.password);
                _newDatabase = _clientMongoDb.GetDatabase(userref.databaseName);


                if (userref.role == "Administrator")
                {
                    IMongoCollection<UserAdm> _users = _newDatabase.GetCollection<UserAdm>("usersAdm");
                    UserAdm _user = _users.Find(_user => _user.email == user.email & _user.password == _passwordSHA256).ToList()[0];
                    string _tokenUser = userService.GenerateToken(_user);
                    return Ok(new { token = _tokenUser });
                }
                else if (userref.role == "Porteiro")
                {
                    IMongoCollection<UserPorteiro> _users = _newDatabase.GetCollection<UserPorteiro>("usersPorteiros");
                    UserPorteiro _user = _users.Find(_user => _user.email == user.email & _user.password == _passwordSHA256).ToList()[0];
                    string _tokenUser = userService.GenerateToken(_user);
                    return Ok(new { token = _tokenUser });
                }
                return StatusCode(400);

            }
            catch (SystemException)
            {
                return Unauthorized();
            }

        }
        public dynamic LoginMorador(UserGenericLogin user, HttpRequest request)
        {
            try
            {
                //password para hash
                string _passwordSHA256 = userService.passwordToHash(user.password);
                IMongoDatabase _newDatabase = _clientMongoDb.GetDatabase(userService.RemoverCaracterEspecial(user.nameCondominio));

                IMongoCollection<UserMorador> _users = _newDatabase.GetCollection<UserMorador>("usersMoradores");
                UserMorador _user = _users.Find(_user => _user.email == user.email & _user.password == _passwordSHA256).ToList()[0];
                string _tokenUser = userService.GenerateToken(_user);
                return Ok(new { token = _tokenUser });


            }
            catch (SystemException)
            {
                return Unauthorized();
            }

        }
        public dynamic RegisterCondominio(UserAdm user, HttpRequest request)
        {

            try
            {
                if (!userService.EmailExist(user, _clientMongoDb))
                {

                    if (verificardatabaseexist(user))
                    {
                        return Conflict("Esse nome de condominio já esta cadastrado, tente outro nome");
                    }
                    else
                    {
                        //criando ou pegando o banco de dados com o nome do condiminio
                        IMongoDatabase _newDatabase = _clientMongoDb.GetDatabase(userService.RemoverCaracterEspecial(user.nameCondominio));
                        //usando o banco para criar as collections
                        _newDatabase.CreateCollection("usersAdm");
                        _newDatabase.CreateCollection("usersPorteiros");
                        _newDatabase.CreateCollection("usersMoradores");
                        _newDatabase.CreateCollection("configApp");
                        _newDatabase.CreateCollection("avisos");

                        _newDatabase.GetCollection<BsonDocument>("usersAdm").InsertOne(userService.RetornaUserAdm(user));

                        _newDatabase = _clientMongoDb.GetDatabase("userscondominio");
                        if (_newDatabase.GetCollection<BsonDocument>("users") == null)
                        {
                            _newDatabase.CreateCollection("users");
                            _newDatabase.GetCollection<BsonDocument>("users").InsertOne(userService.RetornaUserRef(user));
                        }
                        else
                        {
                            _newDatabase.GetCollection<BsonDocument>("users").InsertOne(userService.RetornaUserRef(user));
                        }

                        return Ok("Condominio " + user.nameCondominio + " cadastrado com sucesso!");

                    }

                }
                else
                {
                    return Conflict("Email já existe");

                }

            }
            catch (System.Exception)
            {

                return "Não foi possivel realizar o cadastro, tente novamente mais tarde.";

            }

        }
        public dynamic RegisterPorteiro(UserPorteiro user, HttpRequest request)
        {

            JObject jsonClaim = userService.UnGenereteToken(request);
            user.nameCondominio = jsonClaim["nameCondominio"].ToString();
            if (!userService.EmailExist(user, _clientMongoDb))
            {
                _clientMongoDb.GetDatabase(userService.RemoverCaracterEspecial(user.nameCondominio)).GetCollection<BsonDocument>("usersPorteiros").InsertOne(userService.RetornaUserPorteiro(user));
                _clientMongoDb.GetDatabase("userscondominio").GetCollection<BsonDocument>("users").InsertOne(userService.RetornaUserRef(user));
                return Ok(user.nome + " Cadastrado com sucesso.");

            }
            else
            {
                return Conflict("Email Já Cadastrado");
            }

        }
        public dynamic RegisterMorador(UserMorador user, HttpRequest request)
        {
            if (userService.ValidateToken(request) & userService.UnGenereteToken(request)["role"].ToString() == "Administrator")
            {
                JObject jsonClaim = userService.UnGenereteToken(request);
                user.nameCondominio = jsonClaim["nameCondominio"].ToString();
                if (!userService.EmailExist(user, _clientMongoDb))
                {

                    _clientMongoDb.GetDatabase(userService.RemoverCaracterEspecial(user.nameCondominio)).GetCollection<BsonDocument>("usersMoradores").InsertOne(userService.RetornaUserMorador(user));
                    return Ok("Morador " + user.nome + " Cadastrado com sucesso.");

                }
                else
                {
                    return Conflict("Email Já Cadastrado");
                }
            }
            else
            {
                return Unauthorized("Token Invalido");
            }
        }
        public dynamic CadastroAvisos(Aviso texto, HttpRequest request)
        {
            try
            {

                JObject jsonClaim = userService.UnGenereteToken(request);
                string nameCondominio = jsonClaim["nameCondominio"].ToString();
                _clientMongoDb.GetDatabase(userService.RemoverCaracterEspecial(nameCondominio)).GetCollection<BsonDocument>("avisos").InsertOne(objectsService.RetornaAviso(texto));


                return Ok("Aviso" + texto.titulo + " cadastrado com Sucesso");
            }
            catch (System.Exception)
            {
                return Conflict();
            }

        }
        public dynamic CriarAgendamento(Agendamento agend, HttpRequest request)
        {
            try
            {
                JObject jsonClaim = userService.UnGenereteToken(request);
                string nameCondominio = jsonClaim["nameCondominio"].ToString();
                IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
                db.GetCollection<BsonDocument>("configApp").InsertOne(objectsService.RetornaAgendamento(agend));
                db.CreateCollection(userService.RemoverCaracterEspecialDeixarEspaco(agend.itemNome));
                return Ok(agend.itemNome + " para agendamentos cadastrado com Sucesso");
            }
            catch (System.Exception e)
            {
                Console.Write(e);
                return Conflict();
            }

        }
        public dynamic CadastrarAgendamento(CriacaoAgendamento agend, String name, HttpRequest request)
        {
            try
            {
                JObject jsonClaim = userService.UnGenereteToken(request);
                string nameCondominio = jsonClaim["nameCondominio"].ToString();
                IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
                db.GetCollection<BsonDocument>(userService.RemoverCaracterEspecialDeixarEspaco(name)).InsertOne(objectsService.RetornaCriacaoAgendamento(agend, request));
                return Ok("Agendado em "+name+" para "+ agend.dateAgendamento +", Sucesso !");
            }
            catch (System.Exception e)
            {
                Console.Write(e);
                return Conflict();
            }

        }
        public dynamic EnviarFoto(HttpRequest request)
        {
            try
            {
                //byte[] image = request.Body.; 
                //String oi = request.Body.;
                //Console.Write(oi);
                //FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                return Ok("ok");


            }
            catch (System.Exception)
            {
                return Conflict("erro");
            }

        }
        public Boolean verificardatabaseexist(UserAdm user)
        {
            try
            {
                if (_clientMongoDb.GetDatabase(userService.RemoverCaracterEspecial(user.nameCondominio)).ListCollections().ToList().Count > 0)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}