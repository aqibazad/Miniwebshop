using System.ComponentModel.DataAnnotations;

namespace Miniwebshop.Models
{
    public class StudentAdmissionViewModel
    {
        // ── Tab 1 : Student Information ──────────────────────────────────────
        [Required(ErrorMessage = "Student name is required.")]
        [Display(Name = "Student Full Name")]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "B-Form number is required.")]
        [RegularExpression(@"^\d{5}-\d{7}-\d$", ErrorMessage = "B-Form number must follow the format 12345-1234567-1.")]
        [Display(Name = "B-Form Number")]
        public string BFormNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required.")]
        [Range(3, 25, ErrorMessage = "Age must be between 3 and 25.")]
        [Display(Name = "Age")]
        public int? Age { get; set; }

        [Required(ErrorMessage = "Blood group is required.")]
        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; } = string.Empty;

        // ── Tab 2 : Parent / Guardian Information ────────────────────────────
        [Required(ErrorMessage = "Guardian type is required.")]
        [Display(Name = "Guardian Type")]
        public string GuardianType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Guardian full name is required.")]
        [Display(Name = "Guardian Full Name")]
        public string GuardianName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Guardian email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Parent / Guardian Email")]
        public string GuardianEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^03\d{9}$", ErrorMessage = "Phone number must be a valid Pakistani mobile number (e.g. 03001234567).")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "CNIC number is required.")]
        [RegularExpression(@"^\d{5}-\d{7}-\d$", ErrorMessage = "CNIC must follow the format 12345-1234567-1.")]
        [Display(Name = "Guardian CNIC Number")]
        public string GuardianCnic { get; set; } = string.Empty;

        [Required(ErrorMessage = "CNIC front details are required.")]
        [Display(Name = "CNIC Front Details (Name, DOB, Address as printed)")]
        public string CnicFrontDetails { get; set; } = string.Empty;

        [Required(ErrorMessage = "CNIC back details are required.")]
        [Display(Name = "CNIC Back Details (Expiry, Religion, etc.)")]
        public string CnicBackDetails { get; set; } = string.Empty;

        [Required(ErrorMessage = "Home address is required.")]
        [Display(Name = "Parent / Guardian Home Address")]
        public string GuardianAddress { get; set; } = string.Empty;
    }
}
