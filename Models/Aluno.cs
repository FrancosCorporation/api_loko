using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace first_api.Models
{
    public class Aluno
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id {get; set;}

        /*
            O nome da variavel tem que estar igual ao nome da variavel no banco de dados, tanto o nome quanto o tamanho
        */

        [BsonElement("name")] //Ele vai pegar somente o "Name" para buscar na db
        public string Alunoname {get; set;} // O nome da varaivel é o nome que aparece na consulta
        [BsonElement("Idade")] //Ele vai pegar somente o "Idade" para buscar na db
        public int AlunoIdade {get; set;}
        public double Media {get; set;}
        public string Cidade {get; set;}

    }
}