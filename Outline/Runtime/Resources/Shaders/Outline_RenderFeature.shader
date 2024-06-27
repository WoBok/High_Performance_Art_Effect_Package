Shader "Outline/Outline_RenderFeature" {
    Properties {
        _Outline ("Outline", Range(0, 1)) = 0.1
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _DissolutionMap ("Dissolution Map", 2D) = "white" { }
        [Toggle]_AlphaCliping ("Alpha Cliping", Float) = 0
        _Cutoff ("Threshold", Range(0, 1)) = 0
    }

    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Cull Front
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile _ALPHACLIPING_OFF _ALPHACLIPING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 dissolutionUV : TEXCOORD0;
            };
            
            sampler2D _BaseMap;
            TEXTURE2D(_DissolutionMap);
            SAMPLER(sampler_DissolutionMap);

            CBUFFER_START(UnityPerMaterial)
            float _Outline;
            half4 _OutlineColor;
            float4 _DissolutionMap_ST;
            float _Cutoff;
            CBUFFER_END

            Varyings Vertex(Attributes input) {

                Varyings output;
                
                float4 position = mul(UNITY_MATRIX_MV, input.positionOS);
                float3 normal = mul((float3x3)UNITY_MATRIX_IT_MV, input.normalOS);
                normal.z = -0.5;
                position = position + float4(normalize(normal), 0) * _Outline;
                output.positionCS = mul(UNITY_MATRIX_P, position);

                output.dissolutionUV = TRANSFORM_TEX(input.texcoord, _DissolutionMap);

                return output;
            }

            half4 Fragment(Varyings input) : SV_Target {

                #ifdef _ALPHACLIPING_ON
                    half dissolution = SAMPLE_TEXTURE2D(_DissolutionMap, sampler_DissolutionMap, input.dissolutionUV).r;
                    clip(dissolution - _Cutoff);
                #endif
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}