FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
    Forward();
    Depth();
}

COMMON
{
	#include "common/shared.hlsl"
}

struct VS_INPUT
{
	#include "common/vertexinput.hlsl"
};

struct PS_INPUT
{
	#include "common/pixelinput.hlsl"
};

VS
{
    #include "common/vertex.hlsl"
    
    PixelInput MainVs( VertexInput i)
    {
        PixelInput o = ProcessVertex(i);
        return FinalizeVertex(o);
    }
}

PS
{
	#define CUSTOM_MATERIAL_INPUTS 1
	#include "common/pixel.hlsl"

	Texture2D g_AtlasTexture < Attribute( "AtlasTexture" ); SrgbRead( true ); >;
	float2 g_AtlasSize < Attribute( "AtlasSize" ); >;
	float4 g_TexturePageRect < Attribute( "TexturePageRect" ); >;

	RenderState( CullMode, NONE );
	RenderState( DepthWriteEnable, true );
	RenderState( BlendEnable, true );

	float4 MainPs( PS_INPUT i ) : SV_Target0
	{
		float2 offset = g_TexturePageRect.xy / g_AtlasSize.xy;
		float2 size = g_TexturePageRect.zw / g_AtlasSize.xy * i.vTextureCoords.xy;
		// Calculate the offset plus size in UV coordinates.
		float2 uv = offset + size;
		float4 col = g_AtlasTexture.Sample( g_sPointClamp, uv.xy );
		return col * i.vVertexColor;
	}
}
