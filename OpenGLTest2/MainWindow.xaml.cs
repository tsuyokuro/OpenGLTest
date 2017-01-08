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

namespace OpenGLTest2
{
    public partial class MainWindow : Window
    {
        private DebugInputThread InputThread;

        Matrix4 Projection;

        public MainWindow()
        {
            InitializeComponent();

            InputThread = new DebugInputThread(debugCommand);
            InputThread.start();
        }

        //glControlの起動時に実行される。
        private void glControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
        }

        //glControlのサイズ変更時に実行される。
        private void glControl_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl.Size.Width, glControl.Size.Height);

            float aspect = (float)glControl.Size.Width / (float)glControl.Size.Height;
            float fovy = (float)Math.PI / 2.0f;

            GL.MatrixMode(MatrixMode.Projection);
            Projection =
                Matrix4.CreatePerspectiveFieldOfView(
                    fovy,
                    aspect,
                    1.0f,       // near
                    6400.0f     // far
                    );

            GL.LoadMatrix(ref Projection);

            // Orthographic 平行投影
            /*
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection =
                Matrix4.CreateOrthographic(glControl.Size.Width, glControl.Size.Height, 1.0f, -64.0f);
            GL.LoadMatrix(ref projection);
            */
        }

        //glControlの描画時に実行される。
        private void glControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            float w2 = 1.0f;
            float z = 0.0f;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);

            Vector3 eye = Vector3.Zero;
            eye.Y = 4f;
            eye.Z = 4f;
            Matrix4 modelview = Matrix4.LookAt(eye, Vector3.Zero, Vector3.UnitY);

            /*
            Vector3 eye = Vector3.Zero;
            eye.Z = 1.0f;
            Matrix4 modelview = Matrix4.LookAt(eye, Vector3.Zero, Vector3.UnitY);
            */

            GL.LoadMatrix(ref modelview);

            float[] m = new float[16];

            GL.GetFloat(GetPName.ModelviewMatrix, m);
            /*
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(Color4.White);
            GL.Vertex3(-w2, w2, z);
            GL.Color4(Color4.Red);
            GL.Vertex3(-w2, -w2, z);
            GL.Color4(Color4.Lime);
            GL.Vertex3(w2, -w2, z);
            GL.Color4(Color4.Blue);
            GL.Vertex3(w2, w2, z);
            GL.End();
            */

            GL.Begin(PrimitiveType.Lines);

            GL.Color4(Color4.White);
            GL.Vertex3(-w2, w2, z);
            GL.Vertex3(-w2, -w2, z);

            GL.Color4(Color4.Red);
            GL.Vertex3(-w2, -w2, z);
            GL.Vertex3(w2, -w2, z);

            GL.Color4(Color4.Lime);
            GL.Vertex3(w2, -w2, z);
            GL.Vertex3(w2, w2, z);

            GL.Color4(Color4.Blue);
            GL.Vertex3(w2, w2, z);
            GL.Vertex3(-w2, w2, z);


            GL.Color4(Color4.White);
            GL.Vertex3(-w2, -w2, 0);
            GL.Vertex3(-w2, -w2, -w2);

            GL.Color4(Color4.White);
            GL.Vertex3(-w2, -w2, -w2);
            GL.Vertex3(w2, -w2, -w2);

            GL.Color4(Color4.White);
            GL.Vertex3(w2, -w2, -w2);
            GL.Vertex3(w2, -w2, 0);

            GL.End();
            glControl.SwapBuffers();
        }

        private void test1()
        {
            float w2 = 1.0f;
            float z = 0.0f;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);

            Vector3 eye = Vector3.Zero;
            eye.Y = 4f;
            eye.Z = 4f;

            Matrix4 modelview = Matrix4.LookAt(eye, Vector3.Zero, Vector3.UnitY);
            GL.LoadMatrix(ref modelview);

            float[] m = new float[16];

            GL.GetFloat(GetPName.ModelviewMatrix, m);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(Color4.White);
            GL.Vertex3(-w2, w2, z);
            GL.Color4(Color4.Red);
            GL.Vertex3(-w2, -w2, z);
            GL.Color4(Color4.Lime);
            GL.Vertex3(w2, -w2, z);
            GL.Color4(Color4.Blue);
            GL.Vertex3(w2, w2, z);
            GL.End();

            glControl.SwapBuffers();
        }

        private void test2()
        {
            float w2 = 1.0f;
            float z = 0.0f;


            Vector4 v = default(Vector4);

            v.X = 1f;
            v.Y = 1f;
            v.Z = 0f;
            v.W = 0;

            Vector4 v2;
            Vector4 v3;
            Vector4 v4;

            Vector3 eye = Vector3.Zero;
            eye.Y = 4f;
            eye.Z = 4f;
            Matrix4 modelview = Matrix4.LookAt(eye, Vector3.Zero, Vector3.UnitY);

            v2 = Vector4.Transform(v, modelview);

            v3 = Vector4.Transform(v2, Projection);

            v4 = v3;

            //v4 = Vector3.Multiply(v3, (float)glControl.Size.Height);

            v4.X *= (float)glControl.Size.Height / 2;
            v4.Y *= (float)glControl.Size.Height / 2;

            v4.X += (float)glControl.Size.Width / 2;
            v4.Y += (float)glControl.Size.Height / 2;

        }

        private void debugCommand(String s)
        {
            if (s == "test1")
            {
                test1();
            }
            else if (s == "test2")
            {
                test2();
            }
        }
    }
}
