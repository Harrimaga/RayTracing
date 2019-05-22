using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using INFOGR2019Tmpl8;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Template
{
	class MyApplication
	{
		// member variables
		public Surface screen;
        public Camera camera;
        public int prog, prog2, csID, physID, ssbo_col, ssbo_sphere, ssbo_light, ssbo_plane, ssbo_tri, u_camPos, u_scTL, u_scTR, u_scDL, u_img, img;
        public float[] colors, spheres, cam, lights, planes, tries;
		// initialize
		public void Init()
		{
            camera = new Camera(new Vector3(0, 0, -8), new Vector3(0, 0, 2), screen);

            colors = new float[screen.width * screen.height * 4];
            img = CreateTex(screen.width, screen.height);

            CreateSphereData();

            CreateLightData();

            CreatePlaneData();

            CreateTriData();

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

            Createssbo(ref ssbo_col, colors, 0);
            Createssbo(ref ssbo_sphere, spheres, 1);
            Createssbo(ref ssbo_light, lights, 2);
            Createssbo(ref ssbo_plane, planes, 3);
            Createssbo(ref ssbo_tri, tries, 4);
        }
		// tick: renders one frame
		public void Tick()
		{
            //screen.Clear( 0 );

            GL.UseProgram(prog2);
            GL.DispatchCompute(lights.Length, 1, 1);
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
            spheres = new float[16];
            // Sphere 1: Red
            // Position
            spheres[0] = -0.5f;
            spheres[1] = -1.5f;
            spheres[2] = 0f;
            //Radius
            spheres[3] = 1f;
            //Colour
            spheres[4] = 1f; //R
            spheres[5] = 1f; //G
            spheres[6] = 1f; //B
            spheres[7] = 0f; //A

            // Sphere 2: Green
            spheres[8] = 0.5f;
            spheres[9] = 0.5f;
            spheres[10] = 0f;
            spheres[11] = 0.5f;
            spheres[12] = 1f;
            spheres[13] = 0.5f;
            spheres[14] = 1f;
            spheres[15] = 0.5f;
        }

        private void CreateLightData()
        {
            lights = new float[16];
            // Light 1
            // Position
            lights[0] = 1.0f;
            lights[1] = 1.5f;
            lights[2] = -0.5f;
            lights[3] = 0.1f;
            // Intensity (colour)
            lights[4] = 1.5f;
            lights[5] = 1.5f;
            lights[6] = 4f;
            lights[7] = 0f;
            // Position
            lights[8] = 0f;
            lights[9] = 0f;
            lights[10] = -2f;
            lights[11] = 0.1f;
            // Intensity (colour)
            lights[12] = 4f;
            lights[13] = 4f;
            lights[14] = 1.5f;
            lights[15] = 0f;
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
            planes[9] = 1f;
            planes[10] = 1f;
            planes[11] = 0.5f;
        }

        private void CreateTriData()
        {
            tries = new float[24];
            AddTri(new Vector3(-2, 0, -1), new Vector3(0, 0, -1), new Vector3(0, 1, -1), new Vector4(1f, 1f, 1f, 0), 0);
        }

        private void AddTri(Vector3 v0, Vector3 v1, Vector3 v2, Vector4 color, int tNum)
        {
            Vector3 normal = Vector3.Cross(Vector3.Normalize(v2 - v0), Vector3.Normalize(v1 - v2));
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