Shader "GBHighlight/Pattern 1" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" { }
        _Saturation ("Saturation", Range(0, 1)) = 1
    }

    SubShader {

        Pass {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float4 uv : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            half _Saturation;

            Varyings Vertex(Attributes input) {

                Varyings output = (Varyings)0;
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

                output.uv.xy = input.texcoord.xy;

                return output;
            }

            half4 Fragment(Varyings input) : SV_Target {

                half3 color = tex2D(_MainTex, input.uv.xy).rgb;

                half gray = dot(color, half3(0.2125, 0.7154, 0.0721));
                color = lerp(half3(gray, gray, gray), color, _Saturation);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}