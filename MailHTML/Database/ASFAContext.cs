using MailHTML.Dominio.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;



namespace MailHTML.Database
{
    public class ASFAContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public ASFAContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var conn = _configuration.GetConnectionString("Crm");

            options.UseSqlServer(conn, sql =>
            {
                sql.CommandTimeout(3000);
            });
        }


        public DbSet<DbPingResult> DbPing { get; set; }
        public DbSet<LayoutMailModel> LayoutsMailWeb { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LayoutMailModel>(entity =>
            {
                entity.HasNoKey(); 
                entity.ToTable("LayoutsmailWeb"); 

                entity.Property(e => e.Layoutsmail).HasColumnName("Layoutsmail");
                entity.Property(e => e.ID).HasColumnName("ID");
                entity.Property(e => e.Nombre).HasColumnName("Nombre");
                entity.Property(e => e.Titulo).HasColumnName("Titulo");
                entity.Property(e => e.Asunto).HasColumnName("Asunto");
                entity.Property(e => e.Texto).HasColumnName("Texto");
                entity.Property(e => e.Body).HasColumnName("Body");
                entity.Property(e => e.Piepagina).HasColumnName("Piepagina");
            });

            modelBuilder.Entity<DbPingResult>().HasNoKey();
        }
    }
}