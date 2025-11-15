using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChurchService.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchService.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MeetingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ عرض كل الاجتماعات
        public async Task<IActionResult> Index()
        {
            var meetings = await _context.Meetings
                .OrderByDescending(m => m.Date)
                .ToListAsync();

            return View(meetings);
        }

        // ✅ إنشاء اجتماع جديد (GET)
        public IActionResult Create()
        {
            return View();
        }

        // ✅ إنشاء اجتماع جديد (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Meeting meeting)
        {
            if (!ModelState.IsValid)
            {
                _context.Meetings.Add(meeting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(meeting);
        }

        // ✅ تعديل اجتماع (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null)
                return NotFound();

            return View(meeting);
        }

        // ✅ تعديل اجتماع (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Meeting meeting)
        {
            if (id != meeting.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                try
                {
                    _context.Update(meeting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Meetings.Any(m => m.Id == meeting.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(meeting);
        }

        // ✅ حذف اجتماع (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null)
                return NotFound();

            return View(meeting);
        }

        // ✅ حذف اجتماع (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting != null)
            {
                _context.Meetings.Remove(meeting);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // ✅ تفاصيل الاجتماع
        public async Task<IActionResult> Details(int id)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Attendances)
                .ThenInclude(a => a.Person)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meeting == null)
                return NotFound();

            return View(meeting);
        }

    }
}
