using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Histo_Product.Models
{
    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model1")
        {
        }

        public virtual DbSet<Board> Boards { get; set; }
        public virtual DbSet<Ligne> Lignes { get; set; }
        public virtual DbSet<Operateur> Operateurs { get; set; }
        public virtual DbSet<Test> Tests { get; set; }
        public virtual DbSet<Profil> Profils { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Machine> Machines { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Board configuration
            modelBuilder.Entity<Board>()
                .Property(e => e.Id)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Board>()
                .Property(e => e.Code_AsteelFlash)
                .IsUnicode(false)
                .HasMaxLength(40);

            modelBuilder.Entity<Board>()
                .Property(e => e.Designation)
                .IsUnicode(false)
                .HasMaxLength(100);

            modelBuilder.Entity<Board>()
                .HasMany(e => e.Tests)
                .WithOptional(e => e.Board)
                .HasForeignKey(e => e.Id_Board);

            // Ligne configuration
            modelBuilder.Entity<Ligne>()
                .Property(e => e.Nom_Ligne)
                .IsUnicode(false)
                .HasMaxLength(40);

            modelBuilder.Entity<Ligne>()
                .HasMany(e => e.Products)
                .WithOptional(e => e.Ligne)
                .HasForeignKey(e => e.Id_Ligne);

            // Operateur configuration
            modelBuilder.Entity<Operateur>()
                .Property(e => e.Id)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Operateur>()
                .Property(e => e.Nom)
                .IsUnicode(false)
                .HasMaxLength(15);

            modelBuilder.Entity<Operateur>()
                .Property(e => e.Prénom)
                .IsUnicode(false)
                .HasMaxLength(15);

            modelBuilder.Entity<Operateur>()
                .Property(e => e.Matricule)
                .IsUnicode(false)
                .HasMaxLength(5);

            modelBuilder.Entity<Operateur>()
                .Property(e => e.Fonction)
                .IsUnicode(false)
                .HasMaxLength(15);
            // Dans OnModelCreating
            modelBuilder.Entity<Operateur>()
                .Property(e => e.Email)
                .IsUnicode(false)
                .HasMaxLength(100)
                .IsOptional();

            modelBuilder.Entity<Operateur>()
                .Property(e => e.Service)
                .IsUnicode(false)
                .HasMaxLength(15);

            modelBuilder.Entity<Operateur>()
                .HasMany(e => e.Tests)
                .WithOptional(e => e.Operateur)
                .HasForeignKey(e => e.Id_Operateur);

            // Test configuration
            modelBuilder.Entity<Test>()
                .Property(e => e.Id)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Test>()
                .Property(e => e.Id_Board)
                .HasPrecision(18, 0)
                .IsOptional();

            modelBuilder.Entity<Test>()
                .Property(e => e.Num_Serie)
                .IsUnicode(false)
                .HasMaxLength(40);

            modelBuilder.Entity<Test>()
                .Property(e => e.Id_Machine)
                .HasPrecision(18, 0)
                .IsOptional();

            modelBuilder.Entity<Test>()
                .Property(e => e.Id_Operateur)
                .HasPrecision(18, 0)
                .IsOptional();

            modelBuilder.Entity<Test>()
                .Property(e => e.Id_Product)
                .HasPrecision(18, 0)
                .IsOptional();

            modelBuilder.Entity<Test>()
                .Property(e => e.TypeTest)
                .IsUnicode(false)
                .HasMaxLength(20);

            modelBuilder.Entity<Test>()
                .Property(e => e.TestSoftwareVersion)
                .IsUnicode(false)
                .HasMaxLength(50);

            modelBuilder.Entity<Test>()
                .Property(e => e.StatutTest)
                .IsUnicode(false)
                .HasMaxLength(20);

            // Product configuration
            modelBuilder.Entity<Product>()
                .Property(e => e.Id)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Product>()
                .Property(e => e.Num_Serie)
                .IsUnicode(false)
                .HasMaxLength(50);

            modelBuilder.Entity<Product>()
                .Property(e => e.Id_Ligne)
                .IsOptional();

            modelBuilder.Entity<Product>()
                .HasMany(e => e.Tests)
                .WithOptional(e => e.Product)
                .HasForeignKey(e => e.Id_Product);

            // Machine configuration
            modelBuilder.Entity<Machine>()
                .Property(e => e.Id)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Machine>()
                .Property(e => e.CodeMachine)
                .IsUnicode(false)
                .HasMaxLength(20);

            modelBuilder.Entity<Machine>()
                .Property(e => e.NomMachine)
                .IsUnicode(false)
                .HasMaxLength(50);

            modelBuilder.Entity<Machine>()
                .Property(e => e.TypeTest)
                .IsUnicode(false)
                .HasMaxLength(20);

            modelBuilder.Entity<Machine>()
                .Property(e => e.SoftwareVersion)
                .IsUnicode(false)
                .HasMaxLength(30);

            modelBuilder.Entity<Machine>()
                .Property(e => e.Description)
                .IsUnicode(false)
                .HasMaxLength(100);

            modelBuilder.Entity<Machine>()
                .HasMany(e => e.Tests)
                .WithOptional(e => e.Machine)
                .HasForeignKey(e => e.Id_Machine);

            // Loging configuration (MIS À JOUR avec Email)
            modelBuilder.Entity<Profil>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Profil>()
                .Property(e => e.Username)
                .IsUnicode(false)
                .HasMaxLength(50);

            modelBuilder.Entity<Profil>()
                .Property(e => e.Password)
                .IsUnicode(false)
                .HasMaxLength(255);

            modelBuilder.Entity<Profil>()
                .Property(e => e.Role)
                .IsUnicode(false)
                .HasMaxLength(20);

            modelBuilder.Entity<Profil>()
                .Property(e => e.CreateDate)
                .HasColumnType("datetime");

            modelBuilder.Entity<Profil>()
                .Property(e => e.Id_Operateur)
                .HasPrecision(18, 0)
                .IsOptional();

            modelBuilder.Entity<Profil>()
                .Property(e => e.Email)
                .IsUnicode(false)
                .HasMaxLength(100)
                .IsOptional();

            // Relation Loging-Operateur
            modelBuilder.Entity<Profil>()
                .HasOptional(e => e.Operateur)
                .WithMany()
                .HasForeignKey(e => e.Id_Operateur);
        }
    }
}