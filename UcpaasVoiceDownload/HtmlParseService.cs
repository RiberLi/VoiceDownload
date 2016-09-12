using HtmlAgilityPack;
using ScrapySharp.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace UcpaasVoiceDownload
{
    public class HtmlParseService
    {
        public ICollection<Stat> GetStats(string html)
        {
            var stats = new List<Stat>();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var doc = htmlDocument.DocumentNode;
            var tableNode = doc.CssSelect(".table_box_lylist table");
            foreach (var trNode in tableNode.CssSelect("tr"))
            {
                var tdElements = trNode.CssSelect("td").ToList();
                var count = tdElements.Count;
                if (count == 0)
                    continue;
                stats.Add(new Stat
                {
                    Id = tdElements[0].Attributes["id"].Value,
                    Name = tdElements[0].InnerText
                });
            }
            return stats;
        }

        public int GetVoicesTotalPage(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var doc = htmlDocument.DocumentNode;
            var total = int.Parse(doc.CssSelect(".pagenum span i").First().InnerText);
            return total % 15 == 0 ? total / 15 : total / 15 + 1;
        }

        public ICollection<Voice> GetVoices(string html)
        {
            var voices = new List<Voice>();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var doc = htmlDocument.DocumentNode;
            var tableNode = doc.CssSelect("#ly_csm_div");
            foreach (var trNode in tableNode.CssSelect("tr"))
            {
                var tdElements = trNode.CssSelect("td").ToList();
                var count = tdElements.Count;
                if (count < 7)
                    continue;
                voices.Add(new Voice
                {
                    From = tdElements[1].InnerText,
                    To = tdElements[2].InnerText,
                    Created = tdElements[3].InnerText,
                    FileName = tdElements[4].InnerText,
                    FileSize = tdElements[5].InnerText,
                    FileUrl = tdElements[6].CssSelect("a").First().Attributes["href"].Value
                });
            }
            return voices;
        }
    }
}
