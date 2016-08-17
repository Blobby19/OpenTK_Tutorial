using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKTutorial3
{
    class Game : GameWindow
    {
        public Game()
            : base(512, 512, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 4))
        {

        }

        int pgmID;
        int vsID;
        int fsID;
        int attribute_vcol;
        int attribute_vpos;
        int uniform_mview;
        int vbo_position;
        int vbo_color;
        int vbo_mview;
        int ibo_elements;
        Vector3[] vertData;
        Vector3[] colData;
        Matrix4[] mviewData;
        int[] indiceData;
        float time = 0.0f;

        void initProgram()
        {
            pgmID = GL.CreateProgram();

            loadShader("vs.glsl", ShaderType.VertexShader, pgmID, out vsID);
            loadShader("fs.glsl", ShaderType.FragmentShader, pgmID, out vsID);

            GL.LinkProgram(pgmID);
            Console.WriteLine(GL.GetProgramInfoLog(pgmID));

            attribute_vpos = GL.GetAttribLocation(pgmID, "vPosition");
            attribute_vcol = GL.GetAttribLocation(pgmID, "vColor");
            uniform_mview = GL.GetUniformLocation(pgmID, "modelview");

            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out vbo_color);
            GL.GenBuffers(1, out vbo_mview);

            GL.GenBuffers(1, out ibo_elements);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            initProgram();

            vertData = new Vector3[]
            {
                new Vector3(-0.8f, -0.8f, -0.8f),
                new Vector3(0.8f, -0.8f, -0.8f),
                new Vector3(0.8f, 0.8f, -0.8f),
                new Vector3(-0.8f, 0.8f, -0.8f),
                new Vector3(-0.8f, -0.8f, 0.8f),
                new Vector3(0.8f, -0.8f, 0.8f),
                new Vector3(0.8f, 0.8f, 0.8f),
                new Vector3(-0.8f, 0.8f, 0.8f),
            };

            indiceData = new int[]
            {
                0, 2, 1,
                0, 3, 2,
                1, 2, 6,
                6, 5, 1,
                4, 5, 6,
                6, 7, 4,
                2, 3, 6,
                6, 3, 7,
                0, 7, 3,
                0, 4, 7,
                0, 1, 5,
                0, 5, 4
            };

            colData = new Vector3[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 1f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 1f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 0f, 1f)
            };

            mviewData = new Matrix4[]
            {
                Matrix4.Identity
            };

            Title = "Tutorial 3";
            GL.ClearColor(Color.CornflowerBlue);
            GL.PointSize(5f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            GL.EnableVertexAttribArray(attribute_vpos);
            GL.EnableVertexAttribArray(attribute_vcol);

            Console.WriteLine(this.RenderFrequency);

            GL.DrawElements(BeginMode.Triangles, indiceData.Length, DrawElementsType.UnsignedInt, 0);

            GL.DisableVertexAttribArray(attribute_vpos);
            GL.DisableVertexAttribArray(attribute_vcol);

            GL.Flush();
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertData.Length * Vector3.SizeInBytes), vertData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(colData.Length * Vector3.SizeInBytes), colData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, true, 0, 0);

            time += (float)e.Time;

            mviewData[0] = Matrix4.CreateRotationY(0.55f * time) * Matrix4.CreateRotationX(0.15f * time) * Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f) * Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f);

            GL.UniformMatrix4(uniform_mview, false, ref mviewData[0]);

            GL.UseProgram(pgmID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indiceData.Length * sizeof(int)), indiceData, BufferUsageHint.StaticDraw);
        }

        void loadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using(StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }
    }
}
