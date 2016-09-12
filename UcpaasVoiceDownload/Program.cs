namespace UcpaasVoiceDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            var downloadSevice = new DownloadService();
            downloadSevice.Start("http://www.ucpaas.com/app/lyList?appSid=8huvAbMaCR6XB7wqfzl5nWwKBwkmq3O2jUhC%2B%2BbiwTZ0I1YMHYDWEA%3D%3D", "http://www.ucpaas.com/app/lyDetail");
        }
    }
}
