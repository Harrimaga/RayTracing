#version 430
struct Sphere
{
	vec4 pos;
	vec4 color;
};

struct Plane
{
	vec4 center;
	vec4 normal;
	vec4 color;
};

struct Light
{
	vec4 pos;
	vec4 color;
};

struct Tri
{
	Plane p;
	vec4 v0;
	vec4 v1;
	vec4 v2;
};

struct Ray 
{
	vec3 origin;
	vec3 direction;
	float dis;
};

layout(std430, binding=2) buffer lights{
     Light light[];
};

layout(std430, binding=4) readonly buffer tries{
	Tri tri[];
};

layout(std430, binding=1) readonly buffer spheres{
    Sphere sphere[];
};

layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

// p1, p2, ball, deltatime
uniform vec4 positions;

// w, s, up, down
uniform vec4 input;

void main()
{
	float p1 = positions.x;
	float p2 = positions.y;
	float b = positions.z;
	float delta = positions.w;

	bool up1 = input.x == 1;
	bool down1 = input.y == 1;
	bool up2 = input.z == 1;
	bool down2 = input.w == 1;

	if (up1 && !down1)
	{
		float amount = delta/250.0;
		
	}
	else if(!up1 && down1)
	{
		
	}

	if (up2 && !down2) 
	{
		 
	}
	else if(!up2 && down2)
	{

	}
}
