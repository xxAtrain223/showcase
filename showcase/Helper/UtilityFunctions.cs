using HtmlAgilityPack;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

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
    }
}
