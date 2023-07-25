using System.ComponentModel.DataAnnotations;

namespace EmployeeForm.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Display(Name = "Birthdate")]
        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; }

        public string Image { get; set; }

        public string Email { get; set; }

        [Display(Name = "Contact Number")]
        [Required]
        public string MobileNumber { get; set; }
        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }
        public List<CasteCategory> CategoryList { get; set; }
        public List<EmployeeAddress> AddressList { get; set; }
        
    }
}
