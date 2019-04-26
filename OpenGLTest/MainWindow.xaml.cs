using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using DebugUtil;
using System.Windows.Resources;
using System.IO;
using FontUtil;
using System.Drawing.Imaging;
using SharpFont;
using System.Collections;
using System.Diagnostics;
using GLFont;

namespace OpenGLTest3
{
    public partial class MainWindow : Window
    {
        private DebugInputThread InputThread;

        GLControl glControl;

        Vector4 LightPosition;
        Color4 LightAmbient;    // 環境光
        Color4 LightColor;

        int FigShaderProgram;

        FontFaceW mFontW;
        FontRenderer mFontRenderer;

        private void test()
        {
            FontTex ft1 = new FontTex(16, 3);
            ft1.Set(0, 0, 0xa1);
            ft1.Set(1, 0, 0xa2);
            ft1.Set(2, 0, 0xa3);
            ft1.Set(3, 0, 0xa4);
            ft1.Set(4, 0, 0xa5);

            ft1.Set(0, 1, 0xb1);
            ft1.Set(1, 1, 0xb2);
            ft1.Set(2, 1, 0xb3);
            ft1.Set(3, 1, 0xb4);
            ft1.Set(4, 1, 0xb5);

            ft1.Set(0, 2, 0xc1);
            ft1.Set(1, 2, 0xc2);
            ft1.Set(2, 2, 0xc3);
            ft1.Set(3, 2, 0xc4);
            ft1.Set(4, 2, 0xc5);

            ft1.dump();
            Console.WriteLine();

            FontTex ft2 = new FontTex(12, 10);

            ft2.Paste(0, 0, ft1);

            ft2.dump();
        }

        private void test2()
        {
            FontTex ft = mFontW.CreateTexture("agjk123");
        }

        public MainWindow()
        {
            InitializeComponent();

            InputThread = new DebugInputThread(debugCommand);
            InputThread.start();

            mFontW = new FontFaceW();
            mFontW.SetFont("C:\\Windows\\Fonts\\msgothic.ttc", 1);
            mFontW.SetSize(24);

            //test2();

            //return;

            glControl = new GLControl();

            glControl.Load += glControl_Load;
            glControl.Resize += glControl_Resize;
            glControl.Paint += glControl_Paint;
            glControl.MouseMove += GlControl_MouseMove;

            GLControlHost.Child = glControl;

            mFontRenderer = new FontRenderer();

            //mFontW.CreateTexture(' ');
            //mFontW.CreateTexture('1');
            //mFontW.CreateTexture('5');

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //for (int i=0; i<1000; i++)
            //{
            //    FontTex tex = mFontW.CreateTexture("01234567890");
            //}
            //sw.Stop();

            //Console.WriteLine($"CreateTexture time: {sw.Elapsed}");
        }

        private void GlControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            
        }

        //glControlの起動時に実行される。
        private void glControl_Load(object sender, EventArgs e)
        {
            SetupGL();
        }

