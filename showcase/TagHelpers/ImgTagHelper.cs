using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using showcase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace showcase.TagHelpers
{
    [HtmlTargetElement("img", Attributes = "asp-for")]
    public class ImgTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public Image For { get; set; }

        public IUrlHelper Url { get; }

        public ImgTagHelper(IUrlHelper urlHelper)
        {
            Url = urlHelper;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            
            if (For != null)
            {
                output.Attributes.Add("src", Url.Content(For.Path));
                output.Attributes.Add("alt", For.AltText);
                output.Attributes.Add("title", For.AltText);
            }
            else
            {
                output.SuppressOutput();
            }
        }
    }
}
