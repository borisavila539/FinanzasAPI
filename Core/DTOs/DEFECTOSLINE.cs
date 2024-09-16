using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Core.DTOs
{
    [XmlRoot("INTEGRATION")]
    public class DEFECTOSHEADER
    {
        [XmlElement("DEFECTOSLINE", typeof(DEFECTOSLINE))]
        public DEFECTOSLINE[] LINES { get; set; }
    }
    public class DEFECTOSLINE
    {
        [XmlElement]
        public string ROLLID { get; set; }
        [XmlElement]
        public string APVENDROLL { get; set; }
        [XmlElement]
        public string DEFECTOID { get; set; }
        [XmlElement]
        public string LEVEL1 { get; set; }
        [XmlElement]
        public string LEVEL2 { get; set; }
        [XmlElement]
        public string LEVEL3 { get; set; }
        [XmlElement]
        public string LEVEL4 { get; set; }
        [XmlElement]
        public string OBSERVACION { get; set; }
        [XmlElement]
        public string YARDASREALES { get; set; }
    }
}
