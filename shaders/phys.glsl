#version 430
struct Light
{
	vec4 pos;
	vec4 color;
};

layout(std430, binding=2) buffer lights{
     Light light[];
};

layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

void main()
{
	for(int i = 0; i < light.length(); i++) {
		light[i].pos.x += 1;
	}
}
