#version 430
const int aa = 2;
const int bounceLimit = 3;

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

uniform vec3 camPos;
uniform vec3 screenTL;
uniform vec3 screenTR;
uniform vec3 screenDL;

uniform writeonly image2D img;

layout(std430, binding=1) buffer spheres{
    Sphere sphere[];
};

layout(std430, binding=2) buffer lights{
     Light light[];
};

layout(std430, binding=3) buffer planes{
	Plane plane[];
};

layout(std430, binding=4) buffer tries{
	Tri tri[];
};

layout(std430, binding=5) buffer Activ{
	float activ[];
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

void IntersectSphere(in vec4 s, in Ray ray, out bool success)
{
	vec3 c = s.xyz - ray.origin;
	float t = dot(c, ray.direction);
	
	if (t < 0) 
	{
		success = false;
		if(dot(c,c) <= s.w*s.w) 
		{
			success = true;
		}
		return;
	}
	vec3 q = c - t * ray.direction;
	float p2 = dot(q, q);

	if (p2 > s.w * s.w) 
	{
		success = false;
		if(dot(c,c) <= s.w*s.w) 
		{
			success = true;
		}
		return;
	}

	t -= sqrt(s.w * s.w - p2);
	if ((t < ray.dis) && (t > 0))
	{
		success = true;
		return;
	}

	success = false;
	if(dot(c,c) <= s.w*s.w) {
		success = true;
	}
	return;
}

void IntersectPlane(in int p, inout Ray ray, out vec3 intersectionPoint, out bool success)
{
	float d = -dot(plane[p].center, plane[p].normal);
	float e = dot(ray.origin, plane[p].normal.xyz);
	float f = dot(ray.direction, plane[p].normal.xyz);
	float t = -(e + d) / f;

	if (t > 0 && t < ray.dis)
	{
		intersectionPoint = ray.origin + t * ray.direction;
		ray.dis = t;
		success = true;
		return;
	}

	intersectionPoint = vec3(0, 0, 0);
	success = false;
	return;
	
}

void IntersectTri(in int tt, inout Ray ray, out vec3 intersectionPoint, out bool success) 
{
	//same code as intersect plane
	float t = -(dot(ray.origin, tri[tt].p.normal.xyz) - dot(tri[tt].p.center, tri[tt].p.normal)) / dot(ray.direction, tri[tt].p.normal.xyz);

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

void GetColor(in int am, out vec4 col, in Ray primaryRay, out Ray refRay, out vec4 hitColor, inout float totdis) 
{
	vec3 hitPos;
	bool succ = false, isLight = false;
	vec3 rayCastHit;
	vec3 norm;
	float lightAngle;
	float lightw;

	col = vec4(0, 0, 0, 1);
	for(int ii=0;ii<plane.length();ii++)//intersect planes
	{
		bool suc;
		vec3 rayCasthit;
		IntersectPlane(ii, primaryRay, rayCasthit, suc);
		
		if (suc)
		{
			succ = true;
			hitColor = plane[ii].color;
			hitPos = rayCasthit;
			rayCastHit = rayCasthit;
			norm = plane[ii].normal.xyz;
			isLight = false;
		}
	}
	for(int ii=0;ii<tri.length();ii++)//intersect planes
	{
		if(ii == 8) 
		{
			bool b;
			IntersectSphere(vec4(-3.2f, tri[ii].v0.y+0.75f, -1.5f, -1.6f),primaryRay, b);
			if(!b) {
				ii+=11;
				continue;
			}
		}
		if(ii == 20) 
		{
			bool b;
			IntersectSphere(vec4(3.2f, tri[ii].v0.y+0.75f, -1.5f, -1.6f),primaryRay, b);
			if(!b) 
			{
				break;
			}
		}
		bool suc;
		vec3 rayCasthit;
		IntersectTri(ii, primaryRay, rayCasthit, suc);
		
		if (suc)
		{
			succ = true;
			hitColor = tri[ii].p.color;
			hitPos = rayCasthit;
			rayCastHit = rayCasthit;
			norm = tri[ii].p.normal.xyz;
			isLight = false;
		}
	}
	for(int i=0;i<sphere.length();i++)//intersect Spheres
	{
		if(activ[i]==0) 
		{
			continue;
		}
		bool suc;
		vec3 rayCasthit;
		IntersectSphere(sphere[i].pos, primaryRay, rayCasthit, suc);

		if (suc) 
		{
			succ = true;
			hitPos = sphere[i].pos.xyz;
			hitColor = sphere[i].color;
			rayCastHit = rayCasthit;
			norm = normalize(rayCastHit-hitPos.xyz);
		}
	}
	for(int i=0;i<light.length();i++)//intersect lights
	{
		if(activ[i + sphere.length()]==0) 
		{
			continue;
		}
		bool suc;
		vec3 rayCasthit;
		IntersectSphere(light[i].pos, primaryRay, rayCasthit, suc);

		if (suc) 
		{
					
			succ = true;
			hitPos = light[i].pos.xyz;
			hitColor = light[i].color;
			rayCastHit = rayCasthit;
			isLight = true;
			lightw = light[i].pos.w;
			lightAngle = dot(normalize(rayCasthit - primaryRay.origin), normalize(light[i].pos.xyz - rayCasthit));
		}
	}
	// Shoot shadow rays
	if(succ) 
	{
		if(isLight) {
			totdis++;
			col += hitColor * lightAngle / (lightw);
			hitColor = vec4(1, 1, 1, 0);
		}
			if(hitColor.w > 0) 
		{
			//reflect
			vec3 dir = primaryRay.direction - 2*dot(norm, primaryRay.direction)*norm;
			refRay = Ray(rayCastHit + dir*0.0001f, dir, 999999);
		}
		for(int j=0;j<light.length();j++)
		{
			if(activ[j + sphere.length()]==0) 
			{
				continue;
			}
			if(dot(norm, light[j].pos.xyz - rayCastHit) < 0) 
			{
				continue;
			}
			vec3 shadowOrigin = rayCastHit + normalize(norm) * 0.0001f;
			Ray shadowRay = Ray(shadowOrigin, normalize(light[j].pos.xyz - rayCastHit), length(light[j].pos.xyz - rayCastHit));
			bool intersectOther = false;

			vec4 lightval = (1-hitColor.w)*light[j].color * hitColor * dot(norm, normalize( light[j].pos.xyz - rayCastHit))/(shadowRay.dis*shadowRay.dis);
			if(dot(lightval, lightval) < 0.0001f) 
			{
				continue;
			}

			for(int k=0;k<sphere.length();k++)
			{
				vec3 notimp;
				if(activ[k] == 0) 
				{
					continue;
				}
									
				IntersectSphere(sphere[k].pos, shadowRay, notimp, intersectOther);

				if (intersectOther)
				{
					break;
				}
			}
			if (intersectOther)
			{
				continue;
			}
			for(int k=0;k<tri.length();k++)
			{
				if(k == 8) 
				{
					bool b;
					IntersectSphere(vec4(-3.2f, tri[k].v0.y+0.75f, -1.5f, -1.6f),primaryRay, b);
					if(!b) {
						k+=11;
						continue;
					}
				}
				if(k == 20) 
				{
					bool b;
					IntersectSphere(vec4(3.2f, tri[k].v0.y+0.75f, -1.5f, -1.6f),primaryRay, b);
					if(!b) 
					{
						break;
					}
				}
				vec3 notimp;
								
				IntersectTri(k, shadowRay, notimp, intersectOther);
				if (intersectOther)
				{
					break;
				}
			}
			if (!intersectOther)
			{
				col += lightval;
			}
			for(int k=0;k<plane.length();k++)
			{
				vec3 notimp;
								
				IntersectPlane(k, shadowRay, notimp, intersectOther);
				if (intersectOther)
				{
					break;
				}
			}
			if (intersectOther)
			{
				continue;
			}
		}	
		totdis = primaryRay.dis;
	}
}

void main() 
{
	ivec2 storePos = ivec2(gl_GlobalInvocationID.xy);
	uint gWidth = gl_WorkGroupSize.x * gl_NumWorkGroups.x;
	uint gHeight = gl_WorkGroupSize.y * gl_NumWorkGroups.y;
	uint offset = storePos.y * gl_WorkGroupSize.x * gl_NumWorkGroups.x + storePos.x;
	//light[offset].pos.z += 0.001;


	//(center + new Vector3(-1, -1, 0))
    //(center + new Vector3( 1, -1, 0))
    //(center + new Vector3(-1,  1, 0))

	Color[offset] = vec4(0, 0, 0, 1);
	vec4 cc = vec4(0, 0, 0, 1);
	
	for(int g = 0; g < aa; g++) {
		for(int h = 0; h < aa; h++) {
			vec3 pixel = vec3(screenTL.x+(screenTR.x-screenTL.x)*(storePos.x+g/float(aa))/gWidth, screenTL.y+(screenDL.y-screenTL.y)*(storePos.y+h/float(aa))/gHeight, screenTL.z);
			Ray primaryRay = Ray(camPos, normalize(pixel - camPos), 999999);
			vec4 hitColor = vec4(1, 1, 1, 1);
			int am = 0;
			float atot = 1;
			Color[offset] = vec4(0, 0, 0, 0);
			float totdis = 0;
			while(atot > 0 && am < bounceLimit+1) 
			{
				vec4 nc;
				Ray r;
				vec4 prev = hitColor;
				GetColor(0, nc, primaryRay, primaryRay, hitColor, totdis);
				nc = clamp(nc, vec4(0, 0, 0, 0), vec4(1, 1, 1, 1));
				Color[offset] += clamp(nc*atot*prev, vec4(0, 0, 0, 0), vec4(1, 1, 1, 1));
				atot *= hitColor.w;
				am++;
			}
			clamp(Color[offset], 0, 1);
			cc += Color[offset]/(aa*aa);
		}
	}
	
	imageStore(img, storePos, vec4(cc.xyz, 1));
}