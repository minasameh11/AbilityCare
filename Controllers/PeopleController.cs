using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChurchService.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using static System.Net.Mime.MediaTypeNames;

namespace ChurchService.Controllers
{
    public class PeopleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PeopleController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ============================================================
        // 🟢 INDEX - عرض الكل + البحث + pagination بسيط
        // ============================================================
        public async Task<IActionResult> Index(string searchName, string searchArea, string searchDisabilityDegree, string searchGender, int pageNumber = 1, int pageSize = 10)
        {
            IQueryable<Person> query = _context.People.AsQueryable();

            // ✅ الفلاتر
            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(p => EF.Functions.Like(p.FullName, $"%{searchName}%"));

            if (!string.IsNullOrWhiteSpace(searchArea))
                query = query.Where(p => EF.Functions.Like(p.Area, $"%{searchArea}%"));

            if (!string.IsNullOrWhiteSpace(searchDisabilityDegree))
                query = query.Where(p => EF.Functions.Like(p.DisabilityDegree, $"%{searchDisabilityDegree}%"));

            if (!string.IsNullOrWhiteSpace(searchGender))
                query = query.Where(p => p.Gender == searchGender);

            // ✅ إجمالي عدد الأشخاص بعد الفلترة
            int totalItems = await query.CountAsync();

            // ✅ جلب البيانات مع الترتيب والتقسيم
            var people = await query
                .OrderByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ✅ تمرير البيانات للـ View
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.SearchName = searchName;
            ViewBag.SearchArea = searchArea;
            ViewBag.SearchDisability = searchDisabilityDegree;
            ViewBag.SearchGender = searchGender;

            return View(people);
        }



        // GET: People/Dashboard

        /// <summary>
        /// ////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        /// 

        // ✅ صفحة الداشبورد
        public IActionResult Dashboard()
        {
            // 🧮 إحصائيات عامة
            ViewBag.TotalPeople = _context.People.Count();
            ViewBag.MaleCount = _context.People.Count(p => p.Gender == "ذكر");
            ViewBag.FemaleCount = _context.People.Count(p => p.Gender == "أنثى");
            ViewBag.DisabledCount = _context.People.Count(p => !string.IsNullOrEmpty(p.DisabilityType));

            // ♿ أكثر نوع إعاقة
            ViewBag.TopDisability = _context.People
                .Where(p => !string.IsNullOrEmpty(p.DisabilityType))
                .GroupBy(p => p.DisabilityType)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "غير محدد";

            // 🗓️ عدد الاجتماعات
            var meetingCount = _context.Meetings.Count();
            ViewBag.MeetingCount = meetingCount;

            // 🧠 تحميل الحضور مرة واحدة لتسريع الأداء
            var allAttendances = _context.Attendances.AsNoTracking().ToList();
            var people = _context.People.AsNoTracking().ToList();

            // 📊 حساب نسبة الحضور الإجمالية
            double attendanceRate = 0;
            if (meetingCount > 0 && people.Any())
            {
                var totalPossible = meetingCount * people.Count;
                var totalPresent = allAttendances.Count(a => a.IsPresent);
                attendanceRate = (double)totalPresent / totalPossible * 100;
            }
            ViewBag.AttendanceRate = attendanceRate;

            // 📉 أعلى نسبة غياب
            var personAbsenceRates = allAttendances
                .GroupBy(a => a.PersonId)
                .Select(g => new
                {
                    PersonId = g.Key,
                    AbsenceRate = 100.0 * g.Count(a => !a.IsPresent) / g.Count()
                })
                .OrderByDescending(x => x.AbsenceRate)
                .ToList();

            ViewBag.MaxAbsenceRate = personAbsenceRates.FirstOrDefault()?.AbsenceRate.ToString("0.0") ?? "0";

            // 🏅 الأكثر التزامًا بالحضور
            var bestAttendee = allAttendances
                .GroupBy(a => a.PersonId)
                .Select(g => new
                {
                    PersonId = g.Key,
                    AttendanceRate = 100.0 * g.Count(a => a.IsPresent) / g.Count()
                })
                .OrderByDescending(x => x.AttendanceRate)
                .FirstOrDefault();

            if (bestAttendee != null)
            {
                var person = people.FirstOrDefault(p => p.Id == bestAttendee.PersonId);
                ViewBag.TopAttendeeName = person?.FullName ?? "غير معروف";
                ViewBag.TopAttendeeRate = bestAttendee.AttendanceRate.ToString("0.0");
            }
            else
            {
                ViewBag.TopAttendeeName = "لا يوجد";
                ViewBag.TopAttendeeRate = "0";
            }

            // 🚨 الأشخاص كثيرو الغياب (أكثر من 50%)
            var frequentAbsentees = personAbsenceRates
                .Where(x => x.AbsenceRate > 50)
                .Select(x => people.FirstOrDefault(p => p.Id == x.PersonId))
                .Where(p => p != null)
                .ToList();
            ViewBag.FrequentAbsentees = frequentAbsentees;

            // 📍 توزيع الأشخاص حسب المنطقة
            var areaStats = people
                .GroupBy(p => p.Area)
                .Select(g => (g.Key ?? "غير محدد", g.Count()))
                .ToList();
            ViewBag.AreaStats = areaStats;

            // 🔝 أكثر منطقة فيها غياب
            ViewBag.TopAbsentArea = allAttendances
                .Where(a => !a.IsPresent)
                .Join(_context.People, a => a.PersonId, p => p.Id, (a, p) => p.Area)
                .GroupBy(area => area)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "لا يوجد";

            return View();
        }











