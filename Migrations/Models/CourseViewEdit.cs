using System.ComponentModel.DataAnnotations;

namespace RMS_Management_System.Migrations.Models
{
    public class CourseViewEdit
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        public string CourseName { get; set; }

        [Required]
        public string CourseFee { get; set; }

        
        public string CoursePdf { get; set; }
    }
}
