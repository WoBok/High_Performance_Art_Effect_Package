Shader "URP Shader/All In One" {

    Properties {
        [Header(Outline)]
        [Toggle]_OutlineSwitch ("Enable Outline", Float) = 0
        _OutlineWidth ("Outline Width", Range(0, 1)) = 0.1

        [Header(Flicker)]
        [Toggle]_FlickerSwitch ("Enable Flicker", Float) = 0
        _FlickerFrequency ("Flicker Frequency", Float) = 1
        [HDR] _FlickerColor ("Flicker Color", Color) = (0, 0, 0)

        [Header(Dissolution)]
        [Toggle]_DissolutionSwitch ("Enable Dissolution", Float) = 0
        _DissolutionMap ("Dissolution Map", 2D) = "white" { }

        [Header(Fresnel)]
        [Toggle]_FresnelSwitch ("Enable Fresnel", int) = 0
        _FresnelColor ("Fresnel Color", Color) = (1, 1, 1, 0)
        _FresnelPower ("Fresnel Power", Range(0, 8)) = 3

        [Header(HSI)]
        [Toggle]_HSISwitch ("Enable HSI", int) = 0
        _Brightness ("Brightness", Range(0, 10)) = 1
        _Saturation ("Saturation", Range(0, 10)) = 1
        _Contrast ("Contrast", Range(0, 10)) = 1

        [Header(Fog)]
        [Toggle]_FogSwitch ("Enable Fog", int) = 0

        //---------------------------------------------------------------------------------------------------------

        [MainTexture] _BaseMap ("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" { }
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        _Cutoff ("Alpha Clipping", Range(0.0, 1.0)) = 0.5

        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _SpecGlossMap ("Specular Map", 2D) = "white" { }
        _SmoothnessSource ("Smoothness Source", Float) = 0.0
        _SpecularHighlights ("Specular Highlights", Float) = 1.0

        [HideInInspector] _BumpScale ("Scale", Float) = 1.0
        [NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" { }

        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0)
        [NoScaleOffset]_EmissionMap ("Emission Map", 2D) = "white" { }

        _Surface ("__surface", Float) = 0.0
        _Blend ("__blend", Float) = 0.0
        _Cull ("__cull", Float) = 2.0
        [ToggleUI] _AlphaClip ("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha ("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha ("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
        [HideInInspector] _BlendModePreserveSpecular ("_BlendModePreserveSpecular", Float) = 1.0
        [HideInInspector] _AlphaToMask ("__alphaToMask", Float) = 0.0

        [ToggleUI] _ReceiveShadows ("Receive Shadows", Float) = 1.0
        _QueueOffset ("Queue offset", Float) = 0.0

        [HideInInspector] _MainTex ("BaseMap", 2D) = "white" { }
        [HideInInspector] _Color ("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _Shininess ("Smoothness", Float) = 0.0
        [HideInInspector] _GlossinessSource ("GlossinessSource", Float) = 0.0
        [HideInInspector] _SpecSource ("SpecularHighlights", Float) = 0.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" { }
        [HideInInspector][NoScaleOffset]unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" { }
        [HideInInspector][NoScaleOffset]unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" { }
    }

    SubShader {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "SimpleLit" "IgnoreProjector" = "True"
        }
        LOD 300

        Pass {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            ZWrite[_ZWrite]
            Cull[_Cull]
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple

            #pragma shader_feature_local_fragment _OUTLINESWITCH_ON
            #pragma shader_feature_local_fragment _FLICKERSWITCH_ON
            #pragma shader_feature_local_fragment _DISSOLUTIONSWITCH_ON
            #pragma shader_feature_local_fragment _FRESNELSWITCH_ON
            #pragma shader_feature_local_fragment _HSISWITCH_ON
            #pragma shader_feature_local_fragment _FOGSWITCH_ON

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
            #pragma shader_feature_local_fragment _ _SPECGLOSSMAP _SPECULAR_COLOR
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _FORWARD_PLUS
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #define BUMP_SCALE_NOT_SUPPORTED 1

            #include "AllInOneInput.hlsl"
            #include "AllInOneForwardPass.hlsl"
            ENDHLSL
        }

        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "AllInOneInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    Fallback "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.AllInOneShader"
}