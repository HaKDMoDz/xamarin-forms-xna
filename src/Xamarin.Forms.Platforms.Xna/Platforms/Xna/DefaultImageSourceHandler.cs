﻿[assembly: Xamarin.Forms.Platforms.Xna.ExportImageSourceHandler(
    typeof(Xamarin.Forms.StreamImageSource),
    typeof(Xamarin.Forms.Platforms.Xna.DefaultImageSourceHandler))]
[assembly: Xamarin.Forms.Platforms.Xna.ExportImageSourceHandler(
    typeof(Xamarin.Forms.UriImageSource),
    typeof(Xamarin.Forms.Platforms.Xna.DefaultImageSourceHandler))]
#if !PORTABLE
[assembly: Xamarin.Forms.Platforms.Xna.ExportImageSourceHandler(
    typeof(Xamarin.Forms.FileImageSource),
    typeof(Xamarin.Forms.Platforms.Xna.DefaultImageSourceHandler))]
#endif
namespace Xamarin.Forms.Platforms.Xna
{
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Xamarin.Forms;

    public class DefaultImageSourceHandler : IImageSourceHandler
    {
        public async Task<Texture2D> LoadImageAsync(ImageSource imageSource, CancellationToken cancellationToken)
        {
            Task<Stream> getStream = null;

            var streamSource = imageSource as StreamImageSource;
            var uriSource = imageSource as UriImageSource;

            var fileSource = imageSource as FileImageSource;
            if (fileSource != null)
            {
#if !PORTABLE
                if (File.Exists(fileSource.File))
                    getStream = Task.FromResult((Stream)System.IO.File.OpenRead(fileSource.File));
                else
#endif
                {
                    return Forms.Game.Content.Load<Texture2D>(fileSource.File);
                }
            }
            if (streamSource != null)
                getStream = streamSource.Stream(cancellationToken);
            else if (uriSource != null)
            {
                var uri = uriSource.Uri;
                if (uri.Scheme == "content")
                {
                    if (uri.Host != string.Empty && uri.Host != "localhost")
                        throw new ArgumentException("Unsupported image source HOST " + uri.Host + ". Did you mean content:///?", "imageSource");

                    var asset = uriSource.Uri.PathAndQuery.TrimStart('/');
                    return Forms.Game.Content.Load<Texture2D>(asset);
                }

                getStream = uriSource.GetStreamAsync(cancellationToken);
            }

            if (getStream == null)
                throw new InvalidOperationException("Not supported image source");

            return Texture2D.FromStream(Forms.Game.GraphicsDevice, await getStream);
        }
    }
}
