Shader "Unlit/BeamableSDF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SecondaryTexture ("Background", 2D) = "white" {}
        
        [Toggle(MULTISAMPLING)] _SDF_MULTISAMPLING("Multisampling", Float) = 1
        _SDF_SamplingDistance("Sampling Distance", Range(0, .1)) = .01
        
        [HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask("ColorMask", Float) = 15
    }
    SubShader
    {
        LOD 100

        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }
        
        ColorMask RGBA

        Pass
        {
            ZWrite Off
            ZTest[unity_GUIZTestMode]
            Cull Back
            AlphaTest Greater .01
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment MULTISAMPLING
            #pragma multi_compile _BACKGROUND_TEX_AS_MAIN _BACKGROUND_TEX_AS_MAIN_NEG
            #pragma multi_compile_fragment _MODE_DEFAULT _MODE_RECT
            #pragma multi_compile_fragment _SHADOWMODE_DEFAULT _SHADOWMODE_INNER
            #pragma multi_compile_fragment _BGMODE_DEFAULT _BGMODE_OUTLINE _BGMODE_FULL

            #include "UnityCG.cginc"
            #include "Packages\com.beamable\Runtime\UI\Scripts\Sdf\SDFFunctions.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 backgroundUV : TEXCOORD1;
                float2 params : TEXCOORD2;
                float2 coords : TEXCOORD3;
                float4 color : COLOR;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 sizeNCoords : TEXCOORD1;
                float3 roundingNThresholdNOutlineWidth : TEXCOORD3;
                float3 shadowOffset : TEXCOORD4;
                float4 shadowColor : TEXCOORD5;
                float shadowSoftness : TEXCOORD2;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 outlineColor : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _SecondaryTexture;
            float4 _SecondaryTexture_ST;
            
            float _SDF_SamplingDistance;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(float3(v.vertex.x, v.vertex.y, 0));
                #if _BACKGROUND_TEX_AS_MAIN
                o.uv.xy = TRANSFORM_TEX(v.uv, _SecondaryTexture);
                o.uv.zw = TRANSFORM_TEX(v.backgroundUV, _MainTex);
                #else
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.backgroundUV, _SecondaryTexture);
                #endif
                o.color = v.color;
                o.color.a *= step(.0045, o.color.a); // hack to avoid object disappear when alpha is equal to zero (see SDFUtile.ClipColorAlpha)
                o.sizeNCoords.xy = v.normal.yz;
                o.sizeNCoords.zw = v.coords;
                o.outlineColor.rgb = floatToRGB(v.params.y);
                o.roundingNThresholdNOutlineWidth.x = v.vertex.z;
                o.roundingNThresholdNOutlineWidth.y = v.normal.x;
                o.roundingNThresholdNOutlineWidth.z = v.params.x;
                o.shadowColor.rgb = floatToRGB(v.tangent.w);
                float3 tangentZ = floatToRGB(v.tangent.z);
                o.outlineColor.a = tangentZ.x;
                o.shadowColor.a = tangentZ.y;
                o.shadowSoftness = v.tangent.x;
                o.shadowOffset.xy = floatToRG(v.tangent.y);
                o.shadowOffset.z = (tangentZ.z - .5) * 100;
                return o;
            }

            float4 sampleSDFTexture(float2 uv)
            {
                #if _BACKGROUND_TEX_AS_MAIN
                return tex2D(_SecondaryTexture, uv);
                #else
                return tex2D(_MainTex, uv);
                #endif
            }

            float4 sampleBackgroundTexture(float2 uv)
            {
                #if _BACKGROUND_TEX_AS_MAIN
                return tex2D(_MainTex, uv);
                #else
                return tex2D(_SecondaryTexture, uv);
                #endif
            }
            
            // return full rect distance
            float getRectDistance(float2 coords, float2 size, float rounding){
                coords = coords - float2(.5, .5);
                coords *= size;
                float rectDist = sdfRoundedRectangle(coords, size * .5, rounding);
                return rectDist;
            }

            // returns SDF image distance
            float getDistance(float2 uv, float2 coords, float2 size, float rounding){
                float dist = sampleSDFTexture(uv).a;
                #if MULTISAMPLING
                dist += sampleSDFTexture(float2(uv.x - _SDF_SamplingDistance, uv.y - _SDF_SamplingDistance)).a;
                dist += sampleSDFTexture(float2(uv.x + _SDF_SamplingDistance, uv.y - _SDF_SamplingDistance)).a;
                dist += sampleSDFTexture(float2(uv.x - _SDF_SamplingDistance, uv.y + _SDF_SamplingDistance)).a;
                dist += sampleSDFTexture(float2(uv.x + _SDF_SamplingDistance, uv.y + _SDF_SamplingDistance)).a;
                dist += sampleSDFTexture(float2(uv.x - _SDF_SamplingDistance, uv.y)).a;
                dist += sampleSDFTexture(float2(uv.x + _SDF_SamplingDistance, uv.y)).a;
                dist += sampleSDFTexture(float2(uv.x, uv.y + _SDF_SamplingDistance)).a;
                dist += sampleSDFTexture(float2(uv.x, uv.y + _SDF_SamplingDistance)).a;
                dist /= 9;
                #endif
                dist = .5 - dist;
                return dist;
            }
            
            // returns intersection of SDF image distance and rect distance
            float getMergedDistance(float2 uv, float2 coords, float2 size, float rounding){
            #if _MODE_RECT
                return getRectDistance(coords, size, rounding);
            #else
                return getDistance(uv, coords, size, rounding);
            #endif
            }
            
            // returns SDF value with given threshold
            float calculateValue(float dist, float threshold){
                return 1 - aaStep(threshold, dist);
            }
            
            // returns main color
            float4 mainColor(v2f i){
                float4 color = i.color;
                #if _BGMODE_DEFAULT || _BGMODE_FULL
                color *= sampleBackgroundTexture(i.uv.zw);
                #endif
                return color;
            }
            
            float4 outlineColor(v2f i){
                float4 color = i.outlineColor;
                #if _BGMODE_OUTLINE || _BGMODE_FULL
                color *= sampleBackgroundTexture(i.uv.zw);
                #endif
                return color;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 size = i.sizeNCoords.xy;
                float2 coords = i.sizeNCoords.zw;
                float rounding = i.roundingNThresholdNOutlineWidth.x;
                float threshold = i.roundingNThresholdNOutlineWidth.y;
                float outlineWidth = i.roundingNThresholdNOutlineWidth.z;
                
                // Main color
                float dist = getMergedDistance(i.uv.xy, coords, size, rounding);
                float mainColorValue = calculateValue(dist, threshold);
                float4 main = mainColor(i);
                
                // Outline
                float outlineValue = calculateValue(dist, threshold + outlineWidth) - calculateValue(dist, threshold);
                float4 outline = outlineColor(i);
                outline.a *= outlineValue;
                main.a *= mainColorValue;
                // Blending main and outline
                float4 final = blend_cutIn(outline, main, mainColorValue);
                
                // Shadow
                #if _SHADOWMODE_DEFAULT
                
                float shadowDist = getMergedDistance(i.uv - i.shadowOffset.xy, coords - (i.shadowOffset.xy / size), size, rounding);
                float shadowEdge = threshold + outlineWidth + i.shadowOffset.z;
                float shadowValue = 1 - smoothstep(shadowEdge - i.shadowSoftness, shadowEdge, shadowDist);
                i.shadowColor.a *= shadowValue;
                final = blend_overlay(i.shadowColor, final);
                
                #elif _SHADOWMODE_INNER
                
                float shadowDist = getMergedDistance(i.uv - i.shadowOffset.xy, coords - (i.shadowOffset.xy / size), size, rounding);
                float shadowEdge = threshold + -i.shadowOffset.z;
                float shadowValue = smoothstep(shadowEdge, shadowEdge + i.shadowSoftness, shadowDist) * saturate(mainColorValue / outlineValue);
                i.shadowColor.a *= shadowValue;
                final = blend_overlay(final, i.shadowColor);
                
                #endif
                
                return final;
            }
            ENDCG
        }
    }
}
