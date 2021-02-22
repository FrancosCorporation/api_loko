using first_api.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace first_api.Services
{
    public class AlunoService
    {
        private readonly IMongoCollection<Aluno> _alunos;

        public AlunoService(IAlunoDatabaseSetting settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _alunos = database.GetCollection<Aluno>(settings.AlunoCollectionName);
        }

        public List<Aluno> Get() =>
            _alunos.Find(aluno => true).ToList();

        public Aluno Get(string id) =>
            _alunos.Find<Aluno>(aluno => aluno.Id == id).FirstOrDefault();

        public Aluno Create(Aluno aluno)
        {
            _alunos.InsertOne(aluno);
            return aluno;
        }

        public void Update(string id, Aluno alunoIn) =>
            _alunos.ReplaceOne(aluno => aluno.Id == id, alunoIn);

        public void Remove(Aluno alunoIn) =>
            _alunos.DeleteOne(aluno => aluno.Id == alunoIn.Id);

        public void Remove(string id) => 
            _alunos.DeleteOne(aluno => aluno.Id == id);
    }
}