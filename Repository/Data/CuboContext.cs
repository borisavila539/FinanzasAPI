using Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Data
{
    public partial class CuboContext : DbContext
    {
        public CuboContext()
        {

        }

        public CuboContext(DbContextOptions<AxContext> options):base(options)
        {

        }

        public virtual DbSet<MaestroOrdenes> MaestroOrdenes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#pragma warning disable CS1030 // Directiva #warning
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.

                optionsBuilder.UseSqlServer("Server=cubo-intermoda;Database=IMDesarrollos;User ID = imditm; Password = Int3r-M0d@.1nD3s@; Encrypt=False;");
#pragma warning restore CS1030 // Directiva #warning
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MaestroOrdenes>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("IM_MASTERORDENES");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
