Shader "GBHighlight/Pattern 2" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" { }
        _Radius ("Radius", Float) = 0
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
                float4 screenPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float2 _ObjScreenPos;
            float _Radius;

            Varyings Vertex(Attributes input) {

                Varyings output = (Varyings)0;
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

                output.uv.xy = input.texcoord.xy;

                output.screenPos = ComputeScreenPos(output.positionCS);

                return output;
            }

            half4 Fragment(Varyings input) : SV_Target {

                float2 uv = input.screenPos.xy / input.screenPos.w;
                half3 color = tex2D(_MainTex, uv).rgb;

                float2 objScreenPos = _ObjScreenPos * 2 - 1;
                float2 currentScreenPos = uv * 2 - 1;
                float inRange = smoothstep(_Radius, _Radius + 0.3, length(currentScreenPos - objScreenPos));
                
                half gray = dot(color, half3(0.2125, 0.7154, 0.0721));
                color = lerp(half3(gray, gray, gray), color, inRange);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}