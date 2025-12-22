using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Histo_Product.Models
{
    [Table("Test")]
    public partial class Test
    {
        public Test() { }

        [Column(TypeName = "numeric")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Id_Board { get; set; }

        [StringLength(40)]
        public string Num_Serie { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Id_Machine { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Id_Operateur { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool? Result { get; set; }

        [StringLength(20)]
        public string TypeTest { get; set; }

        [StringLength(50)]
        public string TestSoftwareVersion { get; set; }

        [StringLength(20)]
        public string StatutTest { get; set; } = "En cours";
        public DateTime? DateDebutReel { get; set; }
        public DateTime? DateFinReel { get; set; }

        [Column(TypeName = "numeric")]
        [ForeignKey("Product")]
        public decimal? Id_Product { get; set; }
        public virtual Product Product { get; set; }
        public bool? ResultatTemporaire { get; set; }
        public DateTime? DateSelection { get; set; }

        public virtual Board Board { get; set; }
        [ForeignKey("Id_Machine")]
        public virtual Machine Machine { get; set; }
        public virtual Operateur Operateur { get; set; }
    }
}