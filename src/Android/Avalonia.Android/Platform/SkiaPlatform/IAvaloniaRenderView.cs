using System;
using Android.Views;
using Avalonia.Platform;

namespace Avalonia.Android.Platform.SkiaPlatform
{
    internal interface IAvaloniaRenderView : INativePlatformHandleSurface, IDisposable
    {
        View View { get; }
        event EventHandler? SurfaceWindowCreated;
    }
}
