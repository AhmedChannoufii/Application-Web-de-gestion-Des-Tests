using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Histo_Product.Models
{
    [Table("Machine")]
    public partial class Machine
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Machine()
        {
            Tests = new HashSet<Test>();
        }

        [Key]
        [Column(TypeName = "numeric")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }

        [Required]
        [StringLength(25)]
        public string CodeMachine { get; set; }

        [Required]
        [StringLength(60)]
        public string NomMachine { get; set; }

        [Required]
        [StringLength(20)]
        public string TypeTest { get; set; }

        [Required]
        [StringLength(50)]
        public string SoftwareVersion { get; set; }

        [StringLength(150)]
        public string Description { get; set; }
        public bool EstActif { get; set; } = true;
        public DateTime? DateMiseEnService { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Test> Tests { get; set; }
    }
}