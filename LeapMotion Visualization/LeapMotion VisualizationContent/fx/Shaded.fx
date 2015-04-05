float4x4 W;
float4x4 VP;
float3 lightDir = float3(1,0,0);
float4 ambient = float4(.25,.25,.25,1);

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float4 lightFactor : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	
    float4 w = mul(input.Position, W);
    output.Position = mul(w, VP);
	output.Color = input.Color;
	float3 norm = mul(input.Normal,W);
	output.lightFactor = dot(norm, -lightDir) * 2;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = input.Color;
	color.rbg *= saturate(input.lightFactor) + ambient;
    return color;
}

technique Technique1
{
    pass Shaded
    {
        AlphaBlendEnable = FALSE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
