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
        private readonly UserService userservice = new UserService();
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
        public dynamic LoginCondominio(UserGeneric user, HttpRequest request)
        {
            try
            {
                //puxar primeiro qual nome do condominio equivale o email e se existe
                IMongoDatabase _newDatabase = _clientMongoDb.GetDatabase("userscondominio");
                IMongoCollection<UserReferencia> _users2 = _newDatabase.GetCollection<UserReferencia>("users");
                UserReferencia userref = _users2.Find(userref => userref.email == user.email).ToList()[0];

                //password para hash
                string _passwordSHA256 = userservice.passwordToHash(user.password);
                _newDatabase = _clientMongoDb.GetDatabase(userref.databaseName);


                if (userref.role == "Administrator")
                {
                    IMongoCollection<UserAdm> _users = _newDatabase.GetCollection<UserAdm>("usersAdm");
                    UserAdm _user = _users.Find(_user => _user.email == user.email & _user.password == _passwordSHA256).ToList()[0];
                    string _tokenUser = userservice.GenerateToken(_user);
                    return Ok(new { token = _tokenUser });
                }
                else if (userref.role == "Porteiro")
                {
                    IMongoCollection<UserPorteiro> _users = _newDatabase.GetCollection<UserPorteiro>("usersPorteiros");
                    UserPorteiro _user = _users.Find(_user => _user.email == user.email & _user.password == _passwordSHA256).ToList()[0];
                    string _tokenUser = userservice.GenerateToken(_user);
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
                string _passwordSHA256 = userservice.passwordToHash(user.password);
                IMongoDatabase _newDatabase = _clientMongoDb.GetDatabase(userservice.RemoverCaracterEspecial(user.nameCondominio));

                IMongoCollection<UserMorador> _users = _newDatabase.GetCollection<UserMorador>("usersMoradores");
                UserMorador _user = _users.Find(_user => _user.email == user.email & _user.password == _passwordSHA256).ToList()[0];
                string _tokenUser = userservice.GenerateToken(_user);
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
                if (!userservice.EmailExist(user, _clientMongoDb))
                {

                    if (verificardatabaseexist(user))
                    {
                        return Conflict("Esse nome de condominio já esta cadastrado, tente outro nome");
                    }
                    else
                    {
                        //criando ou pegando o banco de dados com o nome do condiminio
                        IMongoDatabase _newDatabase = _clientMongoDb.GetDatabase(userservice.RemoverCaracterEspecial(user.nameCondominio));
                        //usando o banco para criar as collections
                        _newDatabase.CreateCollection("usersAdm");
                        _newDatabase.CreateCollection("usersPorteiros");
                        _newDatabase.CreateCollection("usersMoradores");
                        _newDatabase.CreateCollection("config_app");
                        _newDatabase.CreateCollection("academia");
                        _newDatabase.CreateCollection("avisos");

                        _newDatabase.GetCollection<BsonDocument>("usersAdm").InsertOne(userservice.RetornaUserAdm(user));

                        _newDatabase = _clientMongoDb.GetDatabase("userscondominio");
                        if (_newDatabase.GetCollection<BsonDocument>("users") == null)
                        {
                            _newDatabase.CreateCollection("users");
                            _newDatabase.GetCollection<BsonDocument>("users").InsertOne(userservice.RetornaUserRef(user));
                        }
                        else
                        {
                            _newDatabase.GetCollection<BsonDocument>("users").InsertOne(userservice.RetornaUserRef(user));
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
            if (userservice.ValidateToken(request) & userservice.UnGenereteToken(request)["role"].ToString() == "Administrator")
            {
                JObject jsonClaim = userservice.UnGenereteToken(request);
                user.nameCondominio = jsonClaim["nameCondominio"].ToString();
                if (!userservice.EmailExist(user, _clientMongoDb))
                {
                    _clientMongoDb.GetDatabase(userservice.RemoverCaracterEspecial(user.nameCondominio)).GetCollection<BsonDocument>("usersPorteiros").InsertOne(userservice.RetornaUserPorteiro(user));
                    _clientMongoDb.GetDatabase("userscondominio").GetCollection<BsonDocument>("users").InsertOne(userservice.RetornaUserRef(user));
                    return Ok(user.nome + " Cadastrado com sucesso.");

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
        public dynamic RegisterMorador(UserMorador user, HttpRequest request)
        {
            if (userservice.ValidateToken(request) & userservice.UnGenereteToken(request)["role"].ToString() == "Administrator")
            {
                JObject jsonClaim = userservice.UnGenereteToken(request);
                user.nameCondominio = jsonClaim["nameCondominio"].ToString();
                if (!userservice.EmailExist(user, _clientMongoDb))
                {

                    _clientMongoDb.GetDatabase(userservice.RemoverCaracterEspecial(user.nameCondominio)).GetCollection<BsonDocument>("usersMoradores").InsertOne(userservice.RetornaUserMorador(user));
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
        public dynamic GetInfoUser(HttpRequest request)
        {


            if (userservice.ValidateToken(request))
            {
                JObject jsonClaim = userservice.UnGenereteToken(request);
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
        public Boolean verificardatabaseexist(UserAdm user)
        {
            try
            {
                if (_clientMongoDb.GetDatabase(userservice.RemoverCaracterEspecial(user.nameCondominio)).ListCollections().ToList().Count > 0)
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