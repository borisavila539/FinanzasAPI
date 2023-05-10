using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public partial class Companyinfoview
    {
        [Column("REGNUM")]
        [StringLength(25)]
        public string Regnum { get; set; }
        [Column("VATNUM")]
        [StringLength(20)]
        public string Vatnum { get; set; }
        [Column("DATAAREA")]
        [StringLength(4)]
        public string Dataarea { get; set; }
        [Column("COREGNUM")]
        [StringLength(25)]
        public string Coregnum { get; set; }
        [Column("IMPORTVATNUM")]
        [StringLength(20)]
        public string Importvatnum { get; set; }
        [Column("PARTY")]
        public long Party { get; set; }
        [Column("PARTITION")]
        public long Partition { get; set; }
        [Column("RECID")]
        public long Recid { get; set; }
        [Column("PARTITION#2")]
        public long? Partition2 { get; set; }
        [Column("BANKACCOUNTNUM")]
        [StringLength(35)]
        public string Bankaccountnum { get; set; }
        [Column("BANKIBAN")]
        [StringLength(35)]
        public string Bankiban { get; set; }
        [Column("BANKSWIFT")]
        [StringLength(20)]
        public string Bankswift { get; set; }
        [Column("BANKLOCATION")]
        public long? Banklocation { get; set; }
        [Column("PARTITION#3")]
        public long? Partition3 { get; set; }
        [Column("CONTACTEMAIL")]
        [StringLength(255)]
        public string Contactemail { get; set; }
        [Column("CONTACTPHONEEXTENSION")]
        [StringLength(10)]
        public string Contactphoneextension { get; set; }
        [Column("CONTACTFAX")]
        [StringLength(255)]
        public string Contactfax { get; set; }
        [Column("CONTACTPHONE")]
        [StringLength(255)]
        public string Contactphone { get; set; }
        [Column("CONTACTTELEX")]
        [StringLength(255)]
        public string Contacttelex { get; set; }
        [Column("CONTACTURL")]
        [StringLength(255)]
        public string Contacturl { get; set; }
        [Column("PARTITION#4")]
        public long Partition4 { get; set; }
        [Required]
        [Column("ACCOUNTINGCURRENCY")]
        [StringLength(3)]
        public string Accountingcurrency { get; set; }
        [Column("PARTITION#5")]
        public long Partition5 { get; set; }
        [Required]
        [Column("NAME")]
        [StringLength(100)]
        public string Name { get; set; }
        [Column("PARTITION#6")]
        public long? Partition6 { get; set; }
        [Column("COMPANYIDNAF")]
        [StringLength(5)]
        public string Companyidnaf { get; set; }
    }
}
