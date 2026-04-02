using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Avalonia.Platform;

namespace Avalonia.Android.Platform.SkiaPlatform;

internal abstract class InvalidationAwareTextureView : TextureView, TextureView.ISurfaceTextureListener,
    IAvaloniaRenderView
{
    private bool _isSurfaceValid;
    private Surface? _surface;

    public event EventHandler? SurfaceWindowCreated;

    View IAvaloniaRenderView.View
    {
        get => this;
    }

    IntPtr IPlatformHandle.Handle
    {
        get => _isSurfaceValid && _surface?.Handle is { } handle ?
            AndroidFramebuffer.ANativeWindow_fromSurface(JNIEnv.Handle, handle) :
            default;
    }

    public InvalidationAwareTextureView(Context context) : base(context)
    {
        SurfaceTextureListener = this;
        SetOpaque(false);
    }

    internal new void Dispose()
    {
        _surface?.Dispose();
        _surface = null;
    }

    public void OnSurfaceTextureAvailable(SurfaceTexture surfaceTexture, int width, int height)
    {
        _surface = new Surface(surfaceTexture);
        _isSurfaceValid = true;
        Log.Info("AVALONIA", "Surface Created");
        SurfaceWindowCreated?.Invoke(this, EventArgs.Empty);
        Draw();
    }

    public virtual void OnSurfaceTextureSizeChanged(SurfaceTexture surfaceTexture, int width, int height)
    {
        _isSurfaceValid = true;
        Log.Info("AVALONIA", "Surface Changed");
        Draw();
    }

    public bool OnSurfaceTextureDestroyed(SurfaceTexture surfaceTexture)
    {
        _isSurfaceValid = false;
        _surface?.Dispose();
        _surface = null;
        Log.Info("AVALONIA", "Surface Destroyed");
        return true;
    }

    public virtual void OnSurfaceTextureUpdated(SurfaceTexture surfaceTexture)
    {
        // No action needed — called after each frame is drawn to the texture.
    }

    protected abstract void Draw();

    public string HandleDescriptor
    {
        get => "TextureView";
    }

    public PixelSize Size
    {
        get => new(Width > 0 ? Width : 1, Height > 0 ? Height : 1);
    }

    public double Scaling
    {
        get => Resources?.DisplayMetrics?.Density ?? 1;
    }
}