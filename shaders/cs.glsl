#version 430
layout(std430, binding=0) buffer Pos{
    vec4 Color[];
};

struct Sphere
{
	vec4 pos;
	vec4 color;
};

struct Plane
{
	vec3 center;
	vec3 normal;
	vec4 color;
};

struct Light
{
	vec4 pos;
	vec4 color;
};

uniform vec3 camPos;
uniform vec3 screenTL;
uniform vec3 screenTR;
uniform vec3 screenDL;

uniform image2D img;

struct Ray 
{
	vec3 origin;
	vec3 direction;
	float dis;
};

layout(std430, binding=1) readonly buffer spheres{
    Sphere sphere[];
};

layout(std430, binding=2) readonly buffer lights{
     Light light[];
};

layout(std430, binding=3) readonly buffer planes{
	Plane plane[];
};

layout (local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

void IntersectSphere(in vec4 s, inout Ray ray, out vec3 intersectionPoint, out bool success)
{
	vec3 c = s.xyz - ray.origin;
	float t = dot(c, ray.direction);

	if (t < 0) 
	{
		intersectionPoint = vec3(0, 0, 0);
		success = false;
		return;
	}
	vec3 q = c - t * ray.direction;
	float p2 = dot(q, q);
	float d2 = dot(c, c) - t * t;

	if (p2 > s.w * s.w) 
	{
		intersectionPoint = vec3(0, 0, 0);
		success = false;
		return;
	}

	t -= sqrt(s.w * s.w - p2);
	if ((t < ray.dis) && (t > 0))
	{
		ray.dis = t;
		intersectionPoint = ray.origin + t * ray.direction;
		success = true;
		return;
	}

	intersectionPoint = vec3(0, 0, 0);
	success = false;
	return;
}

void IntersectPlane(in Plane p, inout Ray ray, out vec3 intersectionPoint, out bool success)
{
	float d = -dot(p.center, p.normal);
	float t = -(dot(ray.origin, p.normal) + d) / dot(ray.direction, p.normal);

	if (t > 0)
	{
		intersectionPoint = ray.origin + t * ray.direction;
		success = true;
		return;
	}

	intersectionPoint = vec3(0, 0, 0);
	success = false;
	return;
	
}

void main() 
{
	ivec2 storePos = ivec2(gl_GlobalInvocationID.xy);
	uint gWidth = gl_WorkGroupSize.x * gl_NumWorkGroups.x;
	uint gHeight = gl_WorkGroupSize.y * gl_NumWorkGroups.y;
	uint offset = storePos.y * gl_WorkGroupSize.x * gl_NumWorkGroups.x + storePos.x;


	//(center + new Vector3(-1, -1, 0))
    //(center + new Vector3( 1, -1, 0))
    //(center + new Vector3(-1,  1, 0))
	vec3 pixel = vec3(screenTL.x+(screenTR.x-screenTL.x)*storePos.x/gWidth, screenTL.y+(screenDL.y-screenTL.y)*storePos.y/gHeight, screenTL.z);

	Ray primaryRay = Ray(camPos, normalize(pixel - camPos), 999999);

	Color[offset] = vec4(0, 0, 0, 1);
	vec4 hitColor;
	Sphere sHit;
	bool succ = false;
	vec3 rayCastHit;
	for(int i=0;i<sphere.length();i++)
	{
		bool suc;
		vec3 rayCasthit;
		IntersectSphere(sphere[i].pos, primaryRay, rayCasthit, suc);

		if (suc) 
		{
			
			succ = true;
			hitPos = sphere[i].pos.xyz;
			hitColor = sphere[i].color;
			rayCastHit = rayCasthit;
		}
	}
	for(int ii=0;ii<plane.length();ii++)
	{
		bool suc;
		vec3 rayCasthit;
		IntersectPlane(plane[ii], primaryRay, rayCasthit, suc);

		if (suc)
		{
			succ = true;
			hitPos = rayCasthit;
			hitColor = plane[ii].color;
			rayCastHit = rayCasthit;
		}
	}
	// Shoot shadow rays
	for(int j=0;j<light.length();j++)
	{
		vec3 shadowOrigin = rayCastHit + normalize(light[j].pos.xyz - rayCastHit) * -0.0001f;
		Ray shadowRay = Ray(shadowOrigin, normalize(light[j].pos.xyz - rayCastHit), length(light[j].pos.xyz - rayCastHit));
		bool intersectOther;

		for(int k=0;k<sphere.length();k++)
		{
			vec3 notimp;
					
			IntersectSphere(sphere[k].pos, shadowRay, notimp, intersectOther);

			if (intersectOther)
			{
				break;
			}
		}

		if (!intersectOther)
		{
			vec3 norm = normalize(rayCastHit-hitPos.xyz);
			Color[offset] += light[j].color * hitColor * dot(norm, normalize( light[j].pos.xyz - rayCastHit))/(shadowRay.dis*shadowRay.dis);
		}
	}
	imageStore(img, storePos, vec4(Color[offset].xyz, 1));
}