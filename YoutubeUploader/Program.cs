using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using OctoThumbnailGenerator;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YTUploader.Core;

namespace YoutubeUploader
{
    internal class Program
    {
        private static readonly ConcurrentDictionary<string, (int consoleTop, long fileSize)> consolePositions = new ConcurrentDictionary<string, (int, long)>();
        private static ThumbnailGenerator generator;

        [STAThread]
        private static void Main(string[] args)
        {
            generator = new ThumbnailGenerator();

            Console.WriteLine("YouTube Data API: Upload Video");
            Console.WriteLine("==============================");

            try
            {

                var fd = new OpenFileDialog
                {
                    Multiselect = true,
                    Filter = "Video files|*.mp4"
                };
                if (fd.ShowDialog() == DialogResult.OK)
                {

                    var uploader = new Uploader(fd.FileNames);
                    uploader.ProgressChanged += OnProgressChanged;

                    foreach (var item in fd.FileNames)
                    {
                        var fileSize = new FileInfo(item).Length;
                        consolePositions.TryAdd(item, (Console.CursorTop, fileSize / 1024 / 1024));
                        Console.WriteLine(item.Substring(item.LastIndexOf("\\") + 1,
                            item.Length - item.LastIndexOf("\\") - 1 - 4) + $": 0 MB of {fileSize / 1024 / 1024} MB sent.");
                    }

                    uploader.Run().Wait();
                }

            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void OnProgressChanged(object sender, Uploader.ProgressChangedEventArgs args)
        {
            Console.CursorTop = consolePositions[args.FileName].consoleTop;
            switch (args.UploadProgress.Status)
            {
                case UploadStatus.Uploading:
                    Console.WriteLine(args.Title + $": {args.UploadProgress.BytesSent / 1024 / 1024} MB of {consolePositions[args.FileName].fileSize} MB sent.");
                    break;

                case UploadStatus.Failed:
                    Console.WriteLine("An error prevented the upload from completing.\n{0}", args.UploadProgress.Exception);
                    break;
            }
        }

        private static void OnResponseReceived(object sender, Uploader.ResponseReceivedEventArgs args)
        {
            var smu = new ThumbnailsResource(args.YouTubeService);
            Console.CursorTop = consolePositions[args.FileName].consoleTop;
            Console.WriteLine("{0} '{1}' was successfully uploaded.".PadRight(Console.WindowWidth, ' '),
                args.Video.Snippet.Title, args.Video.Id);
            args.Video.ProcessingDetails = new VideoProcessingDetails();

            using (var stream = new MemoryStream())
            {
                var image = generator.GenerateThumbnail(args.FileName.Substring(args.FileName.LastIndexOf("[") + 4,
                    args.FileName.LastIndexOf("]") - args.FileName.LastIndexOf("[") - 4));

                image.Save(stream, image.RawFormat);
                smu.Set(args.Video.Id, stream, "image/png").Upload();
            }
        }
    }
}
