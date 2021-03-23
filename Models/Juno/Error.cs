using System.Collections.Generic;

namespace condominioApi.Models
{
    public class JunoError
    {
        public string timestamp {get; set;}
        public int status {get; set;}
        public string error  {get; set;}
        public List<DetailsError> details {get; set;}
        public string path {get; set;}
    }

    public class DetailsError
    {
        public string field {get; set;}
        public string message {get; set;}
        public string errorCode {get; set;}
    }
}