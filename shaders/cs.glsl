#version 430
layout(std430, binding=0) writeonly buffer Pos{
    vec4 Color[];
};

struct Sphere
{
	vec4 pos;
	vec4 color;
};

struct Camera
{
	vec3 camPos;
	vec3 camDir;
	vec3 screenCenter;
	vec3 screen[3];
};

struct Ray 
{
	vec3 origin;
	vec3 direction;
	float dis;
};

layout(std430, binding=1) readonly buffer spheres{
    Sphere sphere[];
};

layout(std140) uniform camera_data {
	Camera camera;
};

layout (local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

void IntersectSphere(in vec4 s, in Ray ray, out vec3 intersectionPoint, out bool success)
{
	vec3 c = s.xyz - ray.origin;
	float t = dot(c, ray.direction);
	vec3 q = c - t * ray.direction;
	float p2 = dot(q, q);

	if (p2 > s.w * s.w) 
	{
		intersectionPoint = vec3(0, 0, 0);
		success = false;
		return;
	}

	t -= sqrt(s.w * s.w - p2);
	intersectionPoint = ray.origin + t * ray.direction;
	success = true;
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
	vec3 pixel = vec3(camera.screen[0].x+(camera.screen[1].x-camera.screen[0].x)*storePos.x/gWidth, camera.screen[0].y+(camera.screen[1].y-camera.screen[0].y)*storePos.y/gWidth, camera.screen[0].z);

	Ray primaryRay = Ray(camera.camPos, normalize(pixel - camera.camPos), 999999);

	for(int i=0;i<sphere.length();i++)
	{
		bool suc;
		vec3 rayCastHit;
		IntersectSphere(sphere[i].pos, primaryRay, rayCastHit, suc);

		if (suc) 
		{
			Color[offset] = sphere[i].color;
		}
	}
}