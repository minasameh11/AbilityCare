using ChurchService.Models;
using Microsoft.EntityFrameworkCore;
using profile.Models;
using System.Diagnostics;
using System.IO;

namespace profile
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services
            builder.Services.AddControllersWithViews();

            // Configure SQLite
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // ✅ إنشاء قاعدة البيانات تلقائيًا لو مش موجودة
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            }

            // ✅ فتح المتصفح تلقائيًا على الصفحة الرئيسية
            var url = "http://localhost:5000";
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ خطأ أثناء فتح المتصفح: " + ex.Message);
                }
            });

            // Middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
