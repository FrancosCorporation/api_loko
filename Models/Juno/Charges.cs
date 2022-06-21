using System.Collections.Generic;

namespace condominio_api.Models
{
    public class JunoEmbedded
    {        
        public JunoCharges _embedded {get; set;}
    }

    public class JunoCharges
    {
        public List<JunoCharge> charges {get; set;}
    }

}