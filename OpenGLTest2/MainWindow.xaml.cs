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
using FTGL;
using System.Windows.Resources;
using System.IO;

namespace OpenGLTest2
{
    public partial class MainWindow : Window
    {
        private DebugInputThread InputThread;

        GLControl glControl;

        Vector4 LightPosition;
        Color4 LightAmbient;    // 環境光
        Color4 LightColor;

        int ShaderProgram;

        FontWrapper FontW;

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

            //FontW = FontWrapper.LoadFile("F:\\vsprj\\OpenGLTest2\\OpenGLTest2\\Fonts\\togoshi-gothic.ttf");

            FontW = FontWrapper.LoadFile("C:\\Windows\\Fonts\\msgothic.ttc");
            //FontW = FontWrapper.LoadFile("F:\\vsprj\\OpenGLTest2\\OpenGLTest2\\Fonts\\SmartFont.otf");

            FontW.FontSize = 20;
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

            //FontW.SetCharMap(FTEncord.UNICODE);

            FontW.RenderW(s, RenderMode.All);

            GL.Uniform1(GL.GetUniformLocation(ShaderProgram, "texture"), 1);

            GL.PopMatrix();
        }

        private void SetupGL()
        {
            GL.ClearColor(Color4.Black);
            //GL.Enable(EnableCap.DepthTest);

            SetupLight();
            SetupShader();
        }

        private void SetupShader()
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


            ShaderProgram = GL.CreateProgram();

            //各シェーダオブジェクトをシェーダプログラムへ登録
            GL.AttachShader(ShaderProgram, vertexShader);
            GL.AttachShader(ShaderProgram, fragmentShader);

            //不要になった各シェーダオブジェクトを削除
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            //シェーダプログラムのリンク
            GL.LinkProgram(ShaderProgram);

            GL.GetProgram(ShaderProgram, GetProgramParameterName.LinkStatus, out status);
            //シェーダプログラムのリンクのチェック
            if (status == 0)
            {
                throw new ApplicationException(GL.GetProgramInfoLog(ShaderProgram));
            }

            GL.UseProgram(ShaderProgram);
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
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            Vector3 eye = Vector3.Zero;
            eye.X = 40f;
            eye.Y = 40f;
            eye.Z = 40f;
            Matrix4 modelview = Matrix4.LookAt(eye, Vector3.Zero, Vector3.UnitY);
            GL.LoadMatrix(ref modelview);

            GL.Light(LightName.Light0, LightParameter.Position, LightPosition);

            SetupLight();

            SetMaterial(new Color4(0.6f, 0.1f, 0.1f, 1.0f));

            float w = 10.0f;
            float z = 0.0f;

            GL.Normal3(new Vector3d(0, 0, 1));

            GL.Begin(PrimitiveType.Quads);
                       
            GL.Vertex3(-w, -w, z);
            GL.Vertex3(w, -w, z);
            GL.Vertex3(w, w, z);
            GL.Vertex3(-w, w, z);

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
