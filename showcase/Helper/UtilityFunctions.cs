using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace showcase.UtilityFunctions
{
    public class ShowcaseUtilities
    {
        public static string SanitizeHtml(string v)
        {
            // Doesn't handle malformed xml
            // http://html-agility-pack.net/

            HtmlDocument doc = new HtmlDocument();
            HtmlNode root = doc.CreateElement("root");

            root.InnerHtml = v;

            HtmlNodeCollection nodes;
            HtmlNode node;

            // Select all script elements
            while ((node = root.SelectSingleNode(@".//script")) != null)
            {
                HtmlEscapeElement(node);

            }

            // Select elements whose attribute values start with javascript:
            while ((node = root.SelectSingleNode(@".//*[@*[starts-with(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript:')]]")) != null)
            {
                HtmlEscapeElement(node);
            }

            // Select elements that has attributes that start with "on"
            // Does not work
            // nodes = root.SelectNodes(@"//*[@*[starts-with(name(), 'on')]]");

            // This is a less elegant way of doing the same thing.
            nodes = root.SelectNodes(@".//*[@*]");
            for (int i = 0; nodes != null && i < nodes.Count; i++)
            {
                foreach (HtmlAttribute attr in nodes[i].Attributes)
                {
                    if (Regex.IsMatch(attr.Name, "^on"))
                    {
                        HtmlEscapeElement(nodes[i]);
                        nodes = root.SelectNodes(@".//*[@*]");
                        i--;
                        break;
                    }
                }
            }

            return root.InnerHtml;
        }

        private static void HtmlEscapeElement(HtmlNode element)
        {
            HtmlDocument doc = element.OwnerDocument;
            HtmlNode parent = element.ParentNode;

            parent.InsertAfter(doc.CreateTextNode(WebUtility.HtmlEncode(element.OuterHtml)), element);
            parent.RemoveChild(element);
        }

        public static void SaveStreamToFile(Stream stream, string filepath)
        {
            using (var fileSystem = File.Create(filepath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileSystem);
            }
        }

        public static async Task SaveStreamToFileAsync(Stream stream, string filepath)
        {
            using (var fileSystem = File.Create(filepath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(fileSystem);
            }
        }

        public static bool IsImage(IFormFile postedFile)
        {
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                postedFile.ContentType.ToLower() != "image/jpeg" &&
                postedFile.ContentType.ToLower() != "image/pjpeg" &&
                postedFile.ContentType.ToLower() != "image/gif" &&
                postedFile.ContentType.ToLower() != "image/x-png" &&
                postedFile.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".png"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".gif"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg")
            {
                return false;
            }

            Stream stream = postedFile.OpenReadStream();

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try
            {
                if (!stream.CanRead)
                {
                    return false;
                }

                if (postedFile.Length < 512)
                {
                    return false;
                }

                byte[] buffer = new byte[512];
                stream.Read(buffer, 0, 512);
                string content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            stream.Position = 0;

            return true;
        }
    }
}
