using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace showcase.Models.ResumesViewModels
{
    public class ResumeUploadViewModel
    {
        public string Categories { get; set; }
        public string Companies { get; set; }
        public string Category { get; set; }
        public string Company { get; set; }

        [Required]
        public IFormFile FormFile { get; set; }
    }
}
