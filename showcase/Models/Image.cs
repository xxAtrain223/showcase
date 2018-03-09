using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace showcase.Models
{
    public class Image
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Required]
        public string Path { get; set; }

        public string AltText { get; set; }

        public HtmlString Display(Object htmlAttributes)
        {
            TagBuilder img = new TagBuilder("img");
            
            img.MergeAttribute("src", Path);
            img.MergeAttribute("alt", AltText ?? "");
            img.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            return new HtmlString(img.ToString());
        }
    }
}
