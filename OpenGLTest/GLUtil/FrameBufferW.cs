﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace GLUtil
{
    class FrameBufferW
    {
        int Width;
        int Height;

        int FrameBufferDesc;
        int DepthRenderBufferDesc;
        int ColorTexDesc;
        //int DepthTexDesc;

        public Bitmap GetBitmap()
        {
            Bitmap bmp = new Bitmap(Width, Height);
            BitmapData bmpData
                = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                ImageLockMode.WriteOnly,
                                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.ReadBuffer(ReadBufferMode.Front);

            GL.ReadPixels(
                0, 0,
                Width, Height,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,
                bmpData.Scan0);

            bmp.UnlockBits(bmpData);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bmp;
        }

        public void Start()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferDesc);
            GL.Viewport(0, 0, Width, Height);
        }

        public void End()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        }

        public void Dispose()
        {
            GL.DeleteTexture(ColorTexDesc);
            //GL.DeleteTexture(DepthTexDesc);
            GL.DeleteRenderbuffer(DepthRenderBufferDesc);
            GL.DeleteFramebuffer(FrameBufferDesc);
        }

        public void Create(int width, int height)
        {
            Width = width;
            Height = height;

            // Color Texture
            ColorTexDesc = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ColorTexDesc);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                width, height,
                0,
                OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
                PixelType.UnsignedByte,
                IntPtr.Zero);


            // Depth Texture
            //DepthTexDesc = GL.GenTexture();
            //GL.BindTexture(TextureTarget.Texture2D, DepthTexDesc);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //GL.TexImage2D(
            //    TextureTarget.Texture2D,
            //    0,
            //    (PixelInternalFormat)All.DepthComponent32,
            //    width, height,
            //    0,
            //    OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent,
            //    PixelType.UnsignedInt,
            //    IntPtr.Zero);


            // Create FrameBuffer
            FrameBufferDesc = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferDesc);

            // Create RenderBuffer
            DepthRenderBufferDesc = GL.GenRenderbuffer();

            // フレームバッファにデプスバッファを割り当てる。
            GL.FramebufferRenderbuffer(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.Depth,
                RenderbufferTarget.Renderbuffer,
                DepthRenderBufferDesc);

            // フレームバッファにカラーバッファを割り当てる
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                ColorTexDesc, 0);

            //GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthTexDesc, 0);

            // Since setup is completed, unbind objects.
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}
