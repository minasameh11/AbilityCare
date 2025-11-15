using System.ComponentModel.DataAnnotations;

namespace ChurchService.Models
{
    public class PersonImage
    {
        public int Id { get; set; }

        [Display(Name = "رقم الشخص")]
        public int PersonId { get; set; }

        [Display(Name = "مسار الصورة")]
        public string ImagePath { get; set; }

        [Display(Name = "وصف الصورة")]
        public string Description { get; set; }

        public Person Person { get; set; }
    }
}
