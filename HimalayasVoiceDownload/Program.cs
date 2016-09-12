namespace HimalayasVoiceDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            var downloadSevice = new DownloadService();
            downloadSevice.DownloadAlbum(new Album
            {
                Name = "百炼成仙",
                Url = "http://www.ximalaya.com/21783389/album/2879299"
            });
        }
    }
}
