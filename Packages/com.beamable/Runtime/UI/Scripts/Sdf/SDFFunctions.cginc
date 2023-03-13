#include "UnityCG.cginc"

float4 blend_cutIn(float4 bottom, float4 top, float opacity){
    float4 result;
    float v = saturate((opacity + step(bottom.a, .001)) * ((top.a + .01) / (bottom.a + .01)));
    result.rgb = lerp(bottom.rgb, top.rgb, v);
    result.a = saturate(bottom.a + top.a);
    return result;
}

float4 blend_overlay(float4 bottom, float4 top){
    float4 result;
    result.rgb = lerp(bottom.rgb, top.rgb, saturate((top.a + .01) / (bottom.a + .01)));
    result.a = saturate(bottom.a + top.a);
    return result;
}

//smooth version of step
float aaStep(float compValue, float gradient){
  float change = fwidth(gradient) * .5; // * .5 to make aa thiner
  //base the range of the inverse lerp on the change over two pixels
  float lowerEdge = compValue - change;
  float upperEdge = compValue + change;
  //do the inverse interpolation
  float stepped = (gradient - lowerEdge) / (upperEdge - lowerEdge + .0001);
  stepped = saturate(stepped);
  //smoothstep version here would be `smoothstep(lowerEdge, upperEdge, gradient)`
  return stepped;
}

// returns distance to the rectange with center in point (0, 0)
float sdfRectangle(float2 samplePosition, float2 halfSize){
    float2 componentWiseEdgeDistance = abs(samplePosition) - halfSize;
    float outsideDistance = length(max(componentWiseEdgeDistance, 0));
    float insideDistance = min(max(componentWiseEdgeDistance.x, componentWiseEdgeDistance.y), 0);
    return outsideDistance + insideDistance;
}

// returns distance to rounded rectange with center in point (0, 0)
float sdfRoundedRectangle(float2 samplePosition, float2 halfSize, float rounding){
    return sdfRectangle(samplePosition, float2(halfSize.x - rounding, halfSize.y - rounding)) - rounding;
}

float3 floatToRGB( float v ){
    return frac((v) / float3(16777216, 65536, 256));
}

float2 floatToRG(float f){
    float3 v = floatToRGB(f);
    v.x -= .5;
    v.y -= .5;
    v.z *= 255;
    return v.xy * v.z;
}
