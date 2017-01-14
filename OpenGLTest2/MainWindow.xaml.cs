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

namespace OpenGLTest2
{
    public partial class MainWindow : Window
    {
        private DebugInputThread InputThread;

        Matrix4 Projection;
        Matrix4 ModelView;

        Vector3 Eye = default(Vector3);
        Vector3 LookAt = default(Vector3);
        Vector3 UpVector = default(Vector3);

        public MainWindow()
        {
            InitializeComponent();

            InputThread = new DebugInputThread(debugCommand);
            InputThread.start();

            glControl.MouseMove += GlControl_MouseMove;

        }

        private void GlControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            
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
            eye.X = 4f;
            eye.Y = 4f;
            eye.Z = 4f;
            Matrix4 modelview = Matrix4.LookAt(eye, Vector3.Zero, Vector3.UnitY);


            GL.LoadMatrix(ref modelview);


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
            GL.Vertex3(-w2, -w2, -w2 * 2);

            GL.Color4(Color4.White);
            GL.Vertex3(-w2, -w2, -w2 * 2);
            GL.Vertex3(w2, -w2, -w2 * 2);

            GL.Color4(Color4.White);
            GL.Vertex3(w2, -w2, -w2 * 2);
            GL.Vertex3(w2, -w2, 0);

            GL.End();

            /*
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection =
                Matrix4.CreateOrthographic(glControl.Size.Width, glControl.Size.Height, 1.0f, -64.0f);
            GL.LoadMatrix(ref projection);


            Vector3 eye2 = Vector3.Zero;
            eye.Z = 4f;
            Matrix4 modelview2 = Matrix4.LookAt(eye2, Vector3.Zero, Vector3.UnitY);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview2);

            GL.Rect(0, 0, 10, 10);
            */

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            Matrix4 view = Matrix4.CreateOrthographicOffCenter(
                                        0, glControl.Size.Width,
                                        glControl.Size.Height,0,
                                        0, 1000);
            GL.MultMatrix(ref view);


            GL.Begin(PrimitiveType.Lines);

            GL.Color4(Color4.White);
            GL.Vertex3(0, 0, 0f);
            GL.Vertex3(100, 100, 0f);

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
            Vector4 v = default(Vector4);

            Vector4 vt;
            Vector4 vt2 = default(Vector4);
            Vector4 v1 = default(Vector4);
            Vector4 v2 = default(Vector4);

            Vector3 eye = Vector3.Zero;
            eye.X = 4f;
            eye.Y = 4f;
            eye.Z = 4f;
            Matrix4 modelview = Matrix4.LookAt(eye, Vector3.Zero, Vector3.UnitY);


            v.X = -1f;
            v.Y = 1f;
            v.Z = 0f;
            v.W = 1f;

            v1 = trans(modelview, Projection, v);

            vt2 = v1;

            vt2.X += 4;

            vt = invert(modelview, Projection, v1);

            vt = invert(modelview, Projection, vt2);


            v.X = 1f;
            v.Y = 1f;
            v.Z = 0f;
            v.W = 1f;

            v2 = trans(modelview, Projection, v);

            dumpVect("v1", v1);
            dumpVect("v2", v2);
            testDrawLine(v1, v2);


            v1 = v2;

            v.X = 1f;
            v.Y = -1f;
            v.Z = 0f;
            v.W = 1f;

            v2 = trans(modelview, Projection, v);
            dumpVect("v1", v1);
            dumpVect("v2", v2);
            testDrawLine(v1, v2);


            v1 = v2;

            v.X = -1f;
            v.Y = -1f;
            v.Z = 0f;
            v.W = 1f;

            v2 = trans(modelview, Projection, v);
            dumpVect("v1", v1);
            dumpVect("v2", v2);
            testDrawLine(v1, v2);


            v1 = v2;

            v.X = -1f;
            v.Y = 1f;
            v.Z = 0f;
            v.W = 1f;

            v2 = trans(modelview, Projection, v);
            dumpVect("v1", v1);
            dumpVect("v2", v2);
            testDrawLine(v1, v2);
        }

        private void testDrawLine(Vector4 v1, Vector4 v2)
        {
            Graphics g = pictureBox1.CreateGraphics();
            g.DrawLine(Pens.White, v1.X, v1.Y, v2.X, v2.Y);
        }

        private Vector4 trans(Matrix4 modelview, Matrix4 projection, Vector4 v)
        {
            Vector4 vw = v;

            vw = Vector4.Transform(vw, modelview);
            vw = Vector4.Transform(vw, projection);

            vw.X /= vw.W;
            vw.Y /= vw.W;
            vw.Z /= vw.W;

            vw.X = vw.X * ((float)glControl.Size.Width / 2f);
            vw.Y = -vw.Y * ((float)glControl.Size.Height / 2f);
            vw.X += (float)glControl.Size.Width / 2f;
            vw.Y += (float)glControl.Size.Height / 2f;

            return vw;
        }


        private Vector4 invert(Matrix4 modelview, Matrix4 projection, Vector4 v)
        {
            Vector4 vw = v;

            Matrix4 imodelview = Matrix4.Invert(modelview);
            Matrix4 iprojection = Matrix4.Invert(projection);

            vw.X -= (float)glControl.Size.Width / 2f;
            vw.Y -= (float)glControl.Size.Height / 2f;

            vw.X = vw.X / ((float)glControl.Size.Width / 2f);
            vw.Y = -vw.Y / ((float)glControl.Size.Height / 2f);

            vw.X *= vw.W;
            vw.Y *= vw.W;
            vw.Z *= vw.W;

            vw = Vector4.Transform(vw, iprojection);
            vw = Vector4.Transform(vw, imodelview);

            vw.W = 1f;

            return vw;
        }


        private void test3()
        {
            Graphics g = pictureBox1.CreateGraphics();

            g.DrawLine(Pens.White, 10, 10, 100, 100);
        }

        private void dumpVect(string name, Vector4 v)
        {
            debugp(name + "\n" +
                    "x=" + v.X.ToString() + "\n" +
                    "y=" + v.Y.ToString() + "\n" +
                    "z=" + v.Z.ToString() + "\n" +
                    "w=" + v.W.ToString() + "\n"
                    );
        }

        private void dump()
        {
            debugl("Ctrl w=" +
                    glControl.Size.Width.ToString() +
                    " h=" +
                    glControl.Size.Height.ToString()
                    );
        }

        private void debugp(string s)
        {
            Console.Write(s);
        }

        private void debugl(string s)
        {
            Console.WriteLine(s);
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
            else if (s == "test3")
            {
                test3();
            }
            else if (s == "dump")
            {
                dump();
            }
        }
    }
}
