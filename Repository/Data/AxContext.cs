using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Data
{
    public partial class AxContext : DbContext
    {
        public AxContext()
        {

        }

        public AxContext(DbContextOptions<AxContext> options)
            : base(options)
        {

        }

        public virtual DbSet<Companyinfoview> Companyinfoview { get; set; }
        public virtual DbSet<Companyimage> Companyimage { get; set; }
        public virtual DbSet<Taxwithholdsignatures> Taxwithholdsignatures { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                #pragma warning disable CS1030 // Directiva #warning
                #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                
                optionsBuilder.UseSqlServer("Server=GIM-PRO3-DBO;Database=MicrosoftDynamicsAX_PRO;User ID = dtsitm; Password = Intermoda2020; Encrypt=False;");
#pragma warning restore CS1030 // Directiva #warning
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Companyinfoview>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("COMPANYINFOVIEW");
            });

            modelBuilder.Entity<Companyimage>(entity =>
            {
                entity.HasKey(e => new { e.Partition, e.Dataareaid, e.Refcompanyid, e.Reftableid, e.Refrecid })
                    .HasName("I_1394REFIDX");

                entity.Property(e => e.Partition).HasDefaultValueSql("((5637144576.))");

                entity.Property(e => e.Dataareaid).HasDefaultValueSql("('dat')");

                entity.Property(e => e.Refcompanyid).HasDefaultValueSql("('')");

                entity.Property(e => e.Recversion).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<Taxwithholdsignatures>(entity =>
            {
                entity.HasKey(e => e.Recid)
                    .HasName("I_103899RECID");

                entity.Property(e => e.Recid).ValueGeneratedNever();

                entity.Property(e => e.Dataareaid).HasDefaultValueSql("('dat')");

                entity.Property(e => e.Partition).HasDefaultValueSql("((5637144576.))");

                entity.Property(e => e.Recversion).HasDefaultValueSql("((1))");
            });

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
