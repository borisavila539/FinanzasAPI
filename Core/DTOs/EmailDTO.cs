using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class EmailDTO
    {
        public string To { get; set; }
        public string Asunto { get; set; }
        public string Html { get; set; }
        public List<string> Attachments {get;set;}
        public List<byte[]> fileBytes { get; set; }
    }
}
