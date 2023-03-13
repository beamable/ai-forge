Shader "Custom/ColorOverlayShader"
{
    Properties
    {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
        }

		Cull Off
		Lighting Off
		ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"

            struct appdata_custom
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                fixed2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                fixed2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _MainTex_ST;

            v2f vert(appdata_custom v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                fixed4 diffuse = tex2D(_MainTex, i.uv);
                fixed luminance = dot(diffuse, fixed4(0.2126, 0.7152, 0.0722, 0));
                fixed oldAlpha = diffuse.a;

                if (luminance < 0.5)
                    diffuse *= 2 * i.color;
                else
                    diffuse = 1 - 2 * (1 - diffuse) * (1 - i.color);

                diffuse.a = oldAlpha * i.color.a;
                return diffuse;
            }
            ENDCG
        }
    }
    Fallback off
}