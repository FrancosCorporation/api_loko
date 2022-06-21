using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace condominio_api.Models
{
    public class JunoHistorico
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string idCharge { get; set; }
        public float amount { get; set; }
        public BsonInt64 dueData { get; set; }
        public string status { get; set; }
    }
}