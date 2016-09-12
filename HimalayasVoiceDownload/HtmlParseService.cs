using HtmlAgilityPack;
using ScrapySharp.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace HimalayasVoiceDownload
{
    public class HtmlParseService
    {
        public ICollection<Sound> GetAlbumSounds(string html)
        {
            var sounds = new List<Sound>();
            var doc = GetHtmlNode(html);
            var soundNodes = doc.CssSelect(".album_soundlist ul li");
            foreach (var soundNode in soundNodes)
            {
                var titleNode = soundNode.CssSelect(".title").First();
                sounds.Add(new Sound
                {
                    Id = soundNode.Attributes["sound_id"].Value,
                    Name = titleNode.Attributes["title"].Value,
                    Href = titleNode.Attributes["href"].Value
                });
            }
            return sounds;
        }

        public int GetAlbumTotalPage(string html)
        {
            var doc = GetHtmlNode(html);
            var pageNodes = doc.CssSelect(".pagingBar_wrapper a");
            return pageNodes.Select(p => int.Parse(p.Attributes["data-page"]?.Value ?? "0")).Max();
        }

        public string GetAlbumTitle(string html)
        {
            var doc = GetHtmlNode(html);
            return doc.CssSelect(".detailContent_title h1").First().InnerText;
        }

        private HtmlNode GetHtmlNode(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            return htmlDocument.DocumentNode;
        }
    }
}
