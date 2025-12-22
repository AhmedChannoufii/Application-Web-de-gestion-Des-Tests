using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Histo_Product.Models
{
    [Table("Ligne")]
    public partial class Ligne
    {
        public Ligne()
        {
            Products = new HashSet<Product>();
        }

        public short Id { get; set; }

        [Required]
        [StringLength(40)]
        public string Nom_Ligne { get; set; }
        public bool? Valide { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}