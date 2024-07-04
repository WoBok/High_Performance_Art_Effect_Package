Shader "LiQingZhao/Water" {
    Properties {
        _BaseColor ("Color", Color) = (1, 1, 1, 1)

        [Header(PBR)]
        [Space(5)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metallic", Range(0, 1)) = 0

        [Header(Water)]
        [Space(5)]
        _Wave ("Wave", 2D) = "white" { }
        [Space(5)]
        _WaveSpeed ("Wave Speed", Float) = 1
        _MainWaveBumpScale ("Main Wave Bump Scale", Range(0, 2)) = 1
        _SecondWaveBumpScale ("Second Normal Bump Scale", Range(0, 2)) = 1
        _WaveTilingOffset ("Main Wave Tiling Offset", Vector) = (1, 1, 1, 1)
        _SecondWaveTilingOffset ("Second Wave Tiling Offset", Vector) = (1, 1, -1, 1)

        [Header(Editor)]
        [Toggle]EditorMode ("Editor Mode", Float) = 0
    }

    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass {
            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma shader_feature EDITORMODE_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float4 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 texcoord : TEXCOORD0;
                float2 staticLightmapUV : TEXCOORD1;
            };

            struct Varyings {
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float4 positionCS : SV_POSITION;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 5);
            };
            
            sampler2D _Wave;
            float4 _Wave_ST;

            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            float _Smoothness;
            float _Metallic;
            float _WaveSpeed;
            half _MainWaveBumpScale;
            half _SecondWaveBumpScale;
            half4 _WaveTilingOffset;
            half4 _SecondWaveTilingOffset;

            float4 _CameraPosition;
            CBUFFER_END

            Varyings Vertex(Attributes input) {
                Varyings output;
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;

                output.uv = input.texcoord;

                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

                return output;
            }

            float3 WaveNormal(Varyings input) {
                float2 mainWaveUV = input.uv * _WaveTilingOffset.xy + _Time.x * _WaveSpeed * _WaveTilingOffset.zw * 0.1;
                float2 secondWaveUV = input.uv * _SecondWaveTilingOffset.xy + _Time.x * _WaveSpeed * _WaveTilingOffset.zw * 0.1;

                half3 waterNormal1 = tex2D(_Wave, mainWaveUV).xyz;
                half3 waterNormal2 = tex2D(_Wave, secondWaveUV).xyz;

                half3 waterNormal = ((waterNormal1 + waterNormal2) * 0.6667 - 0.6667) * half3(_SecondWaveBumpScale, _SecondWaveBumpScale, 1);

                waterNormal += half3(_MainWaveBumpScale, _MainWaveBumpScale, 1);

                return normalize(TransformTangentToWorld(waterNormal, float3x3(input.tangentWS, input.bitangentWS, input.normalWS)));
            }

            void InitializeInputData(Varyings input, out InputData inputData) {
                inputData = (InputData)0;
                inputData.normalWS = WaveNormal(input);
                #ifdef EDITORMODE_ON
                    float3 cameraPos = _WorldSpaceCameraPos;
                #else
                    float3 cameraPos = _CameraPosition;
                #endif
                cameraPos.y = 2;
                inputData.viewDirectionWS = normalize(cameraPos - input.positionWS.xyz);
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, input.normalWS);
            }

            void InitializeSurfaceData(float2 uv, out SurfaceData surfaceData) {
                surfaceData = (SurfaceData)0;
                surfaceData.albedo = _BaseColor.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.occlusion = 1;
                surfaceData.alpha = _BaseColor.a;
            }

            half4 Fragment(Varyings input) : SV_Target {
                InputData inputData;
                InitializeInputData(input, inputData);

                SurfaceData surfaceData;
                InitializeSurfaceData(input.uv, surfaceData);

                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }
    }
}