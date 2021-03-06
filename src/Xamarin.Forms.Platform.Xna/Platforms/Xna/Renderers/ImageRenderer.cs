﻿[assembly: Xamarin.Forms.Platforms.Xna.ExportRenderer(
    typeof(Xamarin.Forms.Image),
    typeof(Xamarin.Forms.Platforms.Xna.Renderers.ImageRenderer))]
namespace Xamarin.Forms.Platforms.Xna.Renderers
{
    using System;
    using System.Threading;
    using Controls;
    using Microsoft.Xna.Framework;

    public class ImageRenderer : VisualElementRenderer<Image>
    {
        CancellationTokenSource _imageLoadCancellation;
        IControl _imageSource;

        public ImageRenderer()
        {
            PropertyTracker.AddHandler(Image.SourceProperty, Handle_Source);
        }

        public override SizeRequest Measure(Size availableSize)
        {
            if (_imageSource == null || !IsVisible)
                return default(SizeRequest);

            SizeRequest measuredSize = _imageSource.Measure(VisualState, availableSize, default(SizeRequest));

            if (double.IsPositiveInfinity(availableSize.Width))
                availableSize.Width = measuredSize.Request.Width;
            if (double.IsPositiveInfinity(availableSize.Height))
                availableSize.Height = measuredSize.Request.Height;

            switch (Model.Aspect)
            {
                case Aspect.Fill:
                    return new SizeRequest(availableSize, measuredSize.Minimum);
                case Aspect.AspectFit:
                    var scaleFit = Math.Min(availableSize.Width / (float)measuredSize.Request.Width, availableSize.Height / (float)measuredSize.Request.Height);
                    return new SizeRequest(new Size(measuredSize.Request.Width * scaleFit, measuredSize.Request.Height * scaleFit), measuredSize.Minimum);
                case Aspect.AspectFill:
                    var scaleFill = Math.Max(availableSize.Width / (float)measuredSize.Request.Width, availableSize.Height / (float)measuredSize.Request.Height);
                    return new SizeRequest(new Size(measuredSize.Request.Width * scaleFill, measuredSize.Request.Height * scaleFill), measuredSize.Minimum);
            }

            throw new NotImplementedException();
        }

        protected override void LocalDraw(GameTime gameTime, Rectangle area)
        {
            if (_imageSource == null)
                return;
            _imageSource.Draw(VisualState, SpriteBatch, area, Color.White);
        }

        #region Property Handlers
        async void Handle_Source(BindableProperty prop)
        {
            if (_imageLoadCancellation != null)
                _imageLoadCancellation.Cancel();
            _imageLoadCancellation = new CancellationTokenSource();

            Model.SetValue(Image.IsLoadingPropertyKey, true);
            try
            {
                _imageSource = await Model.Source.LoadAsync(_imageLoadCancellation.Token);
                _imageLoadCancellation = null;
                InvalidateMeasure();
            }
            catch { }
            Model.SetValue(Image.IsLoadingPropertyKey, false);
        }
        #endregion
    }
}
