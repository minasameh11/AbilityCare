using System.ComponentModel.DataAnnotations;

namespace ChurchService.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        public int PersonId { get; set; }
        public Person Person { get; set; }

        [Required]
        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; }

        [Display(Name = "حضر؟")]
        public bool IsPresent { get; set; }
    }
}
