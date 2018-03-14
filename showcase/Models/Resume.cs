using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace showcase.Models
{
    public class ResumeCategory
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }

        public IList<Resume> Resumes { get; set; }
    }

    public class ResumeCompany
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public IList<Resume> Resumes { get; set; }
    }

    public class Resume
    {
        public int Id { get; set; }

        public ResumeCategory Category { get; set; }

        public ResumeCompany Company { get; set; }

        public int Version { get; set; }

        // A GUID
        [Required]
        public string FileName { get; set; }
    }
}
