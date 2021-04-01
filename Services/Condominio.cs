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
        private double _timeExpiredTokenLogin = 10;

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
        public dynamic ListaAgendamentos(HttpRequest request)
        {
            JObject jsonClaim = userService.UnGenereteToken(request);
            string nameCondominio = jsonClaim["nameCondominio"].ToString();
            if (userService.ValidateToken(request))
            {
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

            else
            {
                return Unauthorized();
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
                    string _tokenUser = userService.GenerateToken(_user, _timeExpiredTokenLogin);
                    return Ok(new { token = _tokenUser });
                }
                else if (userref.role == "Porteiro")
                {
                    IMongoCollection<UserPorteiro> _users = _newDatabase.GetCollection<UserPorteiro>("usersPorteiros");
                    UserPorteiro _user = _users.Find(_user => _user.email == user.email & _user.password == _passwordSHA256).ToList()[0];
                    string _tokenUser = userService.GenerateToken(_user, _timeExpiredTokenLogin);
                    return Ok(new { token = _tokenUser });
                }
                return BadRequest();

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
                string _tokenUser = userService.GenerateToken(_user, _timeExpiredTokenLogin);
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

                    if (VerificarDatabaseExist(user.nameCondominio))
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

                        BsonDocument novoAdm = userService.RetornaUserAdm(user);
                        _newDatabase.GetCollection<BsonDocument>("usersAdm").InsertOne(novoAdm);

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
                        user.role = "Administrator";
                        user.id = novoAdm["_id"].ToString();
                        userService.EmailConfimacao(user);
                        return Ok("Condominio " + user.nameCondominio + " cadastrado com sucesso!");

                    }

                }
                else
                {
                    return Conflict("Email já existe");

                }

            }
            catch (System.Exception e)
            {
                Console.Write(e);
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
        public dynamic CadastroComunicado(Aviso texto, HttpRequest request)
        {
            try
            {

                JObject jsonClaim = userService.UnGenereteToken(request);
                string nameCondominio = jsonClaim["nameCondominio"].ToString();
                _clientMongoDb.GetDatabase(userService.RemoverCaracterEspecial(nameCondominio)).GetCollection<BsonDocument>("avisos").InsertOne(objectsService.RetornaAviso(texto));


                return Ok("Aviso " + texto.titulo + " cadastrado com Sucesso");
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
                string nameCollection = "configApp";
                if (!verificarAgendamentosExist(nameCondominio, agend.itemNome, nameCollection))
                {
                    IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
                    db.GetCollection<BsonDocument>(nameCollection).InsertOne(objectsService.RetornaAgendamento(agend));
                    db.CreateCollection(userService.RemoverCaracterEspecialDeixarEspaco(agend.itemNome));
                    return Ok(agend.itemNome + " para agendamentos cadastrado com Sucesso");
                }
                else
                {
                    return Conflict(agend.itemNome + " Já está cadastrado !");
                }

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
                string nameCondominio = userService.UnGenereteToken(request)["nameCondominio"].ToString();
                IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
                db.GetCollection<BsonDocument>(userService.RemoverCaracterEspecialDeixarEspaco(name)).InsertOne(objectsService.RetornaCriacaoAgendamento(agend, request));
                return Ok("Agendado em " + name + " para " + agend.dateAgendamento + ", Sucesso !");
            }
            catch (System.Exception e)
            {
                Console.Write(e);
                return Conflict();
            }

        }
        public dynamic EditarAgendamento(Agendamento agend, HttpRequest request)
        {
            try
            {
                string nameCondominio = userService.UnGenereteToken(request)["nameCondominio"].ToString();
                string nameCollection = "configApp";
                if (verificarAgendamentosExist(nameCondominio, agend.itemNome, nameCollection))
                {
                    IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
                    BsonDocument old = GetBson(nameCondominio, agend.itemNome, nameCollection);
                    BsonDocument novo = objectsService.RetornaAgendamento(agend);
                    novo["_id"] = old["_id"];
                    db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                    return Ok("Agendado em " + agend.itemNome + " alterado com Sucesso !");
                }
                else
                {
                    return Conflict(agend.itemNome + " não exite.");
                }

            }
            catch (System.Exception e)
            {
                Console.Write(e);
                return Conflict();
            }

        }
        public dynamic EditarSenha(String token, string senha, HttpRequest request)
        {
            request.Headers["Authorization"] = "1 " + token;
            //password para hash
            string _passwordSHA256 = userService.passwordToHash(senha);
            string nameCondominio = userService.UnGenereteToken(request)["nameCondominio"].ToString();
            string role = userService.UnGenereteToken(request)["role"].ToString();
            string id = userService.UnGenereteToken(request)["objectId"].ToString();
            //pego o database  
            IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
            if (userService.ValidateToken(request))
            {
                if (role == "Administrator")
                {
                    string nameCollection = "usersAdm";
                    BsonDocument old = GetBson(nameCondominio, id, nameCollection);
                    BsonDocument novo = GetBson(nameCondominio, id, nameCollection);
                    novo["password"] = _passwordSHA256;
                    //pego a colection                         faço a alteração
                    db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                    return Ok("Senha Alterada");

                }
                else if (role == "Porteiro")
                {
                    string nameCollection = "usersPorteiros";
                    BsonDocument old = GetBson(nameCondominio, id, nameCollection);
                    BsonDocument novo = GetBson(nameCondominio, id, nameCollection);
                    novo["password"] = _passwordSHA256;
                    //pego a colection                         faço a alteração
                    db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                    return Ok("Senha Alterada");
                }
                else if (role == "Morador")
                {
                    string nameCollection = "usersMoradores";
                    BsonDocument old = GetBson(nameCondominio, id, nameCollection);
                    BsonDocument novo = GetBson(nameCondominio, id, nameCollection);
                    novo["password"] = _passwordSHA256;
                    //pego a colection                         faço a alteração
                    db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                    return Ok("Senha Alterada");
                }
                else
                {
                    return Unauthorized();
                }

            }
            else
            {
                return Unauthorized();
            }
        }
        public dynamic AlterarSenha(string senha, HttpRequest request)
        {
            //password para hash
            string _passwordSHA256 = userService.passwordToHash(senha);
            string nameCondominio = userService.UnGenereteToken(request)["nameCondominio"].ToString();
            string role = userService.UnGenereteToken(request)["role"].ToString();
            string id = userService.UnGenereteToken(request)["objectId"].ToString();
            //pego o database  
            IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
            if (userService.ValidateToken(request))
            {

                if (role == "Administrator")
                {
                    string nameCollection = "usersAdm";
                    BsonDocument old = GetBson(nameCondominio, id, nameCollection);
                    BsonDocument novo = GetBson(nameCondominio, id, nameCollection);
                    novo["password"] = _passwordSHA256;
                    //pego a colection                         faço a alteração
                    db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                    return Ok("Senha Alterada");

                }
                else if (role == "Porteiro")
                {
                    string nameCollection = "usersPorteiros";
                    BsonDocument old = GetBson(nameCondominio, id, nameCollection);
                    BsonDocument novo = GetBson(nameCondominio, id, nameCollection);
                    novo["password"] = _passwordSHA256;
                    //pego a colection                         faço a alteração
                    db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                    return Ok("Senha Alterada");
                }
                else if (role == "Morador")
                {
                    string nameCollection = "usersMoradores";
                    BsonDocument old = GetBson(nameCondominio, id, nameCollection);
                    BsonDocument novo = GetBson(nameCondominio, id, nameCollection);
                    novo["password"] = _passwordSHA256;
                    //pego a colection                         faço a alteração
                    db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                    return Ok("Senha Alterada");
                }
                else
                {
                    return Unauthorized();
                }

            }
            else
            {
                return Unauthorized();
            }
        }
        public dynamic ConfirmacaoEmail(String token, HttpRequest request)
        {
            request.Headers["Authorization"] = "1 " + userService.RemoverBarraToken(token);

            if (userService.ValidateToken(request))
            {
                string nameCondominio = userService.UnGenereteToken(request)["nameCondominio"].ToString();
                string role = userService.UnGenereteToken(request)["role"].ToString();
                string id = userService.UnGenereteToken(request)["objectId"].ToString();
                //pego o database  
                IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
                if (role == "Administrator")
                {
                    string nameCollection = "usersAdm";
                    BsonDocument old = GetBson(nameCondominio, id, nameCollection);
                    BsonDocument novo = GetBson(nameCondominio, id, nameCollection);
                    novo["verificado"] = true;
                    //pego a colection                         faço a alteração
                    db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                    return Ok("Email Confirmado");

                }
                /*else if (role == "Porteiro")
                {
                    string nameCollection = "usersPorteiros";
                    BsonDocument old = GetBson(nameCondominio, id, nameCollection);
                    BsonDocument novo = GetBson(nameCondominio, id, nameCollection);
                    novo["password"] = _passwordSHA256;
                    //pego a colection                         faço a alteração
                    db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                    return Ok("Senha Alterada");
                }
                else if (role == "Morador")
                {
                    string nameCollection = "usersMoradores";
                    BsonDocument old = GetBson(nameCondominio, id, nameCollection);
                    BsonDocument novo = GetBson(nameCondominio, id, nameCollection);
                    novo["password"] = _passwordSHA256;
                    //pego a colection                         faço a alteração
                    db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                    return Ok("Senha Alterada");
                } */
                else
                {
                    return Unauthorized();
                }

            }
            else
            {
                return Unauthorized();
            }
        }
        public dynamic RedefinirSenha(RedefinirSenha red, HttpRequest request)
        {
            if (VerificarDatabaseExist(red.nameCondominio))
            {
                IMongoDatabase db = _clientMongoDb.GetDatabase(red.nameCondominio);
                if (red.role == "Administrator")
                {
                    try
                    {
                        string nameCollection = "usersAdm";
                        IMongoCollection<UserAdm> _users = db.GetCollection<UserAdm>(nameCollection);
                        UserAdm _user = _users.Find(_user => _user.email == red.email).ToList()[0];
                        string _tokenUser = userService.GenerateToken(_user, _timeExpiredTokenLogin);
                        userService.EmailDeRedefinicaoDeSenha(_user);
                        return Ok("Verique seu email (Valido por 30 minutos)");
                    }
                    catch
                    {
                        return NotFound("Usuario não encontrado");
                    }
                }
                else if (red.role == "Porteiro")
                {
                    try
                    {
                        string nameCollection = "usersPorteiros";
                        IMongoCollection<UserAdm> _users = db.GetCollection<UserAdm>(nameCollection);
                        UserAdm _user = _users.Find(_user => _user.email == red.email).ToList()[0];
                        string _tokenUser = userService.GenerateToken(_user, _timeExpiredTokenLogin);
                        userService.EmailDeRedefinicaoDeSenha(_user);
                        return Ok("Verique seu email (Valido por 30 minutos)");
                    }
                    catch
                    {
                        return NotFound("Usuario não encontrado");
                    }
                }
                else if (red.role == "Morador")
                {
                    try
                    {
                        string nameCollection = "usersMoradores";
                        IMongoCollection<UserAdm> _users = db.GetCollection<UserAdm>(nameCollection);
                        UserAdm _user = _users.Find(_user => _user.email == red.email).ToList()[0];
                        string _tokenUser = userService.GenerateToken(_user, _timeExpiredTokenLogin);
                        userService.EmailDeRedefinicaoDeSenha(_user);
                        return Ok("Verique seu email (Valido por 30 minutos)");
                    }
                    catch
                    {
                        return NotFound("Usuario não encontrado");
                    }
                }
                else
                {
                    return NotFound("Usuario não encontrado");
                }
            }
            else
            {
                return NotFound("Condominio não encontrado");
            }

        }
        public dynamic ListaItensAgendamentos(ObjectBase obj, HttpRequest request)
        {
            try
            {
                if (userService.ValidateToken(request) & userService.UnGenereteToken(request)["role"].ToString() != "Morador")
                {

                    string nameCondominio = userService.RemoverCaracterEspecial(userService.UnGenereteToken(request)["nameCondominio"].ToString());
                    List<CriacaoAgendamento> list = new List<CriacaoAgendamento>();
                    IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
                    list = db.GetCollection<CriacaoAgendamento>(obj.itemNome).Find(_ => true).ToList();
                    return RetornaListaComUserName(list, nameCondominio);
                }
                else
                {
                    return Unauthorized();
                }

            }
            catch (System.Exception)
            {
                return Conflict();
            }

        }
        public dynamic EnviarFoto(byte[] foto, HttpRequest request)
        {
            try
            {

                if (userService.ValidateToken(request))
                {
                    string role = userService.UnGenereteToken(request)["role"].ToString();
                    string id = userService.UnGenereteToken(request)["_id"].ToString();
                    string nameCondominio = userService.RemoverCaracterEspecial(userService.UnGenereteToken(request)["nameCondominio"].ToString());

                    if (role == "Administrator")
                    {
                        string nameCollection = "usersAdm";
                        IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
                        BsonDocument old = GetBson(nameCondominio, id, nameCollection);
                        BsonDocument novo = GetBson(nameCondominio, id, nameCollection);
                        novo["image"] = foto;
                        db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                        return Ok("Foto alterada com Sucesso !");
                    }
                    else if (role == "Porteiro")
                    {

                        string nameCollection = "usersPorteiros";
                        IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
                        BsonDocument old = GetBson(nameCondominio, id, nameCollection);
                        BsonDocument novo = GetBson(nameCondominio, id, nameCollection);
                        novo["image"] = foto;
                        db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                        return Ok("Foto alterada com Sucesso !");
                    }
                    else if (role == "Morador")
                    {
                        string nameCollection = "usersMoradores";
                        IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
                        BsonDocument old = GetBson(nameCondominio, id, nameCollection);
                        BsonDocument novo = GetBson(nameCondominio, id, nameCollection);
                        novo["image"] = foto;
                        db.GetCollection<BsonDocument>(nameCollection).ReplaceOne(old, novo);
                        return Ok("Foto alterada com Sucesso !");
                    }
                }

                return Unauthorized();

            }
            catch (System.Exception)
            {
                return Conflict("erro");
            }
        }
        public Boolean VerificarDatabaseExist(String nameDatabase)
        {
            try
            {
                if (_clientMongoDb.GetDatabase(userService.RemoverCaracterEspecial(nameDatabase)).ListCollections().ToList().Count > 0)
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
        public Boolean verificarAgendamentosExist(String nameDatabase, String nameitem, String nameCollection)
        {
            try
            {
                if (GetBson(nameDatabase, nameitem, nameCollection) != null)
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
        public BsonDocument GetBson(String nameDatabase, String nameitem, String nameCollection)
        {
            try
            {
                IMongoDatabase _newDatabase = _clientMongoDb.GetDatabase(userService.RemoverCaracterEspecial(nameDatabase));

                if (nameCollection == "configApp")
                {
                    IMongoCollection<Agendamento> agend = _newDatabase.GetCollection<Agendamento>(nameCollection);
                    Agendamento agendam = agend.Find(agendam => agendam.itemNome == userService.RemoverCaracterEspecialDeixarEspaco(nameitem)).ToList()[0];

                    if (agendam != null)
                    {
                        return agendam.ToBsonDocument();
                    }
                }
                else if (nameCollection == "usersAdm")
                {
                    IMongoCollection<UserAdm> user = _newDatabase.GetCollection<UserAdm>(nameCollection);
                    UserAdm user1 = user.Find(user1 => user1.id == userService.RemoverCaracterEspecialDeixarEspaco(nameitem)).ToList()[0];

                    if (user1 != null)
                    {
                        return user1.ToBsonDocument();
                    }

                }
                else if (nameCollection == "usersPorteiros")
                {
                    IMongoCollection<UserPorteiro> user = _newDatabase.GetCollection<UserPorteiro>(nameCollection);
                    UserPorteiro user1 = user.Find(user1 => user1.id == userService.RemoverCaracterEspecialDeixarEspaco(nameitem)).ToList()[0];

                    if (user1 != null)
                    {
                        return user1.ToBsonDocument();
                    }

                }
                else if (nameCollection == "usersMoradores")
                {
                    IMongoCollection<UserMorador> user = _newDatabase.GetCollection<UserMorador>(nameCollection);
                    UserMorador user1 = user.Find(user1 => user1.id == userService.RemoverCaracterEspecialDeixarEspaco(nameitem)).ToList()[0];

                    if (user1 != null)
                    {
                        return user1.ToBsonDocument();
                    }

                }
                else if (nameCollection == "avisos")
                {
                    IMongoCollection<Aviso> user = _newDatabase.GetCollection<Aviso>(nameCollection);
                    Aviso aviso = user.Find(aviso => aviso.titulo == userService.RemoverCaracterEspecialDeixarEspaco(nameitem)).ToList()[0];

                    if (aviso != null)
                    {
                        return aviso.ToBsonDocument();
                    }

                }


                return null;
            }
            catch
            {
                return null;
            }
        }
        public dynamic GetAgendamento(HttpRequest request)
        {
            string role = userService.UnGenereteToken(request)["role"].ToString();
            if ((role == "Administrator" || role == "Porteiro" || role == "Morador") & userService.ValidateToken(request))
            {
                string nameCondominio = userService.UnGenereteToken(request)["nameCondominio"].ToString();
                List<Aviso> list = new List<Aviso>();
                IMongoDatabase db = _clientMongoDb.GetDatabase(nameCondominio);
                return db.GetCollection<Aviso>("avisos").Find(_ => true).ToList();
            }
            else
            {
                return Unauthorized();
            }
        }
        public dynamic GetConfigAgendamentos(ObjectBase obj, HttpRequest request)
        {

            string role = userService.UnGenereteToken(request)["role"].ToString();
            if ((role == "Administrator" || role == "Porteiro" || role == "Morador") & userService.ValidateToken(request))
            {
                string nameCondominio = userService.UnGenereteToken(request)["nameCondominio"].ToString();
                IMongoDatabase db = _clientMongoDb.GetDatabase(userService.RemoverCaracterEspecial(nameCondominio));
                IMongoCollection<Agendamento> config = db.GetCollection<Agendamento>("configApp");
                Agendamento agend = config.Find(agend => agend.itemNome == userService.RemoverCaracterEspecialDeixarEspaco(obj.itemNome)).ToList()[0];
                agend.id = "";
                return agend;
            }
            else
            {
                return Unauthorized();
            }
        }
        public List<CriacaoAgendamento> RetornaListaComUserName(List<CriacaoAgendamento> agend, String nameDatabase)
        {

            IMongoCollection<UserMorador> user = _clientMongoDb.GetDatabase(nameDatabase).GetCollection<UserMorador>("usersMoradores");
            foreach (var item in agend)
            {
                item.id = "";
                Console.Write(item.idUser);
                UserMorador user1 = user.Find(user1 => user1.id == item.idUser).ToList()[0];
                item.idUser = user1.nome;

            }


            return agend;


        }


    }
}