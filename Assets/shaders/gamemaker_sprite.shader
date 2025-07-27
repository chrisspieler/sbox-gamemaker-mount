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
	#include "common/classes/Bindless.hlsl"
	#include "common/pixel.hlsl"

	int g_iAtlasTextureIndex < Attribute( "g_iAtlasTextureIndex" ); SrgbRead( true ); >;
	float2 g_vAtlasSize < Attribute( "g_vAtlasSize" ); Default2( 2048, 2048 );>;
	float4 g_vTexturePageRect < Attribute( "g_vTexturePageRect" ); Default4( 64, 64, 64, 64 ); >;

	RenderState( CullMode, NONE );
	RenderState( DepthWriteEnable, true );
	RenderState( BlendEnable, true );

	float4 MainPs( PS_INPUT i ) : SV_Target0
	{
		float2 offset = g_vTexturePageRect.xy / g_vAtlasSize.xy;
		float2 size = g_vTexturePageRect.zw / g_vAtlasSize.xy * i.vTextureCoords.xy;
		// Calculate the offset plus size in UV coordinates.
		float2 uv = offset + size;
		Texture2D atlasTexture = Bindless::GetTexture2D( g_iAtlasTextureIndex, true );
		float4 col = atlasTexture.Sample( g_sPointClamp, uv.xy );
		return col * i.vVertexColor;
	}
}
