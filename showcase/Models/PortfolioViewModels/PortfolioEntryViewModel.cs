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
        public string ShortDescription { get; set; }
        public string Markdown { get; set; }
        public string Html { get; set; }
        public int? ImageId { get; set; }
    }
}
