using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChurchService.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchService.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Attendance/Mark/5
        public async Task<IActionResult> Mark(int meetingId)
        {
            var meeting = await _context.Meetings.FindAsync(meetingId);
            if (meeting == null) return NotFound();

            var attendees = await _context.People.ToListAsync();

            // نجيب الحضور المسجل سابقًا لهذا الاجتماع
            var existingAttendance = await _context.Attendances
                .Where(a => a.MeetingId == meetingId)
                .ToListAsync();

            // نجهز موديل العرض
            var model = attendees.Select(p => new AttendanceViewModel
            {
                PersonId = p.Id,
                PersonName = p.FullName,
                IsPresent = existingAttendance.Any(a => a.PersonId == p.Id && a.IsPresent)
            }).ToList();

            ViewBag.MeetingId = meetingId;
            ViewBag.MeetingName = meeting.Name;
            return View(model);
        }

        // POST: Attendance/Mark
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mark(int meetingId, List<AttendanceViewModel> model)
        {
            // نحذف القديم
            var old = _context.Attendances.Where(a => a.MeetingId == meetingId);
            _context.Attendances.RemoveRange(old);

            // نحفظ الجديد
            foreach (var item in model)
            {
                _context.Attendances.Add(new Attendance
                {
                    MeetingId = meetingId,
                    PersonId = item.PersonId,
                    IsPresent = item.IsPresent
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Meetings");
        }
    }
}
