// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//The VertFrag CGINC contains the vertex and fragment programs
//in function format so they can be called simply by rearranging
//inputs...

#include "MSDF.cginc"
#include "UnityCG.cginc"


//Common variables used universally by each of the MSDF
//shaders....

sampler2D _MainTex;
float4 _MainTex_ST;

float _Threshold;
float4 _Color;
float _PrimaryErosion;
float _PrimaryDissolve;

float _OutlineThreshold;
float4 _OutlineColor;
float _StrokeErosion;
float _StrokeDissolve;

float _Softness;

sampler2D _TextureTex;
float _uvScrolling;

float4 _TextureMix;
float _TextureImpact;
float _TextureAlpha;

float _Erosion;
float _Dissolve;

float _background;
#if _MSDF_BACKGROUND
float4 _BackgroundMix;
float _BackgroundContrast;
#endif
#if _MSDF_BACKGROUND_BLEND_LERP
float4 _BackgroundColorZero;
float4 _BackgroundColorOne;
#endif

float _dropShadow;
#if _MSDF_DROPSHADOW
float _ShadowBoost;
float4 _ShadowOffset;
float _ShadowThreshold;
float4 _ShadowColor;
float _ShadowSoftness;
#endif

float _centerGlow;
#if _MSDF_CENTERGLOW
float _GlowBoost;
float _GlowThreshold;
float4 _GlowColor;
float _GlowSoftness;
#endif

float _innerGlow;
#if _MSDF_INNERGLOW
float _InnerGlowBoost;
float _InnerGlowThreshold;
float4 _InnerGlowColor;
float _InnerGlowSoftness;
#endif

float _outterGlow;
#if _MSDF_OUTTERGLOW
float _OutterGlowBoost;
float _OutterGlowThreshold;
float4 _OutterGlowColor;
float _OutterGlowSoftness;
#endif

float _gradientHorizontal;
#if _MSDF_GRADIENT_HORIZONTAL
float4 _GradientHorizontalZeroColor;
float4 _GradientHorizontalOneColor;
float _GradientHorizontalContrast;
float _GradientHorizontalOffset;
#endif

float _gradientVertical;
#if _MSDF_GRADIENT_VERTICAL
float4 _GradientVerticalZeroColor;
float4 _GradientVerticalOneColor;
float _GradientVerticalContrast;
float _GradientVerticalOffset;
#endif

float _gradientRadial;
#if _MSDF_GRADIENT_RADIAL
float4 _GradientRadialZeroColor;
float4 _GradientRadialOneColor;
float _GradientRadialSize;
float _GradientRadialContrast;
#endif

float _shine;
#if _MSDF_SHINE
float4 _ShinePicker;
float4 _ShineColor;
float _ShineWidth;
float _ShineSpeed;
float _ShineFrequency;
float _ShineBoost;
float _ShineImpactPrimary;
float _ShineImpactPrimaryStroke;
#endif

float _secondaryShape;
#if _MSDF_SECONDARYSHAPE
float _SecondaryThreshold;
float _SecondaryOutterThreshold;
float4 _SecondaryPicker;
float4 _SecondaryColor;
float _SecondaryErosion;
float _GradientImpactPrimary;
float _GradientImpactSecondary;
float _ShineImpactSecondary;
float _SecondaryTextureImpact;
   #if _MSDF_SECONDARYSHAPE_FILL_HORIZONTAL
   float _SecondaryShapeFill;
   float _SecondaryShapeFillContrast;
   #endif
   #if _MSDF_SECONDARYSHAPE_FILL_RADIAL
   float _SecondaryShapeFill;
   float _SecondaryShapeFillContrast;
   float _SecondaryShapeRotation;
   #endif
#endif
#if _MSDF_SECONDARYSHAPE_OUTTERGLOW
float _SecondaryOutterGlowBoost;
float _SecondaryOutterGlowThreshold;
float4 _SecondaryOutterGlowColor;
float _SecondaryOutterGlowSoftness;
#endif

