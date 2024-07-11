#ifndef ALL_IN_ONE_INPUT_INCLUDED
#define ALL_IN_ONE_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

#ifdef _DISSOLUTIONSWITCH_ON
    TEXTURE2D(_DissolutionMap);            SAMPLER(sampler_DissolutionMap);
#endif

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Surface;

#ifdef _OUTLINESWITCH_ON
    half _OutlineWidth;
#endif

#ifdef _FLICKERSWITCH_ON
    half _FlickerFrequency;
    half4 _FlickerColor;
#endif

#ifdef _DISSOLUTIONSWITCH_ON
    float4 _DissolutionMap_ST;
#endif

#ifdef _FRESNELSWITCH_ON
    half4 _FresnelColor;
    half _FresnelPower;
#endif

#ifdef _HSISWITCH_ON
    half _Brightness;
    half _Saturation;
    half _Contrast;
#endif
CBUFFER_END

#ifdef UNITY_DOTS_INSTANCING_ENABLED
    UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _SpecColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
    UNITY_DOTS_INSTANCED_PROP(float, _Cutoff)
    UNITY_DOTS_INSTANCED_PROP(float, _Surface)
    UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

    #define _BaseColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor)
    #define _SpecColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _SpecColor)
    #define _EmissionColor      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _EmissionColor)
    #define _Cutoff             UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Cutoff)
    #define _Surface            UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Surface)
#endif

TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);

half4 SampleSpecularSmoothness(float2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap)) {
    half4 specularSmoothness = half4(0, 0, 0, 1);
    #ifdef _SPECGLOSSMAP
        specularSmoothness = SAMPLE_TEXTURE2D(specMap, sampler_specMap, uv) * specColor;
    #elif defined(_SPECULAR_COLOR)
        specularSmoothness = specColor;
    #endif

    #ifdef _GLOSSINESS_FROM_BASE_ALPHA
        specularSmoothness.a = alpha;
    #endif

    return specularSmoothness;
}

inline void InitializeSimpleLitSurfaceData(float2 uv,
#ifdef _DISSOLUTIONSWITCH_ON
    float2 dissolutionUV,
#endif
out SurfaceData outSurfaceData) {
    outSurfaceData = (SurfaceData)0;

    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = albedoAlpha.a * _BaseColor.a;

    #ifdef _DISSOLUTIONSWITCH_ON
        half dissolution = SAMPLE_TEXTURE2D(_DissolutionMap, sampler_DissolutionMap, dissolutionUV).r;
        outSurfaceData.alpha = AlphaDiscard(outSurfaceData.alpha * dissolution, _Cutoff);
    #else
        outSurfaceData.alpha = AlphaDiscard(outSurfaceData.alpha, _Cutoff);
    #endif

    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);

    #ifdef _FLICKERSWITCH_ON
        outSurfaceData.albedo += _FlickerColor.rgb * abs(sin(_Time.y * _FlickerFrequency));
    #endif

    half4 specularSmoothness = SampleSpecularSmoothness(uv, outSurfaceData.alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
    outSurfaceData.metallic = 0.0; // unused
    outSurfaceData.specular = specularSmoothness.rgb;
    outSurfaceData.smoothness = specularSmoothness.a;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    outSurfaceData.occlusion = 1.0;
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
}

#endif