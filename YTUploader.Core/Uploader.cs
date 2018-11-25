using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YTUploader.Core
{
    public class Uploader
    {
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        public event EventHandler<ResponseReceivedEventArgs> ResponseReceived;
        private readonly FileInfo[] fileInfos;

        public Uploader(params string[] fileNames)
        {
            fileInfos = fileNames.Select(f => new FileInfo(f)).ToArray();
        }

        public async Task Run()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "Noob Dev Gedöns",
                    CancellationToken.None
                );
            }

            var youTubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
            });

            var description = File.ReadAllText("StandardDescription.txt");

            foreach (var file in fileInfos)
            {
                var video = new Video
                {
                    Snippet = new VideoSnippet
                    {
                        Title = file.Name.Replace(file.Extension, "").TrimEnd('.'),
                        Description = description,
                        Tags = new string[] { "Livecoding", "Programmieren", "Develop", "Developing", "programming", "programm", ".NET", "CodeTalk", "OctoAwesome", "Teaching", "Tutorial", "Java", "Minecraft", "Multiplayer", "Game", "Gamedevelopment", "Spiel", "Spieleprogrammierung", "engine", "kommunikation", "Spaß", "Community", "OpenSource", "C#", "CSharp" },
                        CategoryId = "28" // See https://developers.google.com/youtube/v3/docs/videoCategories/list
                    },
                    Status = new VideoStatus
                    {
                        PrivacyStatus = PrivacyStatus.Private
                    },
                };

                using (var fileStream = file.OpenRead())
                {
                    var videosInsertRequest = youTubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                    videosInsertRequest.ProgressChanged += (p) => OnProgressChanged(p, video.Snippet.Title, file.Name);
                    videosInsertRequest.ResponseReceived += (v) => OnResponseReceived(v, file.Name, youTubeService);

                    await videosInsertRequest.UploadAsync();
                }
            }


        }

        private void OnProgressChanged(IUploadProgress progress, string title, string fileName)
        {
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(progress, title, fileName));
        }

        private void OnResponseReceived(Video video, string fileName, YouTubeService youTubeService)
        {
            ResponseReceived?.Invoke(this, new ResponseReceivedEventArgs(video, fileName, youTubeService));
        }

        public class ProgressChangedEventArgs : EventArgs
        {
            public IUploadProgress UploadProgress { get; }
            public string Title { get; }
            public string FileName { get; }

            public ProgressChangedEventArgs(IUploadProgress progress, string title, string fileName)
            {
                UploadProgress = progress;
                Title = title;
                FileName = fileName;
            }
        }

        public class ResponseReceivedEventArgs : EventArgs
        {
            public Video Video { get; }
            public string FileName { get; }
            public YouTubeService YouTubeService { get; }

            public ResponseReceivedEventArgs(Video video, string fileName, YouTubeService youTubeService)
            {
                Video = video;
                FileName = fileName;
                YouTubeService = youTubeService;
            }
        }
    }
}
