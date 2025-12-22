using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Histo_Product.Models
{
    [Table("Operateur")]
    public partial class Operateur
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Operateur()
        {
            Tests = new HashSet<Test>();
        }

        [Column(TypeName = "numeric")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }

        [StringLength(15)]
        public string Nom { get; set; }

        [StringLength(15)]
        public string Prénom { get; set; }

        [StringLength(5)]
        public string Matricule { get; set; }

        [StringLength(15)]
        public string Fonction { get; set; }

        [StringLength(15)]
        public string Service { get; set; }
        [StringLength(100)] // ✅ NOUVEAU CHAMP
        [EmailAddress]      // ✅ VALIDATION E
        public string Email { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Test> Tests { get; set; }
    }
}