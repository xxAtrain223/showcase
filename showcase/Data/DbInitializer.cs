using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using showcase.Models;
using showcase.Controllers;
using Microsoft.Extensions.Logging;

namespace showcase.Data
{
    public class DbInitializer
    {
        public static void Initialize(ApplicationDbContext db, ILogger<Program> logger)
        {
            // Seed test resumes
            // Category Name, Number of Versions
            Tuple<string, int>[] categories = new Tuple<string, int>[] {
                new Tuple<string, int>("alpha", 3),
                new Tuple<string, int>("bravo", 3),
                new Tuple<string, int>("charlie", 3)
            };

            // Company Name, Number of Versions
            Tuple<string, int>[] companies = new Tuple<string, int>[] {
                new Tuple<string, int>("delta", 3),
                new Tuple<string, int>("echo", 3),
                new Tuple<string, int>("foxtrot", 3)
            };

            // Seed Categories
            if (!db.ResumeCategories.Any())
            {
                for (int i = 0; i < categories.Length; i++)
                {
                    db.ResumeCategories.Add(new ResumeCategory
                    {
                        //Id = i,
                        Name = categories[i].Item1,
                        Resumes = new List<Resume>(),
                        Description = String.Format("<p>Description for {0}</p>", categories[i].Item1)
                    });
                }
            }

            // Seed Companies
            if (!db.ResumeCompanies.Any())
            {
                for (int i = 0; i < companies.Length; i++)
                {
                    db.ResumeCompanies.Add(new ResumeCompany
                    {
                        //Id = i,
                        Name = companies[i].Item1,
                        Resumes = new List<Resume>()
                    });
                }
            }

            db.SaveChanges();

            // Seed Resumes
            if (!db.Resumes.Any())
            {
                // Seed category resumes
                foreach (var category in categories)
                {
                    ResumeCategory dbCategory = db.ResumeCategories.Where(c => c.Name == category.Item1).First();

                    for (int i = 0; i < category.Item2; i++)
                    {
                        string resumeName = String.Format("category-{0}-v{1}", category.Item1, i);
                        logger.LogDebug("Adding {0}", resumeName);
                        string seedPath = String.Format("Data/seed/{0}.pdf", resumeName);
                        string destFilename = String.Format("{0}.pdf", Guid.NewGuid());

                        System.IO.File.Copy(seedPath, "wwwroot/resumes/" + destFilename);

                        db.Resumes.Add(new Resume
                        {
                            Category = dbCategory,
                            FileName = destFilename,
                            Version = i
                        });
                    }
                }

                // Seed company resumes
                foreach (var company in companies)
                {
                    ResumeCompany dbCompany = db.ResumeCompanies.Where(c => c.Name == company.Item1).First();

                    for (int i = 0; i < company.Item2; i++)
                    {
                        string resumeName = String.Format("company-{0}-v{1}", company.Item1, i);
                        logger.LogDebug("Adding {0}", resumeName);
                        string seedPath = String.Format("Data/seed/{0}.pdf", resumeName);
                        string destFilename = String.Format("{0}.pdf", Guid.NewGuid());

                        System.IO.File.Copy(seedPath, "wwwroot/resumes/" + destFilename);

                        db.Resumes.Add(new Resume
                        {
                            Company = dbCompany,
                            FileName = destFilename,
                            Version = i
                        });
                    }
                }

                Console.WriteLine(db.Resumes.Count());

                db.SaveChanges();
            }
        }
    }
}
