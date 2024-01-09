using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_Management_System.DataContext;
using RMS_Management_System.Migrations.Models;
using RMS_Management_System.Models;
using System.Configuration;

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


        //CRUD ON COURSE TABLE

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var courseTodelete = context.Courses.Include(c => c.Students).Where(c => c.CourseId == id).FirstOrDefault();
            if(courseTodelete !=null)
            {
                var pdfTodeletePath = Path.Combine(webHostEnvironment.WebRootPath, "PdfFolder", courseTodelete.CoursePdf);
                context.Courses.Remove(courseTodelete);
                await context.SaveChangesAsync();

                if(System.IO.File.Exists(pdfTodeletePath))
                {
                    System.IO.File.Delete(pdfTodeletePath);
                }

                return RedirectToAction("GetCourseList", "Courses");
            }
            return View();
        }





 /*       public async Task<IActionResult> Edit(int id)
        {
            // Retrieve the course data from the database or other source
            var course = await context.Courses.Where(c => c.CourseId == id).FirstOrDefaultAsync();// your logic to get the course by id

            // Ensure course is not null before passing it to the view
            if (course != null)
            {
                var model = new CourseViewEdit
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseFee = course.CourseFee,
                    CoursePdf = course.CoursePdf,
                    // other properties
                };

                return View(model);
            }

            // Handle the case when the course is not found
            return NotFound();
        }
*/





        
                [HttpGet]
                public async Task<IActionResult> Edit(int id)
                {
                    var course = await context.Courses.Where(c => c.CourseId == id).FirstOrDefaultAsync();
                    if(course!=null)
                    {
                        var courseview = await context.Courses.Where(x => x.CourseId == id).Select(s => new CourseViewEdit
                        {
                            CourseId = s.CourseId,
                            CourseName = s.CourseName,
                            CourseFee = s.CourseFee,
                            CoursePdf = s.CoursePdf,
                        }).FirstOrDefaultAsync();

                        return View(courseview);
                    }
                    return NotFound();
                }


        /* [HttpPost, ActionName("Edit")]
         public async Task<IActionResult> Edit(int id, [Bind("CourseId, CourseName, CourseFee, CoursePdf")] CourseViewEdit course, IFormFile? newPdf)
         {
             var oldcourse = await context.Courses.FindAsync(course.CourseId);
             string newpdffile = null;
             if (ModelState.IsValid)
             {
                 if (newPdf != null && newPdf.Length > 0)
                 {
                     newpdffile = GetUniqueFileName(newPdf.FileName);
                     var getpath = Path.Combine(webHostEnvironment.WebRootPath, "PdfFolder", newpdffile);
                     var newfilepath = Path.Combine(newpdffile, getpath);
                     var oldpdfpath = Path.Combine(webHostEnvironment.WebRootPath, "PdfFolder", course.CoursePdf);
                     using (var filestream = new FileStream(newfilepath, FileMode.Create))
                     {
                         await newPdf.CopyToAsync(filestream);
                     }

                     if (System.IO.File.Exists(oldpdfpath))
                     {
                         System.IO.File.Delete(oldpdfpath);
                     }

                 }
                 oldcourse.CourseId = course.CourseId;
                 oldcourse.CourseName = course.CourseName;
                 oldcourse.CourseFee = course.CourseFee;
                 if (newPdf != null)
                 {
                     oldcourse.CoursePdf = newpdffile;
                 }
                 else
                 {
                     course.CoursePdf = oldcourse.CoursePdf;
                 }

                 context.Courses.Update(oldcourse);
                 await context.SaveChangesAsync();

             }
             else
             {
                 foreach (var modelState in ModelState.Values)
                 {
                     foreach (var error in modelState.Errors)
                     {
                         Console.WriteLine($"Model Error: {error.ErrorMessage}");
                     }
                 }
             }
             return View();
         }*/







        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId, CourseName, CourseFee, CoursePdf")] CourseViewEdit course, IFormFile? newPdf)
        {
            var oldcourse = await context.Courses.FindAsync(course.CourseId);

            if (oldcourse == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (newPdf != null && newPdf.Length > 0)
                {
                    // Handle the new PDF upload
                    var newPdfFileName = GetUniqueFileName(newPdf.FileName);
                    var newPdfFilePath = Path.Combine(webHostEnvironment.WebRootPath, "PdfFolder", newPdfFileName);

                    using (var fileStream = new FileStream(newPdfFilePath, FileMode.Create))
                    {
                        await newPdf.CopyToAsync(fileStream);
                    }

                    // Delete the old PDF file
                    if (!string.IsNullOrEmpty(oldcourse.CoursePdf))
                    {
                        var oldPdfPath = Path.Combine(webHostEnvironment.WebRootPath, "PdfFolder", oldcourse.CoursePdf);
                        if (System.IO.File.Exists(oldPdfPath))
                        {
                            System.IO.File.Delete(oldPdfPath);
                        }
                    }

                    // Update the CoursePdf property with the new file name
                    oldcourse.CoursePdf = newPdfFileName;
                }

                // Update other properties
                oldcourse.CourseName = course.CourseName;
                oldcourse.CourseFee = course.CourseFee;

                // Save changes to the database
                context.Courses.Update(oldcourse);
                await context.SaveChangesAsync();

                return RedirectToAction("GetCourseList");
            }

            // If ModelState is not valid, return to the edit view with errors
            return View(course);
        }


        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                   + "_"
                   + Guid.NewGuid().ToString().Substring(0, 4)
                   + Path.GetExtension(fileName);
        }

    }
}
