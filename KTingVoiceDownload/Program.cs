using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTingVoiceDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            var downloadSevice = new DownloadService();
            downloadSevice.DownloadAlbum(ConfigurationManager.AppSettings["AlbumUrl"]);
        }
    }
}
