float4x4 MatrixTransform;
float depth=0;
float2 cursorPos;

void vertshader(inout float4 color    : COLOR0,
                        inout float2 texCoord : TEXCOORD0,
                        inout float4 position : SV_Position)
{
    position = mul(position, MatrixTransform);
}

float hash( float n )
{
    return frac(sin(n)*43758.5453);
}

float noise3d(float3 x)
{
    float3 p = floor(x);
    float3 f = frac(x);

    f       = f*f*(3.0-2.0*f);
    float n = p.x + p.y*57.0 + 113.0*p.z;

    return lerp(lerp(lerp( hash(n+0.0), hash(n+1.0),f.x),
                   lerp( hash(n+57.0), hash(n+58.0),f.x),f.y),
               lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
                   lerp( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
}

float4 noiseshader(float2 coords : TEXCOORD0) : COLOR0
{  
   float r = (noise3d(float3(coords*5, (cursorPos.x+cursorPos.y) / 500))+1) / 2;
   return float4(0,0,0,r);
}

technique noisetechnique
{
    pass noisepass
    {
		VertexShader = compile vs_3_0 vertshader();
        PixelShader = compile ps_3_0 noiseshader();
    }
}
