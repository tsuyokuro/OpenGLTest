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

namespace OpenGLTest3
{
    public partial class MainWindow : Window
    {
        //private DebugInputThread InputThread;

        GLControl glControl;

        Vector4 LightPosition;
        Color4 LightAmbient;    // 環境光
        Color4 LightColor;

        int FigShaderProgram;

        int FontShaderProgram;

        FontFaceW mFontW;

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

        public MainWindow()
        {
            InitializeComponent();

            //test();

            //return;

            glControl = new GLControl();

            glControl.Load += glControl_Load;
            glControl.Resize += glControl_Resize;
            glControl.Paint += glControl_Paint;
            glControl.MouseMove += GlControl_MouseMove;

            GLControlHost.Child = glControl;

            mFontW = new FontFaceW();
            mFontW.SetFont("C:\\Windows\\Fonts\\msgothic.ttc");
            mFontW.SetSize(11);
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
            SetupFontShader();

            SetupTestTexture2();
            //SetupTestTexture3();
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

        private void SetupTestTexture2()
        {
            FontTex tex = null;

            //tex = mFontW.CreateTexture("agjA,黒123");
            tex = mFontW.CreateTexture("123.123");

            TestTextureW = tex.ImgW;
            TestTextureH = tex.ImgH;

            TestTexture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, TestTexture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                PixelInternalFormat.Alpha8,
                tex.W, tex.H, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Alpha,
                PixelType.UnsignedByte, tex.Data);
        }

        //glControlの描画時に実行される。
        private void glControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
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


            //GL.Enable(EnableCap.Texture2D);

            GL.UseProgram(FontShaderProgram);

            GL.TexCoord2(1.0, 1.0);

            int texLoc = GL.GetUniformLocation(FontShaderProgram, "tex");

            GL.Uniform1(texLoc, 0);

            float w = TestTextureW/4;
            float h = TestTextureH/4;
            float z = 1f;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Color4(System.Drawing.Color.YellowGreen);

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

            GL.UseProgram(0);

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
