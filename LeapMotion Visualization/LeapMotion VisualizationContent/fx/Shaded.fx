float4x4 W;
float4x4 VP;
float3 lightDir = float3(0,.25,.25);
float4 ambient = float4(.5,.5,.5,0);
float alpha = 1;

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
	output.lightFactor = dot(normalize(norm), -normalize(lightDir));
	output.lightFactor *= output.lightFactor;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = float4(1,1,1,alpha);
	color.rbg *= saturate(input.lightFactor) + ambient;
	color.a = alpha;
    return color;
}

technique Technique1
{
    pass Shaded
    {
		AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
