using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    [Table("COMPANYIMAGE")]
    public partial class Companyimage
    {
        [Key]
        [Column("REFTABLEID")]
        public int Reftableid { get; set; }
        [Key]
        [Column("REFRECID")]
        public long Refrecid { get; set; }
        [Column("IMAGE")]
        public byte[] Image { get; set; }
        [Column("HASIMAGE")]
        public int Hasimage { get; set; }
        [Key]
        [Column("REFCOMPANYID")]
        [StringLength(4)]
        public string Refcompanyid { get; set; }
        [Key]
        [Column("DATAAREAID")]
        [StringLength(4)]
        public string Dataareaid { get; set; }
        [Column("RECVERSION")]
        public int Recversion { get; set; }
        [Key]
        [Column("PARTITION")]
        public long Partition { get; set; }
        [Column("RECID")]
        public long Recid { get; set; }
    }
}
