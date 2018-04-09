using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace showcase.Models
{
    public class PortfolioEntry
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string ShortDescription { get; set; }

        public Image Image { get; set; }

        public string Markdown { get; set; }

        public string Html { get; set; }

        public List<Tag> Tags { get; set; }
    }
}
