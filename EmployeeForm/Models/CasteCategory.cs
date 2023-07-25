using System.ComponentModel.DataAnnotations;

namespace EmployeeForm.Models
{
    public class CasteCategory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
