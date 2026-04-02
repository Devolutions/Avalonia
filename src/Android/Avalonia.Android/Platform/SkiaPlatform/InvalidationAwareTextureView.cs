using System;
using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Avalonia.Platform;

namespace Avalonia.Android.Platform.SkiaPlatform
{
    internal abstract class InvalidationAwareTextureView : TextureView, TextureView.ISurfaceTextureListener, IAvaloniaRenderView
    {
        private IntPtr _nativeWindowHandle = IntPtr.Zero;
        private Surface? _surface;
        private PixelSize _size = new(1, 1);
        private double _scaling = 1;

        public event EventHandler? SurfaceWindowCreated;
        public event EventHandler? SurfaceWindowDestroyed;

        View IAvaloniaRenderView.View => this;

        public PixelSize Size => _size;
        public double Scaling => _scaling;

        IntPtr IPlatformHandle.Handle => _nativeWindowHandle;
        string IPlatformHandle.HandleDescriptor => "TextureView";

        protected InvalidationAwareTextureView(Context context) : base(context)
        {
            SurfaceTextureListener = this;
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseNativeWindowHandle();
            base.Dispose(disposing);
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surfaceTexture, int width, int height)
        {
            Log.Info("AVALONIA", $"TextureView Available. Size:{width} x {height}");
            CacheSurfaceProperties(surfaceTexture, width, height);
            SurfaceWindowCreated?.Invoke(this, EventArgs.Empty);
            OnSurfaceTextureSizeChanged(surfaceTexture, width, height);
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surfaceTexture)
        {
            Log.Info("AVALONIA", "TextureView Destroyed");
            ReleaseNativeWindowHandle();
            _size = new PixelSize(1, 1);
            SurfaceWindowDestroyed?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public virtual void OnSurfaceTextureSizeChanged(SurfaceTexture surfaceTexture, int width, int height)
        {
            Log.Info("AVALONIA", $"TextureView SizeChanged. Size:{width} x {height}");
            CacheSurfaceProperties(surfaceTexture, width, height);
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surfaceTexture)
        {
        }

        private void CacheSurfaceProperties(SurfaceTexture surfaceTexture, int width, int height)
        {
            var newSurface = new Surface(surfaceTexture);
            var newHandle = IntPtr.Zero;
            if (newSurface.Handle is { } handle)
            {
                newHandle = AndroidFramebuffer.ANativeWindow_fromSurface(JNIEnv.Handle, handle);
            }

            var oldSurface = Interlocked.Exchange(ref _surface, newSurface);
            if (Interlocked.Exchange(ref _nativeWindowHandle, newHandle) is var oldHandle
                && oldHandle != IntPtr.Zero)
            {
                AndroidFramebuffer.ANativeWindow_release(oldHandle);
            }

            oldSurface?.Dispose();

            _size = new PixelSize(width, height);
            _scaling = Resources?.DisplayMetrics?.Density ?? 1;
        }

        private void ReleaseNativeWindowHandle()
        {
            if (Interlocked.Exchange(ref _nativeWindowHandle, IntPtr.Zero) is var oldHandle
                && oldHandle != IntPtr.Zero)
            {
                AndroidFramebuffer.ANativeWindow_release(oldHandle);
            }

            Interlocked.Exchange(ref _surface, null)?.Dispose();
        }
    }
}