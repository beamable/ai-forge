//The MSDF_Standard CGINC contains the stock vert/frag
//for MSDF type UI shaders

#include "MSDF_VertFrag.cginc"
#include "UnityCG.cginc"


//Variables that must be specifically calibrated for this
//shader...  This allows you to use different texture
//combinations (using the sprite as the icon or the shape)

#if _MSDF_FLIP_PRIMARY_UV
float _PrimaryUVFlipX;
float _PrimaryUVFlipY;
#endif

float4 _TextureTex_ST;
float _XScroll;
float _YScroll;

#if _MSDF_DROPSHADOW
float _ShadowOffsetX;
float _ShadowOffsetY;
#endif

#if _MSDF_BACKGROUND
sampler2D _BackgroundTex;
float4 _BackgroundTex_ST;
float _BackgroundTex_TexelSize;
#endif

#if _MSDF_SHINE
float _ShineAngle;
#endif

#if _MSDF_ICON
sampler2D _Icon;
uniform float4 _Icon_ST;
#endif

#if _MSDF_SECONDARYSHAPE
sampler2D _Secondary;
float4 _Secondary_ST;
#endif

float4 _SpriteOffset;


v2f vert(appdata_full v)
{
    Input input;

    input.shape_ST = _MainTex_ST;
    input.overlay_ST = _TextureTex_ST;

#if _MSDF_SHINE
    input.shineAngle = _ShineAngle;
#endif
#if _MSDF_BACKGROUND
    input.bg_ST = _BackgroundTex_ST;
    input.texel = _BackgroundTex_TexelSize;
#endif
#if _MSDF_UV2_SCROLLING
    input.xScroll = _XScroll;
    input.yScroll = _YScroll;
#endif
#if _MSDF_DROPSHADOW
    input.shadowX = _ShadowOffsetX;
    input.shadowY = _ShadowOffsetY;
#endif
#if _MSDF_ICON
    input.icon_ST = _Icon_ST;
#endif

    v2f o = VertexFunction(input, v);
    return o;
}

fixed4 frag(v2f v) : SV_Target
{
    //declare these
    float4 shapeSample = float4(1.0,1.0,1.0,1.0);
    float4 iconSample = float4(1.0,1.0,1.0,1.0);
    float4 shadowSample = float4(1.0,1.0,1.0,1.0);
    float4 bgSample = float4(1.0, 1.0, 1.0, 1.0);
    float4 secondarySample = float4(1.0, 1.0, 1.0, 1.0);

    float2 primaryUV = v.uvOne.xy;

   #if _MSDF_FLIP_PRIMARY_UV
       float2 t = frac(primaryUV * 0.5) * 2.0;
       primaryUV = saturate(float2(1.0, 1.0) - abs(t - float2(_PrimaryUVFlipX, _PrimaryUVFlipY)));
   #endif

    //sample what we need
    shapeSample = SampleMainTex(primaryUV);

    #if _MSDF_ICON
        iconSample = tex2D(_Icon, v.uvThree.zw);
    #endif

   #if _MSDF_BACKGROUND
        bgSample = tex2D(_BackgroundTex, v.uvTwo.zw);
   #endif

    #if _MSDF_DROPSHADOW
        shadowSample = SampleMainTex(v.uvThree.xy);
    #endif

        secondarySample = shapeSample;
    #if _MSDF_SECONDARYSHAPE_CUSTOMTEX
        secondarySample = tex2D(_Secondary, TransformUV(v.uvOne.zw, _Secondary_ST));
    #endif

    //run fragment
    fixed4 result = FragmentProgram(v,shapeSample,iconSample, bgSample, shadowSample, secondarySample);

    return result;
}