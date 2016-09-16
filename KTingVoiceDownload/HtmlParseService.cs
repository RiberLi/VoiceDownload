using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using ScrapySharp.Extensions;

namespace KTingVoiceDownload
{
    public class HtmlParseService
    {
        public ICollection<Sound> GetAlbumSounds(string html)
        {
            var sounds = new List<Sound>();
            var doc = GetHtmlNode(html);
            var soundNodes = doc.CssSelect("#hiddenresult dl dd");
            foreach (var soundNode in soundNodes)
            {
                var checkNode = soundNode.CssSelect("span input").First();
                var numberNode= soundNode.CssSelect(".text1 a").First();
                var titleNode = soundNode.CssSelect(".text2 a").First();
                sounds.Add(new Sound
                {
                    Id = checkNode.Attributes["value"].Value,
                    Name = numberNode.InnerText+" "+ titleNode.Attributes["title"].Value
                });
            }
            return sounds;
        }

        public int GetAlbumTotalPage(string html)
        {
            var doc = GetHtmlNode(html);
            var pageNodes = doc.CssSelect(".Pagination a");
            return pageNodes.Select(p => int.Parse(p.InnerText)).Max();
        }

        public string GetAlbumTitle(string html)
        {
            var doc = GetHtmlNode(html);
            return doc.CssSelect(".first_con h1").First().InnerText;
        }

        private HtmlNode GetHtmlNode(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            return htmlDocument.DocumentNode;
        }
    }
}
