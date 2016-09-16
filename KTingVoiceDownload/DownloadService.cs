using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace KTingVoiceDownload
{
    public class DownloadService
    {
        public HtmlParseService HtmlParseService { get; } = new HtmlParseService();

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

            Console.WriteLine($"开始下载专辑-{albumName}");
            var sounds = HtmlParseService.GetAlbumSounds(DownloadHtml(url));
            var threadPool = new ThreadPool<Sound>(sounds.Skip(687));
            threadPool.OnProcessData += sound =>
            {
                Console.WriteLine($"开始下载声音-{sound.Name}");
                DownloadSound(sound, folder);
            };
            threadPool.MaxThreadCount = 8;
            threadPool.Start(false);
        }

        private void DownloadSound(Sound sound, string folder)
        {
            var soundJson = DownloadHtml(GetSoundInfoUrl(sound.Id));
            var soundInfo = JsonConvert.DeserializeObject<SoundInfo>(soundJson);
            var filePath = Path.Combine(folder, sound.Name + ".mp3");
            DownloadFile(soundInfo.Data.DownloadUrl, filePath);
        }

        private string GetSoundInfoUrl(string soundId)
        {
            return $"http://www.kting.cn/play/type/checkPlay/id/17938/aid/{soundId}/isNotAuto/1/uid/0/t/0.4557467084378004?ver=1474002429437";
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

        public class SoundInfo
        {
            public Data Data { get; set; }
        }

        public class Data
        {
            [JsonProperty("audio")]
            public string DownloadUrl { get; set; }
        }
    }
}
