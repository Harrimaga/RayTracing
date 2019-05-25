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
uniform vec4 inp;

void MoveTri(int t, float amount) {
	tri[t].p.center.y += amount;
	tri[t].v0.y += amount;
	tri[t].v1.y += amount;
	tri[t].v2.y += amount;
}

void main()
{
	int p1 = int(positions.x);
	int p2 = int(positions.y);
	int b = int(positions.z);
	float delta = positions.w;

	bool up1 = inp.x == 1;
	bool down1 = inp.y == 1;
	bool up2 = inp.z == 1;
	bool down2 = inp.w == 1;

	if (up1 && !down1)
	{
		float amount = delta/250.0;
		float y = tri[p1].v0.y;
		if(y > -2) {
			if(y-amount < -2) {
				amount = 2+y;
			}
			for(int i=p1;i<p1+12;i++){
				MoveTri(i, -amount);
			}
		}
	}
	else if(!up1 && down1)
	{
		float amount = delta/250.0;
		float y = tri[p1+4].v0.y;
		if(y < 2) {
			if(y+amount > 2) {
				amount = 2-y;
			}
			for(int i=p1;i<p1+12;i++){
				MoveTri(i, amount);
			}
		}
	}

	if (up2 && !down2) 
	{
		float amount = delta/250.0;
		float y = tri[p2].v0.y;
		if(y > -2) {
			if(y-amount < -2) {
				amount = 2+y;
			}
			for(int i=p2;i<p2+12;i++){
				MoveTri(i, -amount);
			}
		}
	}
	else if(!up2 && down2)
	{
		float amount = delta/250.0;
		float y = tri[p2+4].v0.y;
		if(y < 2) {
			if(y+amount > 2) {
				amount = 2-y;
			}
			for(int i=p2;i<p2+12;i++){
				MoveTri(i, amount);
			}
		}
	}
}
