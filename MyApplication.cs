using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using INFOGR2019Tmpl8;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Template
{
	class MyApplication
	{
		// member variables
		public Surface screen;
        public Camera camera;
        public int prog, prog2, csID, physID, ssbo_col, ssbo_sphere, ssbo_light, ssbo_plane, ssbo_tri, ssbo_active, u_camPos, u_scTL, u_scTR, u_scDL, u_img, u_positions, u_input, img;
        public float[] colors, spheres, cam, lights, planes, tries, totalybools;
        public Vector4 input, positions;
        public Vector3 direction;
        public DateTime time;
		// initialize
		public void Init()
		{
            camera = new Camera(new Vector3(0, 0, -8.5f), new Vector3(0, 0, 2), screen);

            time = DateTime.Now;

            colors = new float[screen.width * screen.height * 4];
            img = CreateTex(screen.width, screen.height);

            Random rnd = new Random();
            direction = new Vector3((rnd.Next(100) < 50) ? 1 : -1, (float)rnd.NextDouble() * 2 - 1, 0);
            Vector3.Normalize(direction);
            CreateSphereData();

            CreateLightData();

            CreatePlaneData();

            CreateTriData();

            CreatePlayer1();

            CreatePlayer2();

            totalybools = new float[]{0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1};

            prog2 = GL.CreateProgram();
            prog = GL.CreateProgram();
            LoadShader("../../shaders/phys.glsl", ShaderType.ComputeShader, prog2, out physID);
            LoadShader("../../shaders/cs.glsl", ShaderType.ComputeShader, prog, out csID);
            GL.LinkProgram(prog);
            GL.LinkProgram(prog2);
            u_camPos = GL.GetUniformLocation(prog, "camPos");
            u_scTL = GL.GetUniformLocation(prog, "screenTL");
            u_scTR = GL.GetUniformLocation(prog, "screenTR");
            u_scDL = GL.GetUniformLocation(prog, "screenDL");
            u_img = GL.GetUniformLocation(prog, "img");
            u_positions = GL.GetUniformLocation(prog2, "positions");
            u_input = GL.GetUniformLocation(prog2, "inp");

            Createssbo(ref ssbo_col, colors, 0);
            Createssbo(ref ssbo_sphere, spheres, 1);
            Createssbo(ref ssbo_light, lights, 2);
            Createssbo(ref ssbo_plane, planes, 3);
            Createssbo(ref ssbo_tri, tries, 4);
            Createssbo(ref ssbo_active, totalybools, 5);
        }
		// tick: renders one frame
		public void Tick()
		{
            //screen.Clear( 0 );

            GL.UseProgram(prog2);

            TimeSpan deltaTime = DateTime.Now - time;
            time = DateTime.Now;

            positions = new Vector4(8, 20, 0, deltaTime.Milliseconds);

            KeyboardState state = Keyboard.GetState();
            input = new Vector4
            {
                X = state.IsKeyDown(Key.W) ? 1 : 0,
                Y = state.IsKeyDown(Key.S) ? 1 : 0,
                Z = state.IsKeyDown(Key.Up) ? 1 : 0,
                W = state.IsKeyDown(Key.Down) ? 1 : 0
            };

            GL.Uniform4(u_positions, ref positions);
            GL.Uniform4(u_input, ref input);

            GL.DispatchCompute(1, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            GL.UseProgram(prog);
                
            GL.Uniform3(u_camPos, ref camera.position);
            GL.Uniform3(u_scTL, ref camera.screen[0]);
            GL.Uniform3(u_scTR, ref camera.screen[1]);
            GL.Uniform3(u_scDL, ref camera.screen[2]);
            GL.Uniform1(u_img, 0);

            GL.DispatchCompute(screen.width/8, screen.height/8, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
        }

        void ReadFromBuffer(int ssbo, float[] data)
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo);
            IntPtr ptr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly);
            Marshal.Copy(ptr, data, 0, data.Length);
            GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);
        }

        void LoadShader(String name, ShaderType type, int program, out int ID)
        {
            ID = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(name))
                GL.ShaderSource(ID, sr.ReadToEnd()); GL.CompileShader(ID);
            GL.AttachShader(program, ID);
            Console.WriteLine(GL.GetShaderInfoLog(ID));
        }

        public void Createssbo(ref int ssbo, float[] data, int bind)
        {
            ssbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo);
            GL.BufferData<float>(BufferTarget.ShaderStorageBuffer, data.Length * 4, data, BufferUsageHint.DynamicCopy);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bind, ssbo);
        }

        private int CreateTex(int w, int h)
        {
            int tex;
            tex = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, w, h, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindImageTexture(0, tex, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

            return tex;

        }

        private void CreateSphereData()
        {
            int tNum = 0;
            spheres = new float[8*6];
            // Player 1 lives:
            AddSphere(new Vector3(-3f, -1.8f, -0.2f), 0.2f, new Vector4(0.5f, 0.25f, 0.25f, 0.7f), tNum++);
            AddSphere(new Vector3(-2.5f, -1.8f, -0.2f), 0.2f, new Vector4(0.5f, 0.25f, 0.25f, 0.7f), tNum++);
            AddSphere(new Vector3(-2f, -1.8f, -0.2f), 0.2f, new Vector4(0.5f, 0.25f, 0.25f, 0.7f), tNum++);

            // Player 2 lives:
            AddSphere(new Vector3(3f, -1.8f, -0.2f), 0.2f, new Vector4(0.25f, 0.25f, 0.5f, 0.7f), tNum++);
            AddSphere(new Vector3(2.5f, -1.8f, -0.2f), 0.2f, new Vector4(0.25f, 0.25f, 0.5f, 0.7f), tNum++);
            AddSphere(new Vector3(2f, -1.8f, -0.2f), 0.2f, new Vector4(0.25f, 0.25f, 0.5f, 0.7f), tNum++);
        }

        private void CreateLightData()
        {
            lights = new float[12*8];
            // Light 1
            // Position
            lights[0] = 0f;
            lights[1] = 0f;
            lights[2] = -1.5f;
            lights[3] = 0.1f;
            // Intensity (colour)
            lights[4] = 3f;
            lights[5] = 3f;
            lights[6] = 3f;
            lights[7] = 0f;

            lights[8] = direction.X;
            lights[9] = direction.Y;
            lights[10] = 0f;
            lights[11] = 0f;
            //sunlight
            // Position
            lights[12] = -0.1f;
            lights[13] = -0.1f;
            lights[14] = -19f;
            lights[15] = 0.0001f;
            // Intensity (colour)
            lights[16] = 100f;
            lights[17] = 100f;
            lights[18] = 100f;
            lights[19] = 100f;
            
            int tNum = 2;
            // Player 1 lives:
            AddLight(new Vector3(-3f, -1.8f, -0.2f), 0.2f, new Vector4(0.5f, 0.25f, 0.25f, 0f), tNum++);
            AddLight(new Vector3(-2.5f, -1.8f, -0.2f), 0.2f, new Vector4(0.5f, 0.25f, 0.25f, 0f), tNum++);
            AddLight(new Vector3(-2f, -1.8f, -0.2f), 0.2f, new Vector4(0.5f, 0.25f, 0.25f, 0f), tNum++);

            // Player 2 lives:
            AddLight(new Vector3(3f, -1.8f, -0.2f), 0.2f, new Vector4(0.25f, 0.25f, 0.5f, 0f), tNum++);
            AddLight(new Vector3(2.5f, -1.8f, -0.2f), 0.2f, new Vector4(0.25f, 0.25f, 0.5f, 0f), tNum++);
            AddLight(new Vector3(2f, -1.8f, -0.2f), 0.2f, new Vector4(0.25f, 0.25f, 0.5f, 0f), tNum++);
        }

        private void AddSphere(Vector3 pos, float radius, Vector4 colour, int tNum) 
        {
            tNum *= 8;

            spheres[tNum] = pos.X;
            spheres[tNum + 1] = pos.Y;
            spheres[tNum + 2] = pos.Z;
            spheres[tNum + 3] = radius;
            spheres[tNum + 4] = colour.X;
            spheres[tNum + 5] = colour.Y;
            spheres[tNum + 6] = colour.Z;
            spheres[tNum + 7] = colour.W;
        }

        private void AddLight(Vector3 pos, float radius, Vector4 color, int tNum) 
        {
            tNum *= 12;
            lights[tNum] = pos.X;
            lights[tNum + 1] = pos.Y;
            lights[tNum + 2] = pos.Z;
            lights[tNum + 3] = radius;
            lights[tNum + 4] = color.X;
            lights[tNum + 5] = color.Y;
            lights[tNum + 6] = color.Z;
        }

        private void CreatePlaneData()
        {
            planes = new float[24];
            // Plane 1
            // Center
            planes[0] = 0f;
            planes[1] = 0f;
            planes[2] = 0f;
            planes[3] = 0f;
            // Normal
            planes[4] = 0f;
            planes[5] = 0f;
            planes[6] = -1f;
            planes[7] = 0f;
            // Colour
            planes[8] = 1f;
            planes[9] = 0.75f;
            planes[10] = 0.6f;
            planes[11] = 0.8f;
        }

        private void CreateTriData()
        {
            tries = new float[32*24];
            //left wall
            AddTri(new Vector3(-3.5f, -2f, 0), new Vector3(-3.5f, 2f, 0), new Vector3(-3.5f, 2f, -4f), new Vector4(0.85f, 0.85f, 1f, 0f), 0);
            AddTri(new Vector3(-3.5f, -2f, 0), new Vector3(-3.5f, 2f, -4f), new Vector3(-3.5f, -2f, -4f), new Vector4(0.85f, 0.85f, 1f, 0f), 1);
            //right wall
            AddTri(new Vector3(3.5f, -2f, 0), new Vector3(3.5f, 2f, -4f), new Vector3(3.5f, 2f, 0f), new Vector4(0.85f, 0.85f, 1f, 0f), 2);
            AddTri(new Vector3(3.5f, -2f, 0), new Vector3(3.5f, -2f, -4f), new Vector3(3.5f, 2f, -4f), new Vector4(0.85f, 0.85f, 1f, 0f), 3);
            //up wall
            AddTri(new Vector3(-3.5f, -2f, 0f), new Vector3(3.5f, -2f, -4f), new Vector3(3.5f, -2f, -0f), new Vector4(0.85f, 0.85f, 1f, 0f), 4);
            AddTri(new Vector3(-3.5f, -2f, 0), new Vector3(-3.5f, -2f, -4f), new Vector3(3.5f, -2f, -4f), new Vector4(0.85f, 0.85f, 1f, 0f), 5);
            //down wall
            AddTri(new Vector3(-3.5f, 2f, 0f), new Vector3(3.5f, 2f, 0f), new Vector3(3.5f, 2f, -4f), new Vector4(0.85f, 0.85f, 1f, 0f), 6);
            AddTri(new Vector3(-3.5f, 2f, 0), new Vector3(3.5f, 2f, -4f), new Vector3(-3.5f, 2f, -4f), new Vector4(0.85f, 0.85f, 1f, 0f), 7);
        }

        private void CreatePlayer1()
        {
            int tNum = 8;
            //front
            AddTri(new Vector3(-3.1f, -0.75f, -1.6f), new Vector3(-3.1f, 0.75f, -1.6f), new Vector3(-3.3f, -0.75f, -1.6f), new Vector4(1, 0.2f, 0.2f, 0), tNum++);
            AddTri(new Vector3(-3.3f, -0.75f, -1.6f), new Vector3(-3.1f, 0.75f, -1.6f), new Vector3(-3.3f, 0.75f, -1.6f), new Vector4(1, 0.2f, 0.2f, 0), tNum++);
            //top
            AddTri(new Vector3(-3.1f, -0.75f, -1.6f), new Vector3(-3.3f, -0.75f, -1.6f), new Vector3(-3.1f, -0.75f, -1.4f), new Vector4(1, 0.2f, 0.2f, 0), tNum++);
            AddTri(new Vector3(-3.3f, -0.75f, -1.6f), new Vector3(-3.3f, -0.75f, -1.4f), new Vector3(-3.1f, -0.75f, -1.4f), new Vector4(1, 0.2f, 0.2f, 0), tNum++);
            //bottom
            AddTri(new Vector3(-3.1f, 0.75f, -1.6f), new Vector3(-3.1f, 0.75f, -1.4f), new Vector3(-3.3f, 0.75f, -1.6f), new Vector4(1, 0.2f, 0.2f, 0), tNum++);
            AddTri(new Vector3(-3.3f, 0.75f, -1.6f), new Vector3(-3.1f, 0.75f, -1.4f), new Vector3(-3.3f, 0.75f, -1.4f), new Vector4(1, 0.2f, 0.2f, 0), tNum++);
            //back
            AddTri(new Vector3(-3.1f, -0.75f, -1.4f), new Vector3(-3.3f, -0.75f, -1.4f), new Vector3(-3.1f, 0.75f, -1.4f), new Vector4(1, 0.2f, 0.2f, 0), tNum++);
            AddTri(new Vector3(-3.3f, -0.75f, -1.4f), new Vector3(-3.3f, 0.75f, -1.4f), new Vector3(-3.1f, 0.75f, -1.4f), new Vector4(1, 0.2f, 0.2f, 0), tNum++);
            //right
            AddTri(new Vector3(-3.1f, 0.75f, -1.6f), new Vector3(-3.1f, -0.75f, -1.4f), new Vector3(-3.1f, 0.75f, -1.4f), new Vector4(1, 0.2f, 0.2f, 0), tNum++);
            AddTri(new Vector3(-3.1f, -0.75f, -1.4f), new Vector3(-3.1f, 0.75f, -1.6f), new Vector3(-3.1f, -0.75f, -1.6f), new Vector4(1, 0.2f, 0.2f, 0), tNum++);
            //left
            AddTri(new Vector3(-3.3f, 0.75f, -1.6f), new Vector3(-3.3f, 0.75f, -1.4f), new Vector3(-3.3f, -0.75f, -1.4f), new Vector4(1, 0.2f, 0.2f, 0), tNum++);
            AddTri(new Vector3(-3.3f, -0.75f, -1.4f), new Vector3(-3.3f, -0.75f, -1.6f), new Vector3(-3.3f, 0.75f, -1.6f), new Vector4(1, 0.2f, 0.2f, 0), tNum++);

        }

        private void CreatePlayer2() 
        {
            int tNum = 20;
            //front
            AddTri(new Vector3(3.3f, -0.75f, -1.6f), new Vector3(3.3f, 0.75f, -1.6f), new Vector3(3.1f, -0.75f, -1.6f), new Vector4(0.2f, 0.2f, 1f, 0), tNum++);
            AddTri(new Vector3(3.1f, -0.75f, -1.6f), new Vector3(3.3f, 0.75f, -1.6f), new Vector3(3.1f, 0.75f, -1.6f), new Vector4(0.2f, 0.2f, 1f, 0), tNum++);
            //top
            AddTri(new Vector3(3.3f, -0.75f, -1.6f), new Vector3(3.1f, -0.75f, -1.6f), new Vector3(3.3f, -0.75f, -1.4f), new Vector4(0.2f, 0.2f, 1f, 0), tNum++);
            AddTri(new Vector3(3.1f, -0.75f, -1.6f), new Vector3(3.1f, -0.75f, -1.4f), new Vector3(3.3f, -0.75f, -1.4f), new Vector4(0.2f, 0.2f, 1f, 0), tNum++);
            //bottom
            AddTri(new Vector3(3.3f, 0.75f, -1.6f), new Vector3(3.3f, 0.75f, -1.4f), new Vector3(3.1f, 0.75f, -1.6f), new Vector4(0.2f, 0.2f, 1f, 0), tNum++);
            AddTri(new Vector3(3.1f, 0.75f, -1.6f), new Vector3(3.3f, 0.75f, -1.4f), new Vector3(3.1f, 0.75f, -1.4f), new Vector4(0.2f, 0.2f, 1f, 0), tNum++);
            //back
            AddTri(new Vector3(3.3f, -0.75f, -1.4f), new Vector3(3.1f, -0.75f, -1.4f), new Vector3(3.3f, 0.75f, -1.4f), new Vector4(0.2f, 0.2f, 1f, 0), tNum++);
            AddTri(new Vector3(3.1f, -0.75f, -1.4f), new Vector3(3.1f, 0.75f, -1.4f), new Vector3(3.3f, 0.75f, -1.4f), new Vector4(0.2f, 0.2f, 1f, 0), tNum++);
            //right
            AddTri(new Vector3(3.3f, 0.75f, -1.6f), new Vector3(3.3f, -0.75f, -1.4f), new Vector3(3.3f, 0.75f, -1.4f), new Vector4(0.2f, 0.2f, 1f, 0), tNum++);
            AddTri(new Vector3(3.3f, -0.75f, -1.4f), new Vector3(3.3f, 0.75f, -1.6f), new Vector3(3.3f, -0.75f, -1.6f), new Vector4(0.2f, 0.2f, 1f, 0), tNum++);
            //left
            AddTri(new Vector3(3.1f, 0.75f, -1.6f), new Vector3(3.1f, 0.75f, -1.4f), new Vector3(3.1f, -0.75f, -1.4f), new Vector4(0.2f, 0.2f, 1f, 0), tNum++);
            AddTri(new Vector3(3.1f, -0.75f, -1.4f), new Vector3(3.1f, -0.75f, -1.6f), new Vector3(3.1f, 0.75f, -1.6f), new Vector4(0.2f, 0.2f, 1f, 0), tNum++);
        }

        private void AddTri(Vector3 v0, Vector3 v1, Vector3 v2, Vector4 color, int tNum)
        {
            Vector3 normal = Vector3.Normalize(Vector3.Cross(Vector3.Normalize(v2 - v0), Vector3.Normalize(v1 - v2)));
            tNum *= 24;
            tries[tNum] = v0.X;
            tries[tNum + 1] = v0.Y;
            tries[tNum + 2] = v0.Z;
            tries[tNum + 4] = normal.X;
            tries[tNum + 5] = normal.Y;
            tries[tNum + 6] = normal.Z;
            tries[tNum + 8] = color.X;
            tries[tNum + 9] = color.Y;
            tries[tNum + 10] = color.Z;
            tries[tNum + 11] = color.W;
            tries[tNum + 12] = v0.X;
            tries[tNum + 13] = v0.Y;
            tries[tNum + 14] = v0.Z;
            tries[tNum + 16] = v1.X;
            tries[tNum + 17] = v1.Y;
            tries[tNum + 18] = v1.Z;
            tries[tNum + 20] = v2.X;
            tries[tNum + 21] = v2.Y;
            tries[tNum + 22] = v2.Z;
        }

    }
}