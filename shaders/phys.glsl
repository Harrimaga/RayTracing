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
	vec4 direction;
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

layout(std430, binding=4) buffer tries{
	Tri tri[];
};

layout(std430, binding=1) buffer spheres{
    Sphere sphere[];
};

layout(std430, binding=5) buffer Activ{
	float activ[];
};

layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

// p1, p2, ball, deltatime
uniform vec4 positions;

// w, s, up, down
uniform vec4 inp;

void IntersectTri(in int tt, inout Ray ray, out vec3 intersectionPoint, out bool success, in float radius) 
{
	//same code as intersect plane
	tri[tt].p.center += tri[tt].p.normal*radius;
	float d = -dot(tri[tt].p.center, tri[tt].p.normal);
	float e = dot(ray.origin, tri[tt].p.normal.xyz);
	float f = dot(ray.direction, tri[tt].p.normal.xyz);
	float t = -(e + d) / f;
	tri[tt].p.center -= tri[tt].p.normal*radius;

	if (t > 0 && t < ray.dis)
	{
		intersectionPoint = ray.origin + t * ray.direction;
		vec3 p0 = tri[tt].v1.xyz - tri[tt].v0.xyz;
		vec3 p1 = tri[tt].v2.xyz - tri[tt].v1.xyz;
		vec3 p2 = tri[tt].v0.xyz - tri[tt].v2.xyz;
		vec3 c0 = intersectionPoint - tri[tt].v0.xyz;
		vec3 c1 = intersectionPoint - tri[tt].v1.xyz;
		vec3 c2 = intersectionPoint - tri[tt].v2.xyz;
		float a0 = dot(tri[tt].p.normal.xyz, cross(p0, c0));
		float a1 = dot(tri[tt].p.normal.xyz, cross(p1, c1));
		float a2 = dot(tri[tt].p.normal.xyz, cross(p2, c2));

		if(a0 < 0 && a1 < 0 && a2 < 0 || a0 > 0 && a1 > 0 && a2 > 0) 
		{
			ray.dis = t;
			success = true;
			return;
		}
	}

	intersectionPoint = vec3(0, 0, 0);
	success = false;
	return;
}

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

	Ray r = Ray(light[b].pos.xyz, light[0].direction.xyz, delta*length(light[0].direction.xyz)/500.0f);
	vec3 insPoint;
	bool suc;
	vec3 norm;
	for(int i=0;i<tri.length();i++)
	{
		IntersectTri(i, r, insPoint, suc, light[b].pos.w);
		if (suc)
		{
			if(i < 2) 
			{
				//linker muur
				if (activ[0] == 0)
				{
					activ[0] = 1;
					activ[8] = 0;
				}
				else if (activ[1] == 0)
				{
					activ[1] = 1;
					activ[9] = 0;
				}
				else if (activ[2] == 0)
				{
					activ[2] = 1;
					activ[10] = 0;
				}
			}
			else if( i < 4) 
			{
				//rechter muur
				if (activ[3] == 0)
				{
					activ[3] = 1;
					activ[11] = 0;
				}
				else if (activ[4] == 0)
				{
					activ[4] = 1;
					activ[12] = 0;
				}
				else if (activ[5] == 0)
				{
					activ[5] = 1;
					activ[13] = 0;
				}
			}
			norm = tri[i].p.normal.xyz;
			break;
		}
	}

	if (suc)
	{
		light[0].direction.xyz = light[0].direction.xyz - 2*dot(norm, light[0].direction.xyz)*norm;
		light[0].direction.xyz *= 1.01f;
		light[b].pos.xyz = insPoint + light[0].direction.xyz*(delta*length(light[0].direction.xyz)/500.0f - r.dis);
	}
	else
	{
		light[b].pos.xyz += light[0].direction.xyz*delta*length(light[0].direction.xyz)/500.0f;
	}
}
