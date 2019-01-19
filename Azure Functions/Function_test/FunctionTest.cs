using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Function_test
{
    public static class FunctionTest
    {
        [FunctionName("FunctionTest")]
        public static void Run(
            [TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
            TraceWriter log,
            IBinder binder
            )
        {
            const int maxImageCount = 10;
            int actualImage = 0;
            /**
             * Let's parse the HTML
             */
            string url = "https://www.deviantart.com/";
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument doc = htmlWeb.Load(url);
            List<string> imgSrcs = new List<string>();
            foreach (HtmlNode htmlNode in doc.DocumentNode.SelectNodes("//img")) //now get img.src
            {
                if (actualImage >= maxImageCount)
                {
                    break;
                }

                string imgSrc = htmlNode.GetAttributeValue("src", string.Empty);
                imgSrcs.Add(imgSrc);
                log.Info($"Got image src {imgSrc}");
                actualImage++;
                Thread.Sleep(100);
            }
            actualImage = 0;
            if (imgSrcs.Count > 0) //if we have something, download the image
            {
                foreach (string src in imgSrcs)
                {
                    if (src != string.Empty)
                    {
                        log.Info($"Downloading image {src} to memory...");
                        WebClient client = new WebClient();
                        Stream stream = client.OpenRead(src);
                        if (stream != null)
                        {
                            log.Info("Got the image!");
                            string outputFileName = $"image_{actualImage}.jpg";
                            using (var outputStream =
                                binder.Bind<Stream>(new BlobAttribute($"images/{outputFileName}", //we are creating a binding of our image stream to output stream
                                    FileAccess.Write)))
                            {
                                stream.CopyTo(outputStream);    //now we are copying image to the file
                            }
                        }
                        actualImage++;
                    }
                }
            }

            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

        }
    }
}
