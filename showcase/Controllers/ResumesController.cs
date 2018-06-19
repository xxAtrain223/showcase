using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using showcase.UtilityFunctions;
using Microsoft.Extensions.Configuration;

namespace showcase.Controllers
{
    public class ResumesController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly IHostingEnvironment env;

        public ResumesController(ApplicationDbContext context, IHostingEnvironment environment, ApplicationSettings settings)
            : base(settings)
        {
            db = context;
            env = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await db.
                ResumeCategories.
                Include(c => c.Resumes).
                Select(c => new ResumeCategory
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Resumes = c.Resumes.OrderBy(r => r.Version).ToList()
                }).
                OrderBy(c => c.Name).
                ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> CompanyResumeLink(string name, int? version)
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
                    OrderBy(r => r.Version).
                    Where(r => r.Version >= version).
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
        public async Task<IActionResult> CategoryResumeLink(string name, int? version)
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
                    OrderBy(r => r.Version).
                    Where(r => r.Version >= version).
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
        public async Task<IActionResult> Upload()
        {
            return View(new ResumeUploadViewModel
            {
                Categories = JsonConvert.SerializeObject(await db.ResumeCategories.Select(c => c.Name).ToListAsync()),
                Companies = JsonConvert.SerializeObject(await db.ResumeCompanies.Select(c => c.Name).ToListAsync())
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload([Bind("Category,Company,FormFile,Categories,Companies")]ResumeUploadViewModel resume)
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
                ShowcaseUtilities.SaveStreamToFile(stream, string.Format("{0}/resumes/{1}", env.WebRootPath, filename));
            }

            List<Resume> resumes = (category?.Resumes ?? company?.Resumes)?
                .OrderBy(r => r.Version).ToList();

            Resume newResume = new Resume
            {
                Category = category,
                Company = company,
                FileName = filename,
                Version = resumes?.LastOrDefault()?.Version + 1 ?? 0
            };

            db.Resumes.Add(newResume);
            await db.SaveChangesAsync();

            HttpContext.Session.Set("NewResume", newResume);

            return RedirectToAction("Uploaded");
        }
        
        public IActionResult Uploaded()
        {
            Resume newResume = HttpContext.Session.Get<Resume>("NewResume");
            return View(newResume);
        }

        public IActionResult Manage()
        {
            return View();
        }
        
        [HttpPost]
        [HttpPut]
        [ActionName("Category")]
        public IActionResult PutCategory(int? id, string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return BadRequest("\"name\" is null or whitespace");
            }

            if (id != null)
            {
                ResumeCategory category = db.ResumeCategories.Find(id);

                if (category == null)
                {
                    return NotFound("Category not found");
                }

                category.Name = name;
            }
            else
            {
                db.ResumeCategories.Add(new ResumeCategory { Name = name });
            }

            db.SaveChanges();

            return Ok();
        }

        [HttpPost]
        public IActionResult EditCategoryDescription(int? id, string description)
        {
            if (id == null)
            {
                return BadRequest("\"id\" is null");
            }

            if (String.IsNullOrWhiteSpace(description))
            {
                return BadRequest("\"description\" is null or whitespace");
            }

            ResumeCategory category = db.ResumeCategories.Find(id);

            if (category == null)
            {
                return NotFound("Category not found");
            }
            
            category.Description = String.Join("\n", 
                WebUtility.HtmlEncode(description)
                    .Replace("\r", "")
                    .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => String.Format("<p>{0}</p>", s)));

            db.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [ActionName("Category")]
        public IActionResult GetCategory(int? id)
        {
            if (id == null)
            {
                return BadRequest("\"id\" is null");
            }

            ResumeCategory category = db.ResumeCategories.
                Include(c => c.Resumes).
                Where(c => c.Id == id).
                FirstOrDefault();

            if (category != null)
            {
                return Json(new
                {
                    category.Id,
                    category.Name,
                    Resumes = category.Resumes.Select(r => new { r.Id, r.Version })
                });
            }
            else
            {
                return NotFound("Category not found");
            }
        }

