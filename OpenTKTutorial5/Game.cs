using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKTutorial5
{
    class Game : GameWindow
    {
        int pgmId;
        int vsID;
        int fsID;
        int attribute_vpos;
        int attribute_vcol;
        int uniform_mview;
        int vbo_position;
        int vbo_color;
        int vbo_mview;
        int ibo_elements;
        Vector3[] vertData;
        Vector3[] colData;
        int[] indiceData;
        float time = 0.0f;
        List<Volume> objects = new List<Volume>();
        Camera cam = new Camera();
        Vector2 lastMousePos = new Vector2();

        void initProgram()
        {
            lastMousePos = new Vector2(Mouse.X, Mouse.Y);

            objects.Add(new Cube());
            objects.Add(new Cube());

            pgmId = GL.CreateProgram();

            loadShader("vs.glsl", ShaderType.VertexShader, pgmId, out vsID);
            loadShader("fs.glsl", ShaderType.FragmentShader, pgmId, out fsID);

            GL.LinkProgram(pgmId);
            Console.WriteLine(GL.GetProgramInfoLog(pgmId));

            attribute_vpos = GL.GetAttribLocation(pgmId, "vPosition");
            attribute_vcol = GL.GetAttribLocation(pgmId, "vColor");
            uniform_mview = GL.GetUniformLocation(pgmId, "modelview");

            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out vbo_color);
            GL.GenBuffers(1, out vbo_mview);

            GL.GenBuffers(1, out ibo_elements);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            initProgram();

            Title = "OpenTkTutorial5";

            GL.ClearColor(Color.Cornsilk);
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

            int indiceAt = 0;

            foreach(Volume v in objects)
            {
                GL.UniformMatrix4(uniform_mview, false, ref v.ModelViewProjectionMatrix);
                GL.DrawElements(BeginMode.Triangles, v.IndiceCount, DrawElementsType.UnsignedInt, indiceAt * sizeof(uint));

                indiceAt += v.IndiceCount;
            }

            GL.DisableVertexAttribArray(attribute_vpos);
            GL.DisableVertexAttribArray(attribute_vcol);

            GL.Flush();
            SwapBuffers();

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> cols = new List<Vector3>();

            int vertCount = 0;
            foreach(Volume v in objects)
            {
                verts.AddRange(v.GetVerts().ToList());
                cols.AddRange(v.GetColorData().ToList());
                inds.AddRange(v.GetIndices().ToList());
                vertCount += v.VertCount;
            }

            vertData = verts.ToArray();
            colData = cols.ToArray();
            indiceData = inds.ToArray();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertData.Length * Vector3.SizeInBytes), vertData, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colData.Length * Vector3.SizeInBytes), colData, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, true, 0, 0);

            time += (float)e.Time;

            objects[0].Position = new Vector3(0.3f, -0.5f + (float)Math.Sin(time), -3.0f);
            objects[0].Rotation = new Vector3(0.55f * time, 0.25f * time, 0);
            objects[0].Scale = new Vector3(0.5f, 0.5f, 0.5f);

            objects[1].Position = new Vector3(-1.0f, 0.5f + (float)Math.Cos(time), -2.0f);
            objects[1].Rotation = new Vector3(0.25f * time, 0.35f * time, 0f);
            objects[1].Scale = new Vector3(0.7f, 0.7f, 0.7f);

            foreach(Volume v in objects)
            {
                v.CalculateModelMatrix();
                v.ViewProjectionMatrix = cam.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f);
                v.ModelViewProjectionMatrix = v.ModelMatrix * v.ViewProjectionMatrix;
            }

            GL.UseProgram(pgmId);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indiceData.Length * sizeof(int)), indiceData, BufferUsageHint.StreamDraw);

            if (Focused)
            {
                Vector2 delta = lastMousePos - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
                lastMousePos += delta;
                cam.AddRotation(delta.X, delta.Y);
                ResetCursor();
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if(e.KeyChar == 27)
            {
                Exit();
            }

            switch (e.KeyChar)
            {
                case 'w':
                    cam.Move(0f, 0.1f, 0f);
                    break;
                case 'a':
                    cam.Move(-0.1f, 0f, 0f);
                    break;
                case 's':
                    cam.Move(0f, -0.1f, 0f);
                    break;
                case 'd':
                    cam.Move(0.1f, 0f, 0f);
                    break;
                case 'q':
                    cam.Move(0f, 0f, 0.1f);
                    break;
                case 'e':
                    cam.Move(0f, 0f, -0.1f);
                    break;
            }
        }

        void ResetCursor()
        {
            OpenTK.Input.Mouse.SetPosition(Bounds.Left + Bounds.Width / 2, Bounds.Top + Bounds.Height / 2);
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);
            if (Focused)
                ResetCursor();
        }

        public Game()
            : base(512, 512, new GraphicsMode(32, 24, 0, 4))
        {

        }

        void loadShader(String filename, ShaderType type, int program, out int address)
        {

            address = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }
    }
}
