using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.ConciliacionBancaria
{
    public class ConciliacionBancariaDTO
    {
        public string WeekNumber { get; set; }
        public string AccountId { get; set; }
        public string CurrencyCode { get; set; }
    }
}
