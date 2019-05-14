#version 430
layout(std430, binding=0) writeonly buffer Pos{
    vec3 Position[];
};

layout(std430, binding=1) readonly buffer spheres{
    vec4 sphere[];
};

layout (local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

void main() 
{
	ivec2 storePos = ivec2(gl_GlobalInvocationID.xy);
	float gWidth = gl_WorkGroupSize.x * gl_NumWorkGroups.x;
	float gHeight = gl_WorkGroupSize.y * gl_NumWorkGroups.y;
	uint offset = storePos.y * gl_WorkGroupSize.x * gl_NumWorkGroups.x + storePos.x;
	Position[offset] = vec3(storePos.x/gWidth, storePos.y/gHeight, (storePos.x+storePos.y-2)/(gWidth+gHeight));
	for(int i=0;i<sphere.length();i++){
		if(sphere[i].x == storePos.x && sphere[i].y == storePos.y) {
			Position[offset] = vec3(1, 1, 1);
		}
	}
}

struct Ray 
{
	vec3 origin;
	vec3 direction;
	float dis;
};

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