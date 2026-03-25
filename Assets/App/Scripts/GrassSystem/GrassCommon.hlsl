struct GrassData
{
	float3 position;
};

StructuredBuffer<GrassData> grassDataBuffer;

void GetGrassPosition_float(uint ID, out float3 position)
{
	position = grassDataBuffer[ID].position;
}