#if _MSDF_ICON
float _IconThreshold;
float4 _IconColor;
float _IconStrokeThreshold;
float4 _IconStrokeColor;
float _GradientImpactIcon;
float _ShineImpactIcon;
#endif

#if _MSDF_ICON_ROTATE_UV
float _IconUVRotation;
float _IconPivotX;
float _IconPivotY;
#endif

#if _MSDF_ROTATE_PRIMARY_UV
float _PrimaryUVRotation;
float _PrimaryPivotX;
float _PrimaryPivotY;
#endif

float _Greyscale;


//input struct passed to the Vert function
struct Input
{
    float4 shape_ST;
    float4 overlay_ST;

    #if _MSDF_SHINE
        float shineAngle;
    #endif
    #if _MSDF_BACKGROUND
        float4 bg_ST;
        float2 texel;
    #endif
    #if _MSDF_UV2_SCROLLING
        float xScroll;
        float yScroll;
    #endif
    #if _MSDF_DROPSHADOW
        float shadowX;
        float shadowY;
    #endif
    #if _MSDF_ICON
        float4 icon_ST;
    #endif
};

//struct passed to the Frag function
struct v2f
{
    float4 pos : SV_POSITION;
    float4 uvOne : TEXCOORD0; //xy = MainTex UVs  //zw = ShineUVs if needed
    fixed4 color : COLOR;
    float4 uvTwo : TEXCOORD1; //xy = overlayUVs  //zw = BackgroundUVs
    float4 uvThree : TEXCOORD2; //xy = shadowUVs  zw = IconUVs
    float2 unModUV : TEXCOORD3;
};


//function called in the vertex program
v2f VertexFunction(Input i, appdata_full v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);

    o.uvOne.xyzw = float4(1.0, 1.0, 1.0, 1.0);
    o.uvOne.zw = v.texcoord1.xy;
    o.uvOne.xy = TransformUV(v.texcoord.xy, i.shape_ST);

    #if _MSDF_ROTATE_PRIMARY_UV
        o.uvOne.xy = ComputePivotRotation(o.uvOne.xy, _PrimaryUVRotation, float2(_PrimaryPivotX, _PrimaryPivotY));
    #endif

    o.color = v.color;

    #if _MSDF_SHINE
        float2 shineUV = float2(0.0,0.0);

        #if _MSDF_SHINE_UV_SCREEN
            shineUV = ComputeScreenUV(o.pos, i.overlay_ST);
        #elif _MSDF_SHINE_UV_POSITION
            shineUV = saturate(v.texcoord1.xy);
        #endif

        o.uvOne.zw = ComputeRotatedUV(shineUV, i.shineAngle);
    #endif

    o.uvTwo = saturate(v.texcoord1.xyxy);
    float2 unmodPos = saturate(v.texcoord1.xy);
    o.unModUV = unmodPos;

    #if _MSDF_UV2_SCREEN
        o.uvTwo.xy = ComputeScreenUV(o.pos, i.overlay_ST);
    #elif _MSDF_UV2_POSITION
        o.uvTwo.xy = TransformUV(unmodPos, i.overlay_ST);
    #endif

    #if _MSDF_BACKGROUND_UV2_SCREEN
            o.uvTwo.zw = ComputeScreenUV(o.pos, i.bg_ST);
    #elif _MSDF_BACKGROUND_UV2_POSITION
            o.uvTwo.zw = TransformUV(unmodPos, i.bg_ST);
    #endif

    #if _MSDF_UV2_SCROLLING
        o.uvTwo.xy = ScrollUV(o.uvTwo,i.xScroll,i.yScroll);
    #endif

   o.uvThree.xyzw = float4(0.0, 0.0, 0.0, 0.0);

   #if _MSDF_DROPSHADOW
      o.uvThree.xy = o.uvOne.xy + (float2(i.shadowX,i.shadowY) * 0.01.xx);
   #endif

   #if _MSDF_ICON
      o.uvThree.zw = TransformUV(unmodPos, i.icon_ST);

   #endif
   #if _MSDF_ICON_ROTATE_UV
      o.uvThree.zw = ComputePivotRotation(o.uvThree.zw, _IconUVRotation, float2(_IconPivotX, _IconPivotY));
   #endif
    return o;
}

