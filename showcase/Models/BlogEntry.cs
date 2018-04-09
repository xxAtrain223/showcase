using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace showcase.Models
{
    public enum TitlePlacement
    {
        [Display(Name = "Card Header")]
        CardHeader,
        [Display(Name = "Above Card")]
        AboveCard,
        [Display(Name = "Jumbotron")]
        Jumbotron
    }

    public enum ImagePlacement
    {
        [Display(Name = "Card Body Right")]
        CardBodyRight,
        [Display(Name = "Card Body Left")]
        CardBodyLeft,
        [Display(Name = "Card Top")]
        CardTop,
        [Display(Name = "Jumbotron")]
        Jumbotron,
        [Display(Name = "Left Of Card")]
        LeftOfCard,
        [Display(Name = "Right Of Card")]
        RightOfCard
    }

    public class BlogEntry
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public TitlePlacement TitlePlacement { get; set; }
        public string ShortDescription { get; set; }
        public Image Image { get; set; }
        public ImagePlacement ImagePlacement {get;set;}
        public string Markdown { get; set; }
        public string Html { get; set; }
        public bool ShowOnList { get; set; }
        public bool ShowFooter { get; set; }
        public List<Tag> Tags { get; set; }
        public DateTimeOffset DateUploaded { get; set; }
    }
}
