using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_Management_System.DataContext;
using RMS_Management_System.Models;

namespace RMS_Management_System.Controllers
{
    public class CoursesController : Controller
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly DatabaseFile context;

        public CoursesController(IWebHostEnvironment webHostEnvironment, DatabaseFile context)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.context = context;
        }

        [HttpGet]
        public IActionResult CourseRegistration()
        {
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CourseRegistration(CourseViewModel cvm)
        {
           if(ModelState.IsValid)
           {
               string PdfLocation = UploadPdf(cvm);

               var course = new Course()
               {
                   CourseName = cvm.CourseName,
                   CourseFee = cvm.CourseFee,
                   CoursePdf = PdfLocation,
               };

              await context.Courses.AddAsync(course);
              await context.SaveChangesAsync();
              return RedirectToAction("Index", "Home");

            } 
            return View();
        }



        private string UploadPdf(CourseViewModel cvm)
        {
            string PdfName = null;
            if (cvm.UploadPdf != null && cvm.UploadPdf.Length > 0)
            {
                string PdfDir = Path.Combine(webHostEnvironment.WebRootPath, "PdfFolder");
                PdfName = Guid.NewGuid().ToString() + "-" + cvm.UploadPdf.FileName;
                string filepath = Path.Combine(PdfDir, PdfName);
                using (var filestream = new FileStream(filepath, FileMode.Create))
                {
                    cvm.UploadPdf.CopyTo(filestream);
                }
            }
            return PdfName;
        }



        public async Task<IActionResult> GetCourseList()
        {
            var data = await context.Courses.ToListAsync();
            return View(data);
        }

        [HttpGet]
        public IActionResult DownloadPdf(int courseId)
        {
            var course = context.Courses.Find(courseId);

            if (course != null && !string.IsNullOrEmpty(course.CoursePdf))
            {
                var pdfPath = Path.Combine(webHostEnvironment.WebRootPath, "PdfFolder", course.CoursePdf);

                if (System.IO.File.Exists(pdfPath))
                {
                    var pdfBytes = System.IO.File.ReadAllBytes(pdfPath);
                    return File(pdfBytes, "application/pdf", $"{course.CourseName}_Course.pdf");
                }
            }

            // Handle if the course or PDF is not found
            return NotFound();
        }
    }
}
