namespace HimalayasVoiceDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            var downloadSevice = new DownloadService();
            downloadSevice.DownloadAlbum("http://www.ximalaya.com/4078661/album/232829");
        }
    }
}
