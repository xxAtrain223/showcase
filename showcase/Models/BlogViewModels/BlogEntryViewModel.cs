using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace showcase.Models.BlogViewModels
{
    public class BlogEntryViewModel
    {
        public int? Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [Display(Name = "Title Placement")]
        public TitlePlacement TitlePlacement { get; set; }
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
        [Required]
        [Display(Name = "Image Placement")]
        public ImagePlacement ImagePlacement { get; set; }
        [Display(Name = "Show On List")]
        public bool ShowOnList { get; set; }
        [Display(Name = "Show Footer")]
        public bool ShowFooter { get; set; }
        public string Tags { get; set; }
    }
}
