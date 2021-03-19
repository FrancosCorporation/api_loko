using System.Collections.Generic;
using System.Text.Json;

namespace first_api.Models
{
    public class JunoSubscription
    {
        public string id {get; set;}
        public string createdOn {get; set;}
        public string dueDay {get; set;}
        public string status {get; set;}
        public string startsOn {get; set;}
        public string nextBillingDate {get; set;}
        public Dictionary<dynamic, dynamic> _links {get; set;}
    }

    public class links
    {
        public string href {get; set;}
    }
}