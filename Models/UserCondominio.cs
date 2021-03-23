using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace condominioApp.Models
{
    public class UserAdm : UserGeneric
    {

        [Required]
        public string estado { get; set; }
        [Required]
        public string cidade { get; set; }
        [Required]
        public string endereco { get; set; }


    }
    public class UserReferencia
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string nameCondominio { get; set; }
        public string databaseName { get; set; }
        public string role { get; set; }
    }
    public class UserMorador : UserGeneric
    {
        [Required]
        public string nome { get; set; }
        [Required]
        public string bloco { get; set; }
        [Required]
        public string numeroapartamento { get; set; }

    }
    public class UserPorteiro : UserGeneric
    {
        [Required]
        public string nome { get; set; }

    }
    public class UserGeneric
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
        public string role { get; set; }
        public string nameCondominio { get; set; }
        public BsonInt64 datacreate { get; set; }
    }
    public class UserGenericLogin : UserGeneric
    {
        [Required]
        new public string nameCondominio { get; set; }
    }
}