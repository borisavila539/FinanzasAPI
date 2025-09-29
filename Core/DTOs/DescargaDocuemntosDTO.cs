using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class DescargaDocuemntosDTO
    {
        public string INVOICEID { get; set; }
        public string PDF { get; set; }
        public string DESCRIPTION { get; set; }
        public DateTime INVOICEDATE { get; set; }
        public Int64 RECID { get; set; }
    }
}