// Samples the main texture with an optional bias
fixed4 SampleMainTex(float2 coord)
{
   #ifdef _MSDF_BIAS_MAINTEX
      return tex2Dbias(_MainTex, float4(coord.xy,0.0,(_MSDF_BIAS_MAINTEX)));
   #else
      return tex2D(_MainTex, coord.xy);
   #endif
}

fixed4 FragmentProgram(v2f v, float4 shape, float4 icon, float4 bgSample, float4 shadowSample, float4 secondarySample)
{
    fixed4 result;

    //sample textures
    float3 dist = shape.rgb;
    float4 tex = tex2D(_TextureTex,v.uvTwo.xy);

    //blend our overlay texture
    float mixTex = saturate(MixOverlay(tex.rgba, _TextureMix));
    float dissolve = Dissolve(mixTex, _Dissolve);

    //get thresholds
    float bodyAlpha = BasicMSDF(dist, mixTex , _Threshold, _PrimaryErosion, _Softness);
    float bodyDissolve = Dissolve(mixTex, _PrimaryDissolve);
    bodyAlpha = saturate(bodyAlpha * bodyDissolve);

    float outlineAlpha = BasicMSDF(dist, mixTex, _OutlineThreshold, _StrokeErosion, _Softness);
    float outlineDissolve = Dissolve(mixTex, _StrokeDissolve);
    outlineAlpha = saturate(outlineAlpha * outlineDissolve);
    outlineAlpha = saturate(((1.0 - bodyAlpha) * outlineAlpha));

    float mask = saturate (bodyAlpha + outlineAlpha);
    result.a = mask;

    result.a -= (bodyAlpha * (1.0 - _Color.a));
    result.a -= (outlineAlpha * (1.0 - _OutlineColor.a));

    result.rgb = (_Color.rgb * bodyAlpha.xxx) + (_OutlineColor.rgb * outlineAlpha.xxx * (1.0 - bodyAlpha).xxx);

    result *= v.color;

    //multiply texture onto color
    result.rgb = lerp(result.rgb, result.rgb * mixTex, _TextureImpact);

    //multiply texture onto alpha
    result.a *= max(mixTex, _TextureAlpha);
    //Apply Background Texture

    #if _MSDF_BACKGROUND
        float4 background = bgSample;
        float backgroundMix = MixOverlay(background, _BackgroundMix)* _BackgroundContrast.x;

        #if _MSDF_BACKGROUND_BLEND_ADD
            result.rgb = lerp(result.rgb, result.rgb + backgroundMix.xxx, bodyAlpha);
        #elif _MSDF_BACKGROUND_BLEND_MUL
            result.rgb = lerp(result.rgb, result.rgb * backgroundMix.xxx, bodyAlpha);
        #elif _MSDF_BACKGROUND_BLEND_OVERLAY
            result.rgb = lerp(result.rgb, OverlayColor(result.rgb,backgroundMix.xxx), bodyAlpha);
        #elif _MSDF_BACKGROUND_BLEND_LERP
            float4 backgroundColor = lerp(_BackgroundColorZero.rgba, _BackgroundColorOne.rgba, saturate(backgroundMix));
            result.rgb = lerp(result.rgb, backgroundColor.rgb, backgroundColor.a);
        #elif _MSDF_BACKGROUND_BLEND_NORMAL
            result.rgb = lerp(result.rgb, background.rgb, _BackgroundContrast.x);
        #endif

    #endif

    //Apply Drop Shadow
    #if _MSDF_DROPSHADOW
        float ShadowAlpha = BasicMSDF(shadowSample, mixTex, _Threshold + _ShadowThreshold, _Erosion, min(_ShadowSoftness, -0.01));
        float shadow = saturate(_ShadowBoost * (1.0 - ShadowAlpha)) * _ShadowColor.a;
        result.a += shadow;
        result.rgb = lerp(_ShadowColor.rgb, result.rgb, mask);
    #endif

    //Apply Center Glow
    #if _MSDF_CENTERGLOW
        float glowAlpha = saturate(BasicMSDF(dist, mixTex, _GlowThreshold, _Erosion, _GlowSoftness));

        #if _MSDF_CENTERGLOW_BLEND_ADD
            float4 glowColor = saturate(_GlowColor * saturate(_GlowBoost * ((glowAlpha)-dissolve).xxxx));
            result = saturate(result + glowColor);
        #elif _MSDF_CENTERGLOW_BLEND_MUL
            float4 glowColor = saturate(_GlowColor * (1.0 - saturate(_GlowBoost * ((glowAlpha)-dissolve).xxxx)));
            result.rgb = lerp(result.rgb, saturate(result.rgb * glowColor.rgb), glowAlpha);
        #elif _MSDF_CENTERGLOW_BLEND_OVERLAY
            float4 glowColor = saturate(_GlowColor * saturate(_GlowBoost * (glowAlpha)-dissolve).xxxx);
            result.rgb = lerp(result.rgb, OverlayColor(result.rgb,glowColor.rgb), glowAlpha);
        #endif
    #endif

    //Apply Inner Glow
    #if _MSDF_INNERGLOW
        float innerGlowAlpha = saturate(BasicMSDF(dist, mixTex, _Threshold - _InnerGlowThreshold, _Erosion, min(-_InnerGlowSoftness, -0.01)));
        innerGlowAlpha *= mask;
        innerGlowAlpha -= outlineAlpha;
        float4 innerGlowFinal;

        #if _MSDF_INNERGLOW_BLEND_ADD
            innerGlowFinal = saturate(_InnerGlowColor * (saturate(_InnerGlowBoost * (innerGlowAlpha)-dissolve).xxxx));
            result = saturate(result + innerGlowFinal);
        #elif _MSDF_INNERGLOW_BLEND_MUL
            innerGlowFinal = saturate(_InnerGlowColor * (saturate(_InnerGlowBoost * (innerGlowAlpha)-dissolve).xxxx));
            result.rgb = saturate(result.rgb * innerGlowFinal.rgb);
        #elif _MSDF_INNERGLOW_BLEND_OVERLAY
            innerGlowFinal = saturate(_InnerGlowColor * (saturate(_InnerGlowBoost * (innerGlowAlpha)-dissolve).xxxx));
            result.rgb = lerp(result.rgb, OverlayColor(result.rgb, innerGlowFinal.rgb), innerGlowAlpha);
        #endif

    #endif
    //Apply Outter Glow
    #if _MSDF_OUTTERGLOW
        float outterGlowAlpha = saturate(BasicMSDF(1.0 - dist, mixTex, _OutlineThreshold - _OutterGlowThreshold, _Erosion, min(-_OutterGlowSoftness, -0.01)));
        outterGlowAlpha -= saturate(mask);
        float4 outterGlowFinal = float4(1.0,1.0,1.0,1.0);

        #if _MSDF_OUTTERGLOW_BLEND_ADD
            outterGlowFinal = _OutterGlowColor * (outterGlowAlpha * _OutterGlowBoost - dissolve).xxxx;
            result = saturate(result + outterGlowFinal);
        #elif _MSDF_OUTTERGLOW_BLEND_MUL
            outterGlowFinal = saturate(_OutterGlowColor * (1.0 - saturate(_OutterGlowBoost * saturate(outterGlowAlpha) - dissolve) * (1.0 - mask)).xxxx);
            result = saturate(result * outterGlowFinal);
        #endif

    #endif


    #if _MSDF_SECONDARYSHAPE
        float secondaryTex = MixOverlay(tex.rgba, _SecondaryPicker);

        float3 secondaryDist = secondarySample.rgb;

         #if _MSDF_SECONDARYSHAPE_FILL_HORIZONTAL
            float fillAxis = saturate(v.unModUV.x);
            #if _MSDF_GRADIENT_BACKGROUND_UV
               fillAxis = saturate(v.uvTwo.z);
            #endif
            float fillMask = saturate(((fillAxis - _SecondaryShapeFill)) * _SecondaryShapeFillContrast);
            //secondaryDist = saturate(max(secondaryDist,fillMask.xxx));
            //secondaryDist = saturate(secondaryDist - fillMask.xxx);
         #endif
         #if _MSDF_SECONDARYSHAPE_FILL_RADIAL
            float2 fillAxis = v.uvTwo.xy;
            #if _MSDF_GRADIENT_BACKGROUND_UV
               fillAxis = v.uvTwo.zw;
            #endif

            float fillMask = RadialWipe(fillAxis, _SecondaryShapeFill, _SecondaryShapeFillContrast, _SecondaryShapeRotation);
            secondaryDist = saturate(max(secondaryDist, fillMask.xxx));
         #endif

        float alphaOne = BasicMSDF(secondaryDist, secondaryTex, _SecondaryThreshold, _SecondaryErosion, _Softness);
        float alphaTwo = BasicMSDF(secondaryDist, secondaryTex, _SecondaryOutterThreshold, _SecondaryErosion, _Softness);

        float secondaryMask = saturate(alphaOne - alphaTwo);
        #if _MSDF_SECONDARYSHAPE_FILL_HORIZONTAL
            secondaryMask *= (1.0 - fillMask).xxx;
        #endif

        #if _MSDF_SECONDARYSHAPE_OUTTERGLOW

            float secondaryInner = BasicMSDF(secondaryDist, secondaryTex, _SecondaryThreshold - _SecondaryOutterGlowThreshold, _SecondaryErosion, -_SecondaryOutterGlowSoftness);
            float secondaryOutter = BasicMSDF(secondaryDist, secondaryTex, _SecondaryOutterThreshold + _SecondaryOutterGlowThreshold, _SecondaryErosion, _SecondaryOutterGlowSoftness);

            secondaryOutter *= (1.0 - alphaTwo);
            secondaryInner *= alphaOne;

            float combinedMask = saturate(secondaryOutter + secondaryInner);

            #if _MSDF_SECONDARYSHAPE_FILL_HORIZONTAL
               combinedMask *= (1.0 - fillMask).xxx;
            #endif

            #if _MSDF_SECONDARYSHAPE_OUTTERGLOW_BLEND_ADD

                float secondaryGlowMask = combinedMask * (1.0 - secondaryMask) * _SecondaryOutterGlowBoost;
                result += saturate(_SecondaryOutterGlowColor * secondaryGlowMask.xxxx);

            #elif _MSDF_SECONDARYSHAPE_OUTTERGLOW_BLEND_MUL

                float secondaryGlowMask = saturate(combinedMask * (1.0 - secondaryMask)) ;
                result.a += secondaryGlowMask;
                result.rgb *= saturate(lerp(_SecondaryOutterGlowColor.rgb , float3(1.0, 1.0, 1.0), saturate(_SecondaryOutterGlowBoost * (1.0 - secondaryGlowMask) + secondaryMask)));
                result.rgb = saturate(result.rgb);
            #endif

        #endif

        result.a = saturate(result.a + secondaryMask);

        float4 secondaryResult = _SecondaryColor;

        secondaryResult.rgb = lerp(secondaryResult.rgb, secondaryResult.rgb * secondaryTex, _SecondaryTextureImpact);

        result = lerp(result, secondaryResult, secondaryMask);

    #endif

    #if _MSDF_ICON
         float4 iconTex = icon;

         float iconAlpha = 0.0;
         float iconStrokeAlpha = 0.0;
         float3 iconColor = float3(1.0, 1.0, 1.0);

         #if _MSDF_ICON_ALPHAMODE_MSDF
            iconAlpha = BasicMSDF(iconTex.rgb, 1.0, _IconThreshold, 0.0, _Softness);
            iconStrokeAlpha = BasicMSDF(iconTex.rgb, 1.0, _IconStrokeThreshold, 0.0, _Softness);
            iconColor = lerp(_IconStrokeColor.rgb, _IconColor.rgb, iconAlpha);
         #elif _MSDF_ICON_ALPHAMODE_CUTOFF
            float range = 2.0 * _Softness;
            iconAlpha = SDFSampleInt(iconTex.a, _Softness, _IconThreshold, range);
            iconStrokeAlpha = SDFSampleInt(iconTex.a, _Softness, _IconStrokeThreshold, range);
            iconColor = lerp(_IconStrokeColor.rgb, iconTex.rgb * _IconColor.rgb, iconAlpha);
         #endif

            //mask the icon so the secondary shape is above it
         #if _MSDF_ICON_MASK_SECONDARY
            float iconMask = lerp(_IconStrokeColor.a, _IconColor.a, iconAlpha) * iconStrokeAlpha * (1.0 - secondaryMask);
         #else
            float iconMask = lerp(_IconStrokeColor.a, _IconColor.a, iconAlpha) * iconStrokeAlpha;
         #endif

            //mask the icon so it only appears inside the primary shape
         #if _MSDF_ICON_MASK_PRIMARY
            iconMask *= bodyAlpha;
         #endif

         result.a = saturate(result.a + iconMask);
         result.rgb = lerp(result.rgb, iconColor.rgb, iconMask);

    #endif



    #if _MSDF_GRADIENT_HORIZONTAL

        float4 gH = float4(0.0, 0.0, 0.0, 0.0);

        float gradHAxis = v.uvTwo.x;
         #if _MSDF_GRADIENT_BACKGROUND_UV
                 gradHAxis = v.uvTwo.z;
         #endif

        #if _MSDF_GRADIENT_HORIZONTAL_MODE_ZEROTOONE
            gH = ZeroOneGradient(gradHAxis, _GradientHorizontalOffset, _GradientHorizontalContrast, _GradientHorizontalZeroColor, _GradientHorizontalOneColor);
        #elif _MSDF_GRADIENT_HORIZONTAL_MODE_ZEROONEZERO
            gH = ZeroOneZeroGradient(gradHAxis, _GradientHorizontalOffset, _GradientHorizontalContrast, _GradientHorizontalZeroColor, _GradientHorizontalOneColor);
        #endif

        #if _MSDF_SECONDARYSHAPE
                float knockoutMaskH = Knockout(mask, _GradientImpactPrimary, secondaryMask, _GradientImpactSecondary);

               #if _MSDF_ICON
                  float iconGradientMaskH = iconMask * _GradientImpactIcon;
                  knockoutMaskH += iconGradientMaskH;
                  knockoutMaskH = saturate(knockoutMaskH);
               #endif

            #if _MSDF_GRADIENT_HORIZONTAL_BLEND_MUL
                result.rgb = lerp(result.rgb, result.rgb * gH.rgb, knockoutMaskH);
            #elif _MSDF_GRADIENT_HORIZONTAL_BLEND_ADD
                result.rgb = lerp(result.rgb, saturate(result.rgb + gH.rgb), knockoutMaskH);
            #elif _MSDF_GRADIENT_HORIZONTAL_BLEND_OVERLAY
                result.rgb = lerp(result.rgb, OverlayColor(result.rgb, gH.rgb), knockoutMaskH);
            #elif _MSDF_GRADIENT_HORIZONTAL_BLEND_NORMAL
                  result.rgb = lerp(result.rgb, lerp(result.rgb, gH.rgb, gH.a), knockoutMaskH);
            #endif
        #else
            float ghMask = 1.0;

            #if _MSDF_ICON
               ghMask = Knockout(result.a,1.0,iconAlpha,_GradientImpactIcon);
               ghMask = saturate(ghMask);
            #endif

            #if _MSDF_GRADIENT_HORIZONTAL_BLEND_MUL
                result.rgb = lerp(result.rgb,result.rgb * gH.rgb, ghMask);
            #elif _MSDF_GRADIENT_HORIZONTAL_BLEND_ADD
               result.rgb = lerp(result.rgb, saturate(result.rgb + gH.rgb), ghMask);
            #elif _MSDF_GRADIENT_HORIZONTAL_BLEND_OVERLAY
                result.rgb = lerp(result.rgb, OverlayColor(result.rgb, gH.rgb), ghMask);
            #elif _MSDF_GRADIENT_HORIZONTAL_BLEND_NORMAL
               result.rgb = lerp(result.rgb, lerp(result.rgb, gH.rgb, gH.a), ghMask);
            #endif
        #endif

    #endif

    #if _MSDF_GRADIENT_VERTICAL

        float4 gV = float4(0.0,0.0,0.0,0.0);

        float gradVAxis = v.uvTwo.y;
      #if _MSDF_GRADIENT_BACKGROUND_UV
              gradVAxis = v.uvTwo.w;
      #endif

        #if _MSDF_GRADIENT_VERTICAL_MODE_ZEROTOONE
            gV = ZeroOneGradient(gradVAxis, _GradientVerticalOffset, _GradientVerticalContrast, _GradientVerticalZeroColor, _GradientVerticalOneColor);
        #elif _MSDF_GRADIENT_VERTICAL_MODE_ZEROONEZERO
            gV = ZeroOneZeroGradient(gradVAxis, _GradientVerticalOffset, _GradientVerticalContrast, _GradientVerticalZeroColor, _GradientVerticalOneColor);
        #endif

        #if _MSDF_SECONDARYSHAPE
            float knockoutMaskV = Knockout(mask, _GradientImpactPrimary, secondaryMask, _GradientImpactSecondary);

            #if _MSDF_ICON
               float iconGradientMaskV = Knockout(knockoutMaskV, 1.0, iconMask,_GradientImpactIcon);
               knockoutMaskV = iconGradientMaskV;
            #endif

            #if _MSDF_GRADIENT_VERTICAL_BLEND_MUL
                result.rgb = lerp(result.rgb, result.rgb * gV.rgb, knockoutMaskV);
            #elif _MSDF_GRADIENT_VERTICAL_BLEND_ADD
                result.rgb = lerp(result.rgb, saturate(result.rgb + gV.rgb), knockoutMaskV);
            #elif _MSDF_GRADIENT_VERTICAL_BLEND_OVERLAY
                result.rgb = lerp(result.rgb, OverlayColor(result.rgb, gV.rgb), knockoutMaskV);
            #elif _MSDF_GRADIENT_VERTICAL_BLEND_NORMAL
               result.rgb = lerp(result.rgb, lerp(result.rgb, gV.rgb, gV.a), knockoutMaskV * gV.a);
            #endif
        #else
            float gvMask = 1.0;

            #if _MSDF_ICON
               gvMask = Knockout(result.a, 1.0, iconAlpha, _GradientImpactIcon);
            #endif

            #if _MSDF_GRADIENT_VERTICAL_BLEND_MUL
               result.rgb = lerp(result.rgb, result.rgb * gV.rgb, gvMask);
            #elif _MSDF_GRADIENT_VERTICAL_BLEND_ADD
               result.rgb = lerp(result.rgb, result.rgb + gV.rgb, gvMask);
            #elif _MSDF_GRADIENT_VERTICAL_BLEND_OVERLAY
               result.rgb = lerp(result.rgb, OverlayColor(result.rgb, gV.rgb), gvMask);
            #elif _MSDF_GRADIENT_VERTICAL_BLEND_NORMAL
               result.rgb = lerp(result.rgb, lerp(result.rgb, gV.rgb, gV.a), gvMask);
            #endif
        #endif

    #endif


    #if _MSDF_GRADIENT_RADIAL

        float4 gR = float4(0.0, 0.0, 0.0, 0.0);
        float2 center = float2(0.5,0.5);

        float2 gradRAxis = v.uvTwo.xy;
      #if _MSDF_GRADIENT_BACKGROUND_UV
              gradRAxis = v.uvTwo.zw;
      #endif

        gR = RadialGradient(gradRAxis, center, _GradientRadialSize, _GradientRadialContrast, _GradientRadialZeroColor, _GradientRadialOneColor);

        #if _MSDF_SECONDARYSHAPE
            float knockoutMaskR = Knockout(mask, _GradientImpactPrimary, secondaryMask, _GradientImpactSecondary);

            #if _MSDF_ICON
               float iconGradientMaskR = iconMask * _GradientImpactIcon;
               knockoutMaskR += iconGradientMaskR;
            #endif

            #if _MSDF_GRADIENT_RADIAL_BLEND_MUL
                result.rgb = lerp(result.rgb, result.rgb * gR.rgb, knockoutMaskR);
            #elif _MSDF_GRADIENT_RADIAL_BLEND_ADD
                result.rgb = lerp(result.rgb, result.rgb + gR.rgb, knockoutMaskR);
            #elif _MSDF_GRADIENT_RADIAL_BLEND_OVERLAY
                result.rgb = lerp(result.rgb, OverlayColor(result.rgb, gR.rgb), knockoutMaskR);
            #endif
        #else
            float grMask = 1.0;

            #if _MSDF_ICON
               grMask = Knockout(result.a, 1.0, iconAlpha, _GradientImpactIcon);
            #endif

            #if _MSDF_GRADIENT_RADIAL_BLEND_MUL
               result.rgb = lerp(result.rgb, result.rgb * gR.rgb, grMask);
            #elif _MSDF_GRADIENT_RADIAL_BLEND_ADD
               result.rgb = lerp(result.rgb, saturate(result.rgb + gR.rgb), grMask);
            #elif _MSDF_GRADIENT_RADIAL_BLEND_OVERLAY
               result.rgb = lerp(result.rgb, OverlayColor(result.rgb, gR.rgb), grMask);
            #endif
        #endif
    #endif

    #if _MSDF_SHINE
        float shineTex = MixOverlay(tex.rgba, _ShinePicker);
        float shine = CalculateShine(v.uvOne.zw, shineTex, _ShineWidth, _ShineFrequency, _ShineSpeed);

        //create sectioned Shine results

        float primaryShine = bodyAlpha * _ShineImpactPrimary;
        float strokeShine = (outlineAlpha) * _ShineImpactPrimaryStroke;
        float combinedShine = saturate(primaryShine + strokeShine);

        float3 shineColor = saturate(_ShineColor.rgb * shine * _ShineBoost);
//return float4(shineColor, 1);
        #if _MSDF_SECONDARYSHAPE
            float knockoutMaskS = Knockout(combinedShine, 1.0, secondaryMask, _ShineImpactSecondary);
           #if _MSDF_ICON
               float iconShineMask = iconMask * _ShineImpactIcon;
               knockoutMaskS += iconShineMask;
            #endif
               result.rgb = lerp(result.rgb, result.rgb + shineColor, knockoutMaskS);
        #else
            #if _MSDF_ICON
               float knockoutMaskS = Knockout(combinedShine, 1.0, iconMask, _ShineImpactIcon);
               result.rgb += lerp(result.rgb, saturate(result.rgb + shineColor), knockoutMaskS);
            #else
               result.rgb += saturate(shineColor * combinedShine);
            #endif
        #endif
    #endif

    result.rgb = Greyscale(result.rgb, _Greyscale);

    result.a *= v.color.a;

    return result;
}