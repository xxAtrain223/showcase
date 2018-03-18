using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace showcase.Models.PortfolioViewModels
{
    public class PortfolioEntryViewModel
    {
        public int? Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [Display(Name = "Short Description")]
        public string ShortDescription { get; set; }
        [Required]
        public string Markdown { get; set; }
        [Required]
        public string Html { get; set; }
        [Display(Name = "Image")]
        public int? ImageId { get; set; }
        public Image Image { get; set; }
    }
}
