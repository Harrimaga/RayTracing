#version 430
layout(std430, binding=0) writeonly buffer Pos{
    vec3 Position[];
};

layout (local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

void main() 
{
	ivec2 storePos = ivec2(gl_GlobalInvocationID.xy);
	uint gWidth = gl_WorkGroupSize.x * gl_NumWorkGroups.x;
	uint offset = storePos.y * gWidth + storePos.x;
	Position[offset] = vec3(1, 1, 1); 	
}