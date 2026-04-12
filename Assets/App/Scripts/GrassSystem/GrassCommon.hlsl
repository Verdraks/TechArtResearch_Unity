#ifndef GrassCommon_INCLUDED
#define GrassCommon_INCLUDED
struct GrassData
{
	float3 position;
};

#ifndef SHADERGRAPH_PREVIEW
StructuredBuffer<GrassData> grassDataBuffer;
#endif

void GetGrassPosition_float(uint ID, out float3 position)
{
	#ifndef SHADERGRAPH_PREVIEW
	position = grassDataBuffer[ID].position;
	#else
	position = float3(0, 0, 0);
	#endif
}
#endif

#ifndef UNITY_INDIRECT_DRAW_ARGS
#define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
#include "UnityIndirect.cginc"
#endif

#ifndef RENDER_MESH_INDIRECT_EXAMPLE
#define RENDER_MESH_INDIRECT_EXAMPLE

uniform float4x4 _ObjectToWorld;


void CommandID_IndirectInstanceCount_float(out uint CommandID, out uint IndirectInstanceCount)
{
	#ifndef SHADERGRAPH_PREVIEW
	InitIndirectDrawArgs(0);
	CommandID = GetCommandID(0);
	IndirectInstanceCount = GetIndirectInstanceCount();
	#else
	CommandID = 0;
	IndirectInstanceCount = 1;
	#endif
}
#endif

