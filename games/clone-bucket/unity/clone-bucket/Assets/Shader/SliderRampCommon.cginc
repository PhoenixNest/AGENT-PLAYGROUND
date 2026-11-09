#ifndef SLIDER_RAMP_COMMON_INCLUDED
#define SLIDER_RAMP_COMMON_INCLUDED

inline float3 SR_LightDir()
{
    float3 l = _WorldSpaceLightPos0.xyz;
    return dot(l, l) > 0.0 ? normalize(l) : float3(0.0, 1.0, 0.0);
}

inline float SR_Ramp(float3 worldNormal, float3 lightDir, float threshold, float smoothing)
{
    float nl = max(dot(worldNormal, lightDir), 0.0);
    float lo = threshold - smoothing * 0.5;
    float t = saturate((nl - lo) / max(smoothing, 0.001));
    return t * t * (3.0 - 2.0 * t);
}

inline float3 SR_ShadeColor(float ramp, float4 shadowColor, float3 highlightColor)
{
    float3 mixed = lerp(highlightColor, shadowColor.rgb, shadowColor.a);
    return lerp(mixed, highlightColor, ramp);
}

inline float SR_Specular(float3 worldNormal, float3 lightDir, float3 viewDir, float size, float edge)
{
    float3 h = normalize(lightDir + viewDir);
    float ndh = max(dot(worldNormal, h), 0.0);
    float s = pow(max(ndh, 1e-5), max(size, 0.0) * 128.0);
    float a = 0.5 - edge * 0.5;
    float b = 0.5 + edge * 0.5;
    float t = saturate((2.0 * s - a) / max(b - a, 0.001));
    return t * t * (3.0 - 2.0 * t);
}

inline float2 SR_MatcapUV(float3 worldNormal, float3 viewDir)
{
    float3 r = reflect(-viewDir, worldNormal);
    return r.xy * 0.5 + 0.5;
}

#endif