        [HttpDelete]
        [ActionName("Category")]
        public IActionResult DeleteCategory(int? id)
        {
            if (id == null)
            {
                return BadRequest("\"id\" is null");
            }
            
            ResumeCategory category = db.ResumeCategories.Include(c => c.Resumes).Where(c => c.Id == id).FirstOrDefault();

            if (category == null)
            {
                return NotFound("Category not found");
            }

            db.Resumes.RemoveRange(category.Resumes);

            db.ResumeCategories.Remove(category);

            db.SaveChanges();

            return Ok();
        }

        [HttpGet]
        public IActionResult GetCategories(int page = 1, int pagesize = 10)
        {
            if (page < 1)
            {
                return BadRequest("Page is less than 1");
            }

            if (pagesize < 1)
            {
                return BadRequest("Pagesize is less than 1");
            }

            IList<ResumeCategory> categories = db.ResumeCategories.
                Include(c => c.Resumes).
                Skip((page - 1) * pagesize).
                Take(pagesize).
                ToList();
            
            int numberOfPages = (int)Math.Ceiling((double)db.ResumeCategories.Count() / pagesize);
            
            return Json(new
            {
                numberOfPages,
                currentPage = page,
                Categories = categories.Select(c => new {
                    c.Id,
                    c.Name,
                    c.Description,
                    Resumes = c.Resumes.
                        Select(r => new { r.Id, r.Version }).
                        OrderBy(r => r.Version)
                })
            });
        }

        [HttpPost]
        [HttpPut]
        [ActionName("Company")]
        public IActionResult PutCompany(int? id, string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return BadRequest("\"name\" is null or whitespace");
            }

            if (id != null)
            {
                ResumeCompany company = db.ResumeCompanies.Find(id);

                if (company == null)
                {
                    return NotFound("Company not found");
                }

                company.Name = name;
            }
            else
            {
                db.ResumeCompanies.Add(new ResumeCompany { Name = name });
            }

            db.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [ActionName("Company")]
        public IActionResult GetCompany(int? id)
        {
            if (id == null)
            {
                return BadRequest("\"id\" is null");
            }

            ResumeCompany company = db.ResumeCompanies.
                Include(c => c.Resumes).
                Where(c => c.Id == id).
                FirstOrDefault();

            if (company != null)
            {
                return Json(new
                {
                    company.Id,
                    company.Name,
                    Resumes = company.Resumes.Select(r => new { r.Id, r.Version })
                });
            }
            else
            {
                return NotFound("Company not found");
            }
        }

        [HttpDelete]
        [ActionName("Company")]
        public IActionResult DeleteCompany(int? id)
        {
            if (id == null)
            {
                return BadRequest("\"id\" is null");
            }

            ResumeCompany company = db.ResumeCompanies.Include(c => c.Resumes).Where(c => c.Id == id).FirstOrDefault();

            if (company == null)
            {
                return NotFound("Company not found");
            }

            db.Resumes.RemoveRange(company.Resumes);

            db.ResumeCompanies.Remove(company);

            db.SaveChanges();

            return Ok();
        }

        [HttpGet]
        public IActionResult GetCompanies(int page = 1, int pagesize = 10)
        {
            if (page < 1)
            {
                return BadRequest("Page is less than 1");
            }

            if (pagesize < 1)
            {
                return BadRequest("Pagesize is less than 1");
            }

            IList<ResumeCompany> companies = db.ResumeCompanies.
                Include(c => c.Resumes).
                Skip((page - 1) * pagesize).
                Take(pagesize).
                ToList();

            int numberOfPages = (int)Math.Ceiling((double)db.ResumeCompanies.Count() / pagesize);

            return Json(new
            {
                numberOfPages,
                currentPage = page,
                Companies = companies.Select(c => new {
                    c.Id,
                    c.Name,
                    Resumes = c.Resumes.
                        Select(r => new { r.Id, r.Version }).
                        OrderBy(r => r.Version)
                })
            });
        }

        [HttpDelete]
        [ActionName("Resume")]
        public IActionResult DeleteResume(int? id)
        {
            if (id == null)
            {
                return BadRequest("\"id\" is null");
            }

            Resume resume = db.Resumes.Find(id);

            if (resume == null)
            {
                return NotFound("Resume not found");
            }

            db.Resumes.Remove(resume);
            
            db.SaveChanges();

            return Ok();
        }
    }
}
