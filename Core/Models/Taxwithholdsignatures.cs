using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    [Table("TAXWITHHOLDSIGNATURES")]
    public partial class Taxwithholdsignatures
    {
        [Column("TAXWITHHOLDSIGNATURE")]
        public byte[] Taxwithholdsignature { get; set; }
        [Required]
        [Column("DATAAREAID")]
        [StringLength(4)]
        public string Dataareaid { get; set; }
        [Column("RECVERSION")]
        public int Recversion { get; set; }
        [Column("PARTITION")]
        public long Partition { get; set; }
        [Key]
        [Column("RECID")]
        public long Recid { get; set; }
    }
}
