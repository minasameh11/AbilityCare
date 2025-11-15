using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChurchService.Models
{
    public class Meeting
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الاجتماع مطلوب")]
        [Display(Name = "اسم الاجتماع")]
        public string Name { get; set; }

        [Required(ErrorMessage = "تاريخ الاجتماع مطلوب")]
        [Display(Name = "تاريخ الاجتماع")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public ICollection<Attendance> Attendances { get; set; }
    }
}
