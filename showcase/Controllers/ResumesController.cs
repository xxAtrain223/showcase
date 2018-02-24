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
                ToListAsync());
        }

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

        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult Upload(int CategoryId, int CompanyId, IFormFile FormFile)
        {
            if ((CategoryId == -1 && CompanyId == -1) ||
                (CategoryId != -1 && CompanyId != -1))
            {
                return BadRequest("Invalid CategoryId and CompanyId");
            }

            if (FormFile.ContentType != "application/pdf")
            {
                return BadRequest("Invalid File type");
            }

            ResumeCategory category = null;
            ResumeCompany company = null;

            if (CategoryId != -1)
            {
                category = db.ResumeCategories.Find(CategoryId);

                if (category == null)
                {
                    return NotFound("Category not found");
                }
            }
            if (CompanyId != -1)
            {
                company = db.ResumeCompanies.Find(CompanyId);

                if (company == null)
                {
                    return NotFound("Company not found");
                }
            }
            
            string realPath = String.Format("{0}.pdf", Guid.NewGuid());
            using (var stream = FormFile.OpenReadStream())
            {
                SaveStreamToFile(stream, realPath);
            }

            return RedirectToAction("SuccessfullyUploaded");
        }

        private static void SaveStreamToFile(Stream stream, string filepath)
        {
            using (var fileSystem = System.IO.File.Create(filepath))
            {
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                stream.CopyTo(fileSystem);
            }
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
