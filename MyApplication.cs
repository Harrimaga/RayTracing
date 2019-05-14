using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace Template
{
	class MyApplication
	{
		// member variables
		public Surface screen;
        public int prog, csID, ssbo_col;
        public float[] colors;
		// initialize
		public void Init()
		{
            colors = new float[screen.width * screen.height * 4];
            prog = GL.CreateProgram();
            LoadShader("../../shaders/cs.glsl", ShaderType.ComputeShader, prog, out csID);
            GL.LinkProgram(prog);
            ssbo_col = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo_col);
            GL.BufferData<float>(BufferTarget.ShaderStorageBuffer, colors.Length * 4, colors, BufferUsageHint.DynamicCopy);
		}
		// tick: renders one frame
		public void Tick()
		{
			screen.Clear( 0 );
            GL.UseProgram(prog);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, ssbo_col);
            GL.DispatchCompute(screen.width/8, screen.height/8, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, 0);

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

    }
}