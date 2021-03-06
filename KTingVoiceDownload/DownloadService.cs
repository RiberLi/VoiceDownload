﻿using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace KTingVoiceDownload
{
    public class DownloadService
    {
        private bool m_HasFailed = false;

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
            var sounds = HtmlParseService.GetAlbumSounds(albumHtml);
            var threadPool = new ThreadPool<Sound>(sounds.Skip(1345));
            threadPool.OnProcessData += sound =>
            {
                Console.WriteLine($"开始下载声音-{sound.Name}");
                DownloadSound(sound, folder);
            };
            threadPool.MaxThreadCount = 3;
            threadPool.Start(false);

            if (m_HasFailed)
            {
                DownloadAlbum(url);
            }
        }

        private void DownloadSound(Sound sound, string folder)
        {
            try
            {
                var soundJson = DownloadHtml(GetSoundInfoUrl(sound.Id));
                var soundInfo = JsonConvert.DeserializeObject<SoundInfo>(soundJson);
                var filePath = Path.Combine(folder, sound.Name + ".mp3");
                DownloadFile(soundInfo.Data.DownloadUrl, filePath);
            }
            catch (Exception)
            {
                Console.WriteLine($"!!!!!!下载声音-{sound.Name}失败");
                m_HasFailed = true;
            }
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
