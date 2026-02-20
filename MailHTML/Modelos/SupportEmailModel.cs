using System;
using System.Collections.Generic;
using System.Text;

namespace MailHTML.Modelos
{
    public class SupportEmailModel { 
        public string? CustomerName { get; set; }
        public string? CustomerId { get; set; } 
        public string? AddressId { get; set; }
        public int TerritoryId { get; set; } 
        public string? RequesterName { get; set; } 
        public int RequesterEmployeeId { get; set; }
    }
}
