using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using showcase.Data;
using showcase.Models;
using showcase.Models.ResumesViewModels;
using Newtonsoft.Json;

namespace showcase.Controllers
{
    public class ResumesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IHostingEnvironment env;
        
        public ResumesController(ApplicationDbContext context, IHostingEnvironment environment)
        {
            db = context;
            env = environment;
        }
        
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View(await db.
                ResumeCategories.
                Include(c => c.Resumes).
                Select(c => new ResumeCategory {
                    Id = c.Id,
                    Name = c.Name,
                    Resumes = c.Resumes.OrderBy(r => r.Version).ToList()
                }).
                OrderBy(c => c.Name).
                ToListAsync());
        }

        [HttpGet]
        public async Task<ActionResult> Company(string name, int? version)
        {
            Resume resume = null;

            if (version == null)
            {
                resume = await db.
                    ResumeCompanies.
                    Where(c => c.Name.Replace(" ", "").ToLower() == name.Replace(" ", "").ToLower()).
                    SelectMany(c => c.Resumes).
                    OrderByDescending(r => r.Version).
                    FirstOrDefaultAsync();
            }
            else
            {
                resume = await db.
                    ResumeCompanies.
                    Where(c => c.Name.Replace(" ", "").ToLower() == name.Replace(" ", "").ToLower()).
                    SelectMany(c => c.Resumes).
                    Where(r => r.Version == version).
                    FirstOrDefaultAsync();
            }

            if (resume != null)
            {
                return PhysicalFile(String.Format("{0}/resumes/{1}", env.WebRootPath, resume.FileName),
                    "application/pdf", String.Format("{0}.v{1}.pdf", name, resume.Version));
            }
            else
            {
                return NotFound();
            }
        }
        
        [HttpGet]
        public async Task<ActionResult> Category(string name, int? version)
        {
            Resume resume = null;

            if (version == null)
            {
                resume = await db.
                    ResumeCategories.
                    Where(c => c.Name.Replace(" ", "").ToLower() == name.Replace(" ", "").ToLower()).
                    SelectMany(c => c.Resumes).
                    OrderByDescending(r => r.Version).
                    FirstOrDefaultAsync();
            }
            else
            {
                resume = await db.
                    ResumeCategories.
                    Where(c => c.Name.Replace(" ", "").ToLower() == name.Replace(" ", "").ToLower()).
                    SelectMany(c => c.Resumes).
                    Where(r => r.Version == version).
                    FirstOrDefaultAsync();
            }

            if (resume != null)
            {
                return PhysicalFile(String.Format("{0}/resumes/{1}", env.WebRootPath, resume.FileName), 
                    "application/pdf", String.Format("{0}.v{1}.pdf", name, resume.Version));
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<ActionResult> Upload()
        {
            return View(new ResumeUploadViewModel
            {
                Categories = JsonConvert.SerializeObject(await db.ResumeCategories.Select(c => c.Name).ToListAsync()),
                Companies = JsonConvert.SerializeObject(await db.ResumeCompanies.Select(c => c.Name).ToListAsync())
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Upload([Bind("Category,Company,FormFile,Categories,Companies")]ResumeUploadViewModel resume)
        {
            // Check Category XOR Company
            if (!(String.IsNullOrWhiteSpace(resume.Category) ^
                String.IsNullOrWhiteSpace(resume.Company)))
            {
                ModelState.AddModelError(string.Empty, "Please input either a category or company name.");
                ModelState.AddModelError("Category", string.Empty);
                ModelState.AddModelError("Company", string.Empty);
            }

            if (resume.FormFile == null)
            {
                ModelState.AddModelError(string.Empty, "Please select a file.");
            }
            
            // Check file type
            if (resume.FormFile?.ContentType != "application/pdf")
            {
                ModelState.AddModelError("FormFile", "File should be a PDF.");
            }

            // Check ModelState
            if (!ModelState.IsValid)
            {
                return View(resume);
            }

            ResumeCategory category = await db.ResumeCategories.Include(c => c.Resumes).Where(c => c.Name == resume.Category).FirstOrDefaultAsync();
            ResumeCompany company = await db.ResumeCompanies.Include(c => c.Resumes).Where(c => c.Name == resume.Company).FirstOrDefaultAsync();

            if (!String.IsNullOrWhiteSpace(resume.Category) && category == null)
            {
                await db.ResumeCategories.AddAsync(new ResumeCategory { Name = resume.Category });
                await db.SaveChangesAsync();
                category = await db.ResumeCategories.Include(c => c.Resumes).Where(c => c.Name == resume.Category).FirstOrDefaultAsync();
            }
            else if (!String.IsNullOrWhiteSpace(resume.Company) && company == null)
            {
                await db.ResumeCompanies.AddAsync(new ResumeCompany { Name = resume.Company });
                await db.SaveChangesAsync();
                company = await db.ResumeCompanies.Include(c => c.Resumes).Where(c => c.Name == resume.Company).FirstOrDefaultAsync();
            }

            string filename = String.Format("{0}.pdf", Guid.NewGuid());
            using (var stream = resume.FormFile.OpenReadStream())
            {
                SaveStreamToFile(stream, String.Format("{0}/resumes/{1}", env.WebRootPath, filename));
            }

            Resume newResume = new Resume
            {
                Category = category,
                Company = company,
                FileName = filename,
                Version = (category?.Resumes?.Count ?? company?.Resumes?.Count) ?? 0
            };

            db.Resumes.Add(newResume);
            await db.SaveChangesAsync();

            //HttpContext.Session.SetString("NewResume", JsonConvert.SerializeObject(newResume));
            HttpContext.Session.Set("NewResume", newResume);

            return RedirectToAction("Uploaded");
        }

        private static void SaveStreamToFile(Stream stream, string filepath)
        {
            using (var fileSystem = System.IO.File.Create(filepath))
            {
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                stream.CopyTo(fileSystem);
            }
        }

        [HttpGet]
        public ActionResult Uploaded()
        {
            Resume newResume = HttpContext.Session.Get<Resume>("NewResume");
            HttpContext.Session.Remove("NewResume");
            return View(newResume);
        }

        /*
        // GET: Resumes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Resume.ToListAsync());
        }

        // GET: Resumes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resume = await _context.Resume
                .SingleOrDefaultAsync(m => m.Id == id);
            if (resume == null)
            {
                return NotFound();
            }

            return View(resume);
        }

        // GET: Resumes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Resumes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Version,FileName")] Resume resume)
        {
            if (ModelState.IsValid)
            {
                _context.Add(resume);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(resume);
        }

        // GET: Resumes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resume = await _context.Resume.SingleOrDefaultAsync(m => m.Id == id);
            if (resume == null)
            {
                return NotFound();
            }
            return View(resume);
        }

        // POST: Resumes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Version,FileName")] Resume resume)
        {
            if (id != resume.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(resume);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResumeExists(resume.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(resume);
        }

        // GET: Resumes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resume = await _context.Resume
                .SingleOrDefaultAsync(m => m.Id == id);
            if (resume == null)
            {
                return NotFound();
            }

            return View(resume);
        }

        // POST: Resumes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resume = await _context.Resume.SingleOrDefaultAsync(m => m.Id == id);
            _context.Resume.Remove(resume);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ResumeExists(int id)
        {
            return _context.Resume.Any(e => e.Id == id);
        }
        */
    }
}
