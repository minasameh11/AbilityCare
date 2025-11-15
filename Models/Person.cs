using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChurchService.Models
{
    public class Person
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم بالكامل")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Display(Name = "الرقم القومي")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "يجب أن يكون الرقم القومي 14 رقمًا")]
        public string? NationalId { get; set; }

        [Display(Name = "تاريخ الميلاد")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "النوع مطلوب")]
        [Display(Name = "النوع")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "رقم الموبايل مطلوب")]
        [Display(Name = "رقم الموبايل 1")]
        [Phone]
        public string Phone1 { get; set; }

        [Display(Name = "رقم الموبايل 2")]
        [Phone]
        public string? Phone2 { get; set; }

        [Required(ErrorMessage = "العنوان التفصيللي مطلوب")]
        [Display(Name = "العنوان التفصيلي")]
        [StringLength(250)]
        public string Address { get; set; }

        [Required(ErrorMessage = "المنطقة مطلوبه ")]
        [Display(Name = "المنطقة")]
        [StringLength(100)]
        public string Area { get; set; }

        [Required(ErrorMessage = " نوع الاعاقه مطلوب")]
        [Display(Name = "نوع الإعاقة")]
        [StringLength(100)]
        public string DisabilityType { get; set; }

        [Required(ErrorMessage = "درجه الاعاقه مطلوبه")]
        [Display(Name = "درجة الإعاقة")]
        [StringLength(50)]
        public string DisabilityDegree { get; set; }

        [Display(Name = "ملاحظات")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }


        //[Required(ErrorMessage = "الصوره الشخصيه مطلوبه  ")]
        [Display(Name = "الصورة الشخصية")]
        public string? PhotoPath { get; set; }

        public List<PersonImage> Images { get; set; } = new();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();


    }
}
