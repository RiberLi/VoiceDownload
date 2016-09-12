using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace UcpaasVoiceDownload
{
    public class DownloadService
    {
        private readonly string m_CookieValue;
        private readonly string m_DownloadPath;

        public HtmlParseService HtmlParseService
        {
            get
            {
                return new HtmlParseService();
            }
        }

        public DownloadService()
        {
            m_CookieValue = GetCookieValue();
            m_DownloadPath = ConfigurationManager.AppSettings["DownloadPath"];
        }

        public void Start(string statsUrl, string voiceBaseUrl)
        {
            Console.WriteLine("开始解析录音列表");
            var statsHtml = DownloadHtml(statsUrl);
            var stats = HtmlParseService.GetStats(statsHtml);
            foreach (var stat in stats)
            {
                DownloadStat(stat, voiceBaseUrl);
            }
        }

        private string GetCookieValue()
        {
            using (var client = new WebClient())
            {
                var postData = Encoding.UTF8.GetBytes($"userid={ConfigurationManager.AppSettings["UserId"]}&password={ConfigurationManager.AppSettings["Password"]}");

                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                client.UploadData("http://www.ucpaas.com/user/login", "POST", postData);
                var resultValue = client.ResponseHeaders.Get("Set-Cookie");
                return resultValue.Split(';').FirstOrDefault(r => r.Contains("JSESSIONID"));
            }
        }

        private void DownloadStat(Stat stat, string voiceBaseUrl)
        {
            var folder = Path.Combine(m_DownloadPath, stat.Name);
            if (Directory.Exists(folder))
                return;
            Directory.CreateDirectory(folder);

            Console.WriteLine($"开始解析{stat.Name}的录音");
            var firstPageHtml = DownloadHtml(GetVoiceUrl(voiceBaseUrl, stat.Id, 1));
            var total = HtmlParseService.GetVoicesTotalPage(firstPageHtml);
            var statVoices = new List<Voice>();
            for (int i = 1; i <= total; i++)
            {
                var voiceUrl = GetVoiceUrl(voiceBaseUrl, stat.Id, i);
                var html = DownloadHtml(voiceUrl);
                var voices = HtmlParseService.GetVoices(html);
                statVoices.AddRange(voices);

                foreach (var voice in voices)
                {
                    DownloadVoice(voice, folder);
                    Console.WriteLine($"     下载{stat.Name}的录音的{voice.FileName}文件");
                }
            }
            ExportVoicesToExcel(stat, statVoices, folder);
            Console.WriteLine($"导出{stat.Name}的录音信息到excel");
        }

        private void ExportVoicesToExcel(Stat stat, IEnumerable<Voice> voices, string folder)
        {
            var fileName = $"{stat.Name}通话录音明细.xlsx";
            var titles = new[] { "主叫", "被叫", "录音时间", "文件名", "录音大小" };
            var file = new FileInfo(Path.Combine(folder, fileName));
            if (file.Exists)
            {
                File.Delete(file.FullName);
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                var ws = package.Workbook.Worksheets.Add("通话录音明细");
                for (var cell = 0; cell < titles.Length; cell++)
                {
                    ws.Cells[1, cell + 1].Value = titles[cell];
                }
                if (voices.Any())
                {
                    var row = 2;
                    foreach (var voice in voices)
                    {
                        ws.Cells[row, 1].Value = voice.From;
                        ws.Cells[row, 2].Value = voice.To;
                        ws.Cells[row, 3].Value = voice.Created;
                        ws.Cells[row, 4].Value = voice.FileName;
                        ws.Cells[row, 5].Value = voice.FileSize;
                        row++;
                    }
                }
                package.Save();
            }
        }

        private void DownloadVoice(Voice voice, string folder)
        {
            var voiceUrl = "http://www.ucpaas.com/" + voice.FileUrl;
            folder = Path.Combine(folder, "voices");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var filePath = Path.Combine(folder, voice.FileName);
            if (File.Exists(filePath))
                return;
            DownloadFile(voiceUrl, filePath);
        }

        private string GetVoiceUrl(string voiceBaseUrl, string statId, int pageNum)
        {
            return $"{voiceBaseUrl}?statId={statId}&page.currentPage={pageNum}";
        }

        private string DownloadHtml(string url)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("Cookie", m_CookieValue);
                client.Encoding = Encoding.UTF8;
                return client.DownloadString(url);
            }
        }

        private void DownloadFile(string url, string filePath)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("Cookie", m_CookieValue);
                client.DownloadFile(url, filePath);
            }
        }
    }
}