        //glControlのサイズ変更時に実行される。
        private void glControl_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl.Size.Width, glControl.Size.Height);

            float aspect = (float)glControl.Size.Width / (float)glControl.Size.Height;
            float fovy = (float)Math.PI / 4.0f;

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection =
                Matrix4.CreatePerspectiveFieldOfView(
                    fovy,
                    aspect,
                    1.0f,       // near
                    6400.0f     // far
                    );

            GL.LoadMatrix(ref projection);
        }

        private void SetupGL()
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);

            SetupLight();
            SetupFigShader();

            mFontRenderer.Init();

            //SetupTestTexture2();
        }

        private void SetupFigShader()
        {
            string vertexSrc = ReadResourceText("/Shader/vertex.shader");
            string fragmentSrc = ReadResourceText("/Shader/fragment.shader");

            int status;

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSrc);
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out status);
            if (status == 0)
            {
                throw new ApplicationException(GL.GetShaderInfoLog(vertexShader));
            }

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSrc);
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status);
            if (status == 0)
            {
                throw new ApplicationException(GL.GetShaderInfoLog(fragmentShader));
            }


            FigShaderProgram = GL.CreateProgram();

            //各シェーダオブジェクトをシェーダプログラムへ登録
            GL.AttachShader(FigShaderProgram, vertexShader);
            GL.AttachShader(FigShaderProgram, fragmentShader);

            //不要になった各シェーダオブジェクトを削除
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            //シェーダプログラムのリンク
            GL.LinkProgram(FigShaderProgram);

            GL.GetProgram(FigShaderProgram, GetProgramParameterName.LinkStatus, out status);
            //シェーダプログラムのリンクのチェック
            if (status == 0)
            {
                throw new ApplicationException(GL.GetProgramInfoLog(FigShaderProgram));
            }
        }

        private string ReadResourceText(string path)
        {
            Uri fileUri = new Uri(path, UriKind.Relative);
            StreamResourceInfo info = Application.GetResourceStream(fileUri);
            StreamReader sr = new StreamReader(info.Stream);

            string s = sr.ReadToEnd();
            sr.Close();

            return s;
        }

        private void SetupLight()
        {
            LightPosition = new Vector4(-5.0f, 0.0f, 5.0f, 1.0f);
            LightColor = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            LightAmbient = new Color4(0.1f, 0.1f, 0.1f, 1.0f);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            GL.Light(LightName.Light0, LightParameter.Ambient, LightAmbient);
            GL.Light(LightName.Light0, LightParameter.Diffuse, LightColor);
            GL.Light(LightName.Light0, LightParameter.Specular, LightColor);
            GL.LightModel(LightModelParameter.LightModelLocalViewer, 1);
        }

        private void SetMaterial(Color4 color)
        {
            Color4 specular = new Color4(0.3f, 0.3f, 0.3f, 1.0f);

            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.AmbientAndDiffuse, color);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, specular);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, 100.0f);
        }

        //glControlの描画時に実行される。
        private void glControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Render();
            glControl.SwapBuffers();
        }

        private void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            Vector3 eye = Vector3.Zero;
            eye.X = 10f;
            eye.Y = 20f;
            eye.Z = 50f;
            Matrix4 modelview = Matrix4.LookAt(eye, Vector3.Zero, Vector3.UnitY);
            GL.LoadMatrix(ref modelview);

            GL.Light(LightName.Light0, LightParameter.Position, LightPosition);

            SetupLight();

            GL.UseProgram(FigShaderProgram);

            SetMaterial(new Color4(0.2f, 0.2f, 1.0f, 1.0f));

            float fw = 10.0f;
            float fz = -1.0f;

            GL.Normal3(new Vector3d(0, 0, 1));

            GL.Begin(PrimitiveType.Quads);

            GL.Vertex3(-fw, -fw, fz);
            GL.Vertex3(fw, -fw, fz);
            GL.Vertex3(fw, fw, fz);
            GL.Vertex3(-fw, fw, fz);

            GL.End();

            GL.UseProgram(0);

            fw = 5.0f;
            fz = -0.5f;

            float x = -10;
            float y = 0;

            GL.Normal3(new Vector3d(0, 0, 1));
            GL.Color4(System.Drawing.Color.Coral);

            GL.Begin(PrimitiveType.Quads);

            GL.Vertex3(-fw + x, -fw + y, fz);
            GL.Vertex3(fw + x, -fw + y, fz);
            GL.Vertex3(fw + x, fw + y, fz);
            GL.Vertex3(-fw + x, fw + y, fz);

            GL.End();

            GL.Translate(-12, 0, 0);
            GL.Rotate(30, Vector3d.UnitZ);
            FontTex tex = mFontW.CreateTexture($"agjk{glControl.Width}");
            //tex.dump_b();
            mFontRenderer.Render(tex);
        }

        private void debugCommand(String s)
        {
            if (s == "test1")
            {
                test1();
            }
            else if (s == "test2")
            {
            }
            else if (s == "test3")
            {
            }
        }

        private void test1()
        {
            int imgW = 640;
            int imgH = 640;

            float aspect = (float)imgW / (float)imgH;
            float fovy = (float)Math.PI / 4.0f;

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();

            Matrix4 projection =
                Matrix4.CreatePerspectiveFieldOfView(
                    fovy,
                    aspect,
                    1.0f,       // near
                    6400.0f     // far
                    );

            GL.LoadMatrix(ref projection);

            FrameBufferW fb = new FrameBufferW();
            fb.Create(imgW, imgH);
            fb.Start();

            GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Render();

            Bitmap bmp = fb.GetBitmap();

            fb.End();
            fb.Dispose();

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();

            bmp.Save(@"F:\work\test.bmp");
        }



        class FrameBufferW
        {
            int Width;
            int Height;

            int hFrameBuffer;
            //int RenderBufferId;
            int hColorTex;
            int hDepthTex;

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
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, hFrameBuffer);
                //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBufferId);
                GL.Viewport(0, 0, Width, Height);
            }

            public void End()
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            }

            public void Dispose()
            {
                GL.DeleteTexture(hColorTex);
                GL.DeleteTexture(hDepthTex);
                //GL.DeleteRenderbuffer(RenderBufferId);
                GL.DeleteFramebuffer(hFrameBuffer);
            }

            public void Create(int width, int height)
            {
                Width = width;
                Height = width;

                // Color Texture
                hColorTex = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, hColorTex);

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
                hDepthTex = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, hDepthTex);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    (PixelInternalFormat)All.DepthComponent32,
                    width, height,
                    0,
                    OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent,
                    PixelType.UnsignedInt,
                    IntPtr.Zero);


                // Create FrameBuffer
                hFrameBuffer = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, hFrameBuffer);

                // Create RenderBuffer
                //RenderBufferId = GL.GenRenderbuffer();
                //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBufferId);
                //GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba12, width, height);

                //フレームバッファにレンダバッファを割り当てる。
                //GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, RenderBufferId);

                // Attouch color and depth texture to FrameBuffer
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, hColorTex, 0);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, hDepthTex, 0);

                // Since setup is completed, unbind objects.
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
        }
    }
}
