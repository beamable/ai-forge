Shader "Unlit/De_Panel_Loader_Unlit"
{
    Properties
    {

        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color: COLOR0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float constrainAngle(float x){
                x = fmod(x + 3.14,6.28);
                if (x < 0.)
                    x += 6.28;
                return x - 3.14;
            }

            float angleDiff(float a,float b){
                a = constrainAngle(a);
                b = constrainAngle(b);
                float dif = fmod(b - a + 3.14,6.28);
                if (dif < 0.)
                    dif += 6.28;
                return abs(dif);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = 1;

                float2 dir = i.uv - .5;
                float radius = length(dir);
                float angle = atan2(dir.y, dir.x);

                float circleMask = smoothstep(.5, .49, radius) - smoothstep(.41, .4, radius);

                float t = frac(_Time.x) * 6.28;
                float sweepSpeed = 4;
                float sweepSize = .3;
                float sweepTol = .01;
                float sweep = -sweepSpeed * t;

                float sweepDist = angleDiff(angle, sweep) / 6.28;
                float angleMask = (smoothstep(0, sweepTol, sweepDist) - smoothstep(sweepTol + sweepSize, 2*sweepTol + sweepSize, sweepDist));

                col = 1;
                col *= circleMask;

                col *= angleMask;
                col *= i.color;

                return col;
            }
            ENDCG
        }
    }
}