        // ============================================================
        // 🟢 DETAILS - عرض التفاصيل
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.People
    .Include(p => p.Attendances)
        .ThenInclude(a => a.Meeting)
    .FirstOrDefaultAsync(m => m.Id == id);


            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }


        // ============================================================
        // 🟢 CREATE - إضافة شخص جديد
        // ============================================================
        public IActionResult Create()
        {
            return View(new Person());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Person person, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                if (photo != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                    var path = Path.Combine(_env.WebRootPath, "images", fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await photo.CopyToAsync(stream);
                    person.PhotoPath = "/images/" + fileName;
                }
                else
                {
                    // ✅ حط صورة افتراضية لو المستخدم ما رفعش
                    person.PhotoPath = "/images/default.jpg";
                }

                _context.People.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(person);
        }


        // ============================================================
        // 🟢 EDIT - تعديل بيانات شخص
        // ============================================================
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null) return NotFound();

        //    var person = await _context.People.FindAsync(id);
        //    if (person == null) return NotFound();

        //    return View(person);
        //}

        // ✅ GET: People/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var person = await _context.People.FindAsync(id);
            if (person == null) return NotFound();

            return View(person);
        }

        // ✅ POST: People/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Person person, IFormFile? photo)
        {
            if (id != person.Id) return NotFound();

            // نجيب النسخة القديمة من قاعدة البيانات
            var existingPerson = await _context.People.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (existingPerson == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // لو المستخدم رفع صورة جديدة
                    if (photo != null)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                        var path = Path.Combine(_env.WebRootPath, "images", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }

                        person.PhotoPath = "/images/" + fileName;
                    }
                    else
                    {
                        // لو ما رفعش صورة جديدة، نحافظ على القديمة
                        person.PhotoPath = existingPerson.PhotoPath;
                    }

                    _context.Update(person);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "✅ تم تعديل بيانات الشخص بنجاح.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.People.Any(e => e.Id == person.Id))
                        return NotFound();
                    throw;
                }
            }

            return View(person);
        }


        // GET: People/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var person = await _context.People.FirstOrDefaultAsync(p => p.Id == id);
            if (person == null) return NotFound();

            return View(person);
        }

        // POST: People/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var person = await _context.People.FindAsync(id);
            if (person == null) return NotFound();

            // حذف الصورة من wwwroot لو موجودة
            if (!string.IsNullOrEmpty(person.PhotoPath))
            {
                try
                {
                    var fullPath = Path.Combine(_env.WebRootPath, person.PhotoPath.TrimStart('/', '\\'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
                catch
                {
                    // لو حصل خطأ في حذف الملف، ممكن تتجاهل أو تسجل اللوج
                }
            }

            _context.People.Remove(person);
            await _context.SaveChangesAsync();

            TempData["Success"] = "🗑️ تم حذف الشخص بنجاح.";
            return RedirectToAction(nameof(Index));
        }

    }


}

