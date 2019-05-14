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
        public int prog, csID, ssbo_col, ssbo_sphere, ubo_camera;
        public float[] colors, spheres, cam;
		// initialize
		public void Init()
		{
            camera = new Camera(new Vector3(0, 0, -15), new Vector3(0, 0, 1));

            colors = new float[screen.width * screen.height * 4];
            spheres = new float[8];
            spheres[0] = 0f;
            spheres[1] = 0f;
            spheres[2] = 0f;
            spheres[3] = 0.5f;
            spheres[4] = 1f;
            spheres[5] = 0f;
            spheres[6] = 0f;
            spheres[7] = 1f;

            cam = new float[18]
            {
                camera.position.X,
                camera.position.Y,
                camera.position.Z,
                camera.direction.X,
                camera.direction.Y,
                camera.direction.Z,
                camera.center.X,
                camera.center.Y,
                camera.center.Z,
                camera.screen[0].X,
                camera.screen[0].Y,
                camera.screen[0].Z,
                camera.screen[1].X,
                camera.screen[1].Y,
                camera.screen[1].Z,
                camera.screen[2].X,
                camera.screen[2].Y,
                camera.screen[2].Z
            };

            prog = GL.CreateProgram();
            LoadShader("../../shaders/cs.glsl", ShaderType.ComputeShader, prog, out csID);
            GL.LinkProgram(prog);
            Createssbo(ref ssbo_col, colors);
            Createssbo(ref ssbo_sphere, spheres);
            Createubo(ref ubo_camera, cam);
		}
		// tick: renders one frame
		public void Tick()
		{
			screen.Clear( 0 );
            GL.UseProgram(prog);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, ssbo_col);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, ssbo_sphere);
            GL.BindBuffer(BufferTarget.UniformBuffer, ubo_camera);

            GL.DispatchCompute(screen.width/8, screen.height/8, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, 0);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, 0);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

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

        public void Createubo(ref int ubo, float[] data)
        {
            ubo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, ubo);
            GL.BufferData<float>(BufferTarget.UniformBuffer, data.Length * 4, data, BufferUsageHint.DynamicCopy);
        }
    }
}