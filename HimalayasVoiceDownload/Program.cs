using System.Configuration;

namespace HimalayasVoiceDownload
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
