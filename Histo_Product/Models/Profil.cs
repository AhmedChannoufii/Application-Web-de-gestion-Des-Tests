using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Histo_Product.Models
{
    [Table("Profils")]  // ← CHANGÉ ICI : "Loging" → "Profils"
    public class Profil  // ← CHANGÉ ICI : Loging → Profil
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;

        [Column(TypeName = "numeric")]
        public decimal? Id_Operateur { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [ForeignKey("Id_Operateur")]
        public virtual Operateur Operateur { get; set; }
    }
}