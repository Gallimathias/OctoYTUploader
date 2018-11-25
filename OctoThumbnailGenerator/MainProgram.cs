using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using YTUploader.Core;

namespace OctoThumbnailGenerator
{
    public class MainProgram
    {
        public static void Main(string[] args)
        {
            var generator = new ThumbnailGenerator();
            Console.Write("Folgennummer von: ");
            var fromNumberInclusive = int.Parse(Console.ReadLine());
            Console.Write("Folgennummer bis: ");
            var toNumberInclusive = int.Parse(Console.ReadLine());
            if (!Directory.Exists("Fertige Thumbnails"))
                Directory.CreateDirectory("Fertige Thumbnails");
            Directory.SetCurrentDirectory("Fertige Thumbnails");

            Console.ForegroundColor = ConsoleColor.White;
            for (; fromNumberInclusive <= toNumberInclusive; fromNumberInclusive++)
            {
                using (var newBmp = generator.GenerateThumbnail(fromNumberInclusive.ToString()))
                {
                    newBmp.Save($"octoawesome{fromNumberInclusive}.png");
                    Console.WriteLine($"octoawesome{fromNumberInclusive}.png saved");
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Finished. Press any key to close :)");
            Console.Read();
        }
    }
}
