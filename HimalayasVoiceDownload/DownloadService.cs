using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using UcpaasVoiceDownload;

namespace HimalayasVoiceDownload
{
    public class DownloadService
    {
        public HtmlParseService HtmlParseService
        {
            get
            {
                return new HtmlParseService();
            }
        }

        public void DownloadAlbum(string url)
        {
            var albumHtml = DownloadHtml(url);
            var albumName = HtmlParseService.GetAlbumTitle(albumHtml);

            var downloadPath = ConfigurationManager.AppSettings["DownloadPath"];
            var folder = Path.Combine(downloadPath, albumName);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            Console.WriteLine($"开始下载专辑-{url}");
            int totalPage = HtmlParseService.GetAlbumTotalPage(albumHtml);
            for (int i = 1; i <= totalPage; i++)
            {
                var sounds = HtmlParseService.GetAlbumSounds(DownloadHtml(url + "?page=" + i));
                foreach (var sound in sounds)
                {
                    Console.WriteLine($"开始下载声音-{sound.Name}");
                    DownloadSound(sound, folder);
                }
            }
        }

        private void DownloadSound(Sound sound, string folder)
        {
            var soundJson = DownloadHtml(GetSoundInfoUrl(sound.Id));
            var soundInfo = JsonConvert.DeserializeObject<SoundInfo>(soundJson);
            var filePath = Path.Combine(folder, sound.Name + ".mp3");
            DownloadFile(soundInfo.DownloadUrl, filePath);
        }

        private string GetSoundInfoUrl(string soundId)
        {
            return $"http://www.ximalaya.com/tracks/{soundId}.json";
        }

        private string DownloadHtml(string url)
        {
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                return client.DownloadString(url);
            }
        }

        private void DownloadFile(string url, string filePath)
        {
            if (File.Exists(filePath))
                return;
            using (var client = new WebClient())
            {
                client.DownloadFile(url, filePath);
            }
        }

        private class SoundInfo
        {
            [JsonProperty("play_path_64")]
            public string DownloadUrl { get; set; }
        }
    }
}
