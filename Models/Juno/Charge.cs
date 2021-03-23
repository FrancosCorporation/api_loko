using System.Collections.Generic;

namespace condominioApp.Models
{
    public class JunoCharge
    {
        public string id {get; set;}
        public int code {get; set;}
        public string reference {get; set;}
        public string dueDate {get; set;}
        public string checkoutUrl {get; set;}
        public float amount {get; set;}
        public string status {get; set;}
        public List<Payment> payments {get; set;}
        public JunoSubscriptionCharge subscription {get; set;}
    }

    public class Payment
    {
        public string id {get; set;}
        public string chargeId {get; set;}
        public string date {get; set;}
        public string releaseDate {get; set;}
        public float amount {get; set;}
        public float fee {get; set;}
        public string type {get; set;}
        public string status {get; set;}
        public string transactionId {get; set;}
        public string failReason {get; set;}
    }

    public class JunoSubscriptionCharge : JunoSubscription
    {
        public string lastBillingDate {get; set;}
    }

}