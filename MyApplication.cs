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
        public int prog, csID, ssbo_col, ssbo_sphere, ssbo_light, u_camPos, u_scTL, u_scTR, u_scDL;
        public float[] colors, spheres, cam, lights;
		// initialize
		public void Init()
		{
            camera = new Camera(new Vector3(0, 0, -3), new Vector3(0, 0, 1), screen);

            colors = new float[screen.width * screen.height * 4];
            spheres = new float[16];
            // Sphere 1: Red
            // Position
            spheres[0] = 0f;
            spheres[1] = 0f;
            spheres[2] = 1.5f;
            //Radius
            spheres[3] = 1f;
            //Colour
            spheres[4] = 1f; //R
            spheres[5] = 0f; //G
            spheres[6] = 0.55f; //B
            spheres[7] = 1f; //A

            // Sphere 2: Green
            spheres[8] = 0.5f;
            spheres[9] = 0.5f;
            spheres[10] = 0f;
            spheres[11] = 0.5f;
            spheres[12] = 1f;
            spheres[13] = 1f;
            spheres[14] = 1f;
            spheres[15] = 1f;


            lights = new float[16];
            // Light 1
            // Position
            lights[0] = 2f;
            lights[1] = 2f;
            lights[2] = -2f;
            lights[3] = 0f;
            // Intensity (colour)
            lights[4] = 1.5f;
            lights[5] = 1f;
            lights[6] = 1f;
            lights[7] = 1f;
            // Position
            lights[8] = 0f;
            lights[9] = 2f;
            lights[10] = 0f;
            lights[11] = 0f;
            // Intensity (colour)
            lights[12] = 0.75f;
            lights[13] = 0.75f;
            lights[14] = 1.5f;
            lights[15] = 1f;

            prog = GL.CreateProgram();
            LoadShader("../../shaders/cs.glsl", ShaderType.ComputeShader, prog, out csID);
            GL.LinkProgram(prog);
            u_camPos = GL.GetUniformLocation(prog, "camPos");
            u_scTL = GL.GetUniformLocation(prog, "screenTL");
            u_scTR = GL.GetUniformLocation(prog, "screenTR");
            u_scDL = GL.GetUniformLocation(prog, "screenDL");

            Createssbo(ref ssbo_col, colors);
            Createssbo(ref ssbo_sphere, spheres);
            Createssbo(ref ssbo_light, lights);
		}
		// tick: renders one frame
		public void Tick()
		{
			screen.Clear( 0 );
            GL.UseProgram(prog);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, ssbo_col);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, ssbo_sphere);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, ssbo_light);

            GL.Uniform3(u_camPos, ref camera.position);
            GL.Uniform3(u_scTL, ref camera.screen[0]);
            GL.Uniform3(u_scTR, ref camera.screen[1]);
            GL.Uniform3(u_scDL, ref camera.screen[2]);

            GL.DispatchCompute(screen.width/8, screen.height/8, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, 0);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, 0);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, 0);

            ReadFromBuffer(ssbo_col, colors);
            for(int i = 0; i < screen.width; i++)
            {
                for (int j = 0; j < screen.height; j++)
                {
                    int index = i + j * screen.width;
                    screen.pixels[index] = MixColor((int)(colors[index * 4] * 255), (int)(colors[index * 4 + 1] * 255), (int)(colors[index * 4 + 2] * 255));
                }
            }
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
            Console.WriteLine(GL.GetShaderInfoLog(ID)); }

        int MixColor(int red, int green, int blue) {
            if(red > 255)
            {
                red = 255;
            }
            if (blue > 255)
            {
                blue = 255;
            }
            if (green > 255)
            {
                green = 255;
            }
            if(red < 0)
            {
                red = 0;
            }
            if(green < 0)
            {
                green = 0;
            }
            if(blue < 0)
            {
                blue = 0;
            }
            return (red << 16) + (green << 8) + blue;
        }

        public void Render()
        {
            for (int y = 0; y < 512; y++)
            {
                for (int x = 0; x < 512; x++)
                {
                    // TODO: Shoot ray through every point on the screen
                }
            }
        }

        public void Createssbo(ref int ssbo, float[] data)
        {
            ssbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo);
            GL.BufferData<float>(BufferTarget.ShaderStorageBuffer, data.Length * 4, data, BufferUsageHint.DynamicCopy);
        }

    }
}