#include "UnityCG.cginc"


//Collapsed functions part of SDF.cginc

half SDFSampleInt(half dist, half afwidth, float threshold, half range)
{
   half bottom = threshold-afwidth;

   return saturate((dist - bottom)/range);
}

float SDFSample(sampler2D tex, float2 uv, float threshold, float afwidth)
{
   float dist = tex2D(tex, uv.xy).a;

   float range = 2.0 * afwidth;

   return SDFSampleInt(dist,afwidth,threshold,range);
}


float Median(float r, float g, float b)
{
   return(max(min(r, g), min(max(r, g), b)));
}

float MsdfThreshold(float distance, float erosion, float threshold, float softness, float range)
{
   return saturate(((distance + erosion) - ((1 - threshold) - softness)) / range);
}

float CircleDistance(float2 p, float2 center, float radius) {
    return saturate(length(p - center) - radius);
}

float MsdfSignedDistance(float3 distanceTex){
    return 1.0 - (Median(distanceTex.r, distanceTex.g, distanceTex.b));
}

float BasicMSDF2(float sigDist, float mixTex, float threshold, float erosion, float softness)
{
   float range = 2.0 * softness;

   //apply erosion
   float ero = 1.0 - ((mixTex) * (erosion));

   return MsdfThreshold(sigDist, ero, threshold, softness, range);
}


//returns threshold and mixed tex
float BasicMSDF(float3 distanceTex, float mixTex, float threshold, float erosion, float softness)
{
   float sigDist = 1.0 - (Median(distanceTex.r, distanceTex.g, distanceTex.b));

   float range = 2.0 * softness;

   //apply erosion
   float ero = 1.0 - ((mixTex) * (erosion));

   return MsdfThreshold(sigDist, ero, threshold, softness, range);
}

float MixOverlay(float4 overlayTex, float4 overlayMix)
{
    return saturate(dot(overlayTex, overlayMix));
}


float2 ComputeScreenUV(float4 position, float4 st)
{
    float4 scrPos = ComputeScreenPos(position);
    return (scrPos.xy) * st.xy + st.zw;
}

float2 ScrollUV(float2 uv, float xRate, float yRate)
{
    float t = _Time.x;
    uv.x += (t * xRate);
    uv.y += (t * yRate);
    return uv;
}

float2 ComputeRotatedScreenUV(float2 screenPos, float angleRate)
{
   float angle = angleRate * (_ScreenParams.y / _ScreenParams.x);
   float rot_cos = cos(angle);
   float rot_sin = sin(angle);
   float2 rot_pivot = float2(0.5, 0.5);
   return (mul(screenPos.xy - rot_pivot.xy, half2x2(rot_cos, -rot_sin, rot_sin, rot_cos)) + rot_pivot.xy);
}

float2 ComputePivotRotation(float2 baseUV, float angle, float2 pivot)
{
    float rot_cos = cos(angle);
    float rot_sin = sin(angle);
    float2 rot_pivot = pivot;
    float2x2 rotationMatrix = float2x2(rot_cos, -rot_sin, rot_sin, rot_cos);
    float2 newUV = mul(baseUV - rot_pivot, rotationMatrix) + rot_pivot;
    return newUV;
}

float2 ComputeRotatedUV(float2 baseUV, float angle)
{
   return ComputePivotRotation(baseUV, angle, float2(0.5,0.5));
}


half4 AddPattern(half2 uvRotated, half patternSpeed, half position, half width, float3 patternColor, half2 screenUV, float4 sample, float loopSize)
{
   // apply pattern
   half offset = fmod(_Time.y * patternSpeed + position, loopSize) - (loopSize*0.5);

   half alphaGrad = 1.0 - saturate(abs((uvRotated.x - offset) * width));
   return half4(sample.rgb + saturate(patternColor.rgb * alphaGrad.xxx),sample.a);
}

float ZeroOneZero(float gradient)
{
    return 1.0 - abs(saturate(gradient) * 2.0 - 1.0);
}

float4 ZeroOneGradient(float gradient, float offset, float contrast, float4 ColorOne, float4 ColorTwo)
{
    float grad = saturate((gradient - offset) * contrast);
    float4 col = lerp(ColorOne, ColorTwo, grad);
    return col;
}

float4 ZeroOneZeroGradient(float gradient, float offset, float contrast, float4 ColorOne, float4 ColorTwo)
{
    float grad = ZeroOneZero(gradient);
    return ZeroOneGradient(grad, offset, contrast, ColorOne, ColorTwo);
}

float3 OverlayColor(float3 ColorOne, float3 ColorTwo)
{
    float3 mask = step(0.5, ColorOne);
    return lerp( saturate(ColorOne * ColorTwo * 2.0.xxx), saturate(1.0.xxx - (2.0.xxx * (1.0.xxx - ColorOne) * (1.0.xxx - ColorTwo))), mask);
}

float4 RadialGradient(float2 uv, float2 pos, float size, float contrast, float4 ColorOne, float4 ColorTwo)
{
    float grad = saturate(distance(uv.xy, pos.xy));
    return ZeroOneGradient(grad, size, contrast, ColorOne, ColorTwo);
}

float RadialWipe(float2 uv, float amount, float power, float rotation)
{
   float2 rotUV = ComputeRotatedUV(uv, rotation);
   float2 newUV = (rotUV - float2(0.5, 0.5)) * 2.0.xx;
   float radial = ((atan2(newUV.x, newUV.y) * 0.31830988618379067153776752674503) * 0.5) + 0.5;
   float wipe = pow(radial / amount, power);
   return wipe;
}

float CalculateShine(float2 uv, float overlay, float width, float frequency, float speed)
{
    float gradient = saturate(sin((uv * frequency) + (_Time.x * speed)) - width * overlay);
    return gradient;
}

float Knockout(float maskOne, float impactOne, float maskTwo, float impactTwo)
{
    float primaryMask = saturate(maskOne * (impactOne));
    float secondaryMask = saturate(maskTwo * (impactTwo));

    return  lerp(primaryMask,secondaryMask,maskTwo);
}

float3 Greyscale(float3 color, float amount)
{
    float3 grey = fixed3(0.2125, 0.7154, 0.0721);
    float3 greyscale = dot(color, grey);
    return (lerp(color, greyscale, amount));
}

float Dissolve(float grad, float amount)
{
   return 1.0 - saturate((grad) * (amount));
}

float2 TransformUV(float2 coord, float4 st)
{
    return float2(coord.xy * st.xy + st.zw);
}
