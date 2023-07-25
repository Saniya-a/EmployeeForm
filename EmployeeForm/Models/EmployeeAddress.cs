using System.ComponentModel.DataAnnotations;

namespace EmployeeForm.Models
{
    public class EmployeeAddress
    {
        [Key] 
        public int AddressId { get; set; }
        public int EmployeeId { get; set; }
        public int AddressTypeId { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
        public List<AddressType> AddressTypes { get; set; }
        public List<Country> Countries { get; set; }
        public List<State> States { get; set; }


    }
}
