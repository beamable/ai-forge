Shader "Unlit/AvatarElementUnlit"
{
    Properties
    {
        _Spread ("Spread", Range(0, 1)) = .014
        _BaseAlpha ("Base Alpha", Range(0, 1)) = .1
        _Offset ("Offset", Range(-.5, .5)) = -.03

        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
         _Color ("Tint", Color) = (1,1,1,1)

         // required for UI.Mask
         _StencilComp ("Stencil Comparison", Float) = 8
         _Stencil ("Stencil ID", Float) = 0
         _StencilOp ("Stencil Operation", Float) = 0
         _StencilWriteMask ("Stencil Write Mask", Float) = 255
         _StencilReadMask ("Stencil Read Mask", Float) = 255
         _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

        // required for UI.Mask
         Stencil
         {
             Ref [_Stencil]
             Comp [_StencilComp]
             Pass [_StencilOp]
             ReadMask [_StencilReadMask]
             WriteMask [_StencilWriteMask]
         }
          ColorMask [_ColorMask]

         LOD 100

         ZWrite Off
         Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color  : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Spread;
            float _BaseAlpha;
            float _Offset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);


                float width = i.color.g;
                float xPercentage = i.color.r;
                float progress = i.color.b;

                if (width > .99) return col;

                xPercentage = clamp( xPercentage, 0, 1);
                float x1 = xPercentage + i.uv.x * width;

                float minAlpha = _BaseAlpha;
                float offset = _Offset;
                float spread = _Spread;

                float leftMask = smoothstep(width + offset, width + offset + spread, x1);
                leftMask = lerp(1, leftMask, clamp( progress / width, 0, 1));

                offset += -width + .01;
                float rightMask = smoothstep(width + offset, width + offset + spread, 1-x1);
                rightMask = lerp(1, rightMask, clamp( (2* width) + (1 - ((progress + -4*width) / width)), 0, 1));

                float mask = minAlpha + (rightMask * leftMask * (1 - minAlpha));
                return float4(col.rgb, mask);
            }
            ENDCG
        }
    }
}
