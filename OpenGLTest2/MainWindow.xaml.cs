using System;
using System.Collections.Generic;
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

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using DebugUtil;
using System.Drawing;
using System.Windows.Resources;
using System.IO;
using FontUtil;
using System.Drawing.Imaging;

namespace OpenGLTest2
{
    public partial class MainWindow : Window
    {
        private DebugInputThread InputThread;

        GLControl glControl;

        Vector4 LightPosition;
        Color4 LightAmbient;    // 環境光
        Color4 LightColor;

        int FigShaderProgram;

        int FontShaderProgram;
        FontService mFontService;

        public MainWindow()
        {
            InitializeComponent();

            InputThread = new DebugInputThread(debugCommand);
            InputThread.start();

            glControl = new GLControl();

            glControl.Load += glControl_Load;
            glControl.Resize += glControl_Resize;
            glControl.Paint += glControl_Paint;
            glControl.MouseMove += GlControl_MouseMove;

            GLControlHost.Child = glControl;

            mFontService = new FontService();

            mFontService.SetFont("C:\\Windows\\Fonts\\msgothic.ttc");
            mFontService.SetSize(24);

            Bitmap image = mFontService.RenderString("This is テスト", System.Drawing.Color.White, System.Drawing.Color.Blue);

            image.Save("F:\\work2\\test.bmp");

            image = mFontService.RenderString("a", System.Drawing.Color.White, System.Drawing.Color.Blue);

            image.Save("F:\\work2\\test2.bmp");
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

        private void DrawText()
        {
            GL.MatrixMode(MatrixMode.Modelview);

            GL.PushMatrix();

            GL.Translate(0, 0, 0);

            GL.Scale(0.2, 0.2, 0.2);

            GL.Color4(Color4.White);

            string s = "あtest黒木";


            GL.Uniform1(GL.GetUniformLocation(FigShaderProgram, "texture"), 1);

            GL.PopMatrix();
        }

        private void SetupGL()
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);

            SetupLight();
            SetupFigShader();
            SetupFontShader();

            SetupTestTexture();
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

        private void SetupFontShader()
        {
            string vertexSrc = ReadResourceText("/Shader/font_vertex.shader");
            string fragmentSrc = ReadResourceText("/Shader/font_fragment.shader");

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

            FontShaderProgram = GL.CreateProgram();

            //各シェーダオブジェクトをシェーダプログラムへ登録
            GL.AttachShader(FontShaderProgram, vertexShader);
            GL.AttachShader(FontShaderProgram, fragmentShader);

            //不要になった各シェーダオブジェクトを削除
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            //シェーダプログラムのリンク
            GL.LinkProgram(FontShaderProgram);

            GL.GetProgram(FontShaderProgram, GetProgramParameterName.LinkStatus, out status);
            //シェーダプログラムのリンクのチェック
            if (status == 0)
            {
                throw new ApplicationException(GL.GetProgramInfoLog(FontShaderProgram));
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

        int TestTexture;
        int TestTextureW;
        int TestTextureH;

        private void SetupTestTexture()
        {
            Bitmap image = mFontService.RenderString("This is テスト", System.Drawing.Color.White, System.Drawing.Color.Transparent);

            image.RotateFlip(RotateFlipType.RotateNoneFlipY);

            TestTextureW = image.Width;
            TestTextureH = image.Height;

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);

            BitmapData bd = image.LockBits(rect,
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);


            TestTexture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, TestTexture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgba,
                bd.Width, bd.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, bd.Scan0);

            image.UnlockBits(bd);

            image.Dispose();
        }

        //glControlの描画時に実行される。
        private void glControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            Vector3 eye = Vector3.Zero;
            eye.X = 50f;
            eye.Y = 50f;
            eye.Z = 200f;
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

            //GL.Enable(EnableCap.Texture2D);
            //GL.UseProgram(0);

            GL.UseProgram(FontShaderProgram);

            GL.TexCoord2(1.0, 1.0);

            int texLoc = GL.GetUniformLocation(FontShaderProgram, "tex");

            GL.Uniform1(texLoc, 0);

            float w = TestTextureW/4;
            float h = TestTextureH/4;
            float z = 1f;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Normal3(new Vector3d(0, 0, 1));

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(1.0, 1.0);
            GL.Vertex3(w / 2, h, z);

            GL.TexCoord2(0.0, 1.0);
            GL.Vertex3(-w / 2, h, z);

            GL.TexCoord2(0.0, 0.0);
            GL.Vertex3(-w / 2, 0, z);

            GL.TexCoord2(1.0, 0.0);
            GL.Vertex3(w / 2, 0, z);

            GL.End();

            glControl.SwapBuffers();
        }

        private void debugCommand(String s)
        {
            if (s == "test1")
            {
            }
            else if (s == "test2")
            {
            }
            else if (s == "test3")
            {
            }
        }
    }
}
