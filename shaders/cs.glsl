#version 430
layout(std430, binding=0) writeonly buffer Pos{
    vec3 Position[];
};

layout(std430, binding=1) writeonly buffer spheres{
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