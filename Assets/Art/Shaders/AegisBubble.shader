Shader "Custom/AegisBubble"
{
    Properties
    {
        [HDR] _CoreColor ("Core Color", Color) = (0.05, 0.65, 0.95, 0.10)
        [HDR] _RimColor ("Rim Color", Color) = (0.25, 0.95, 1.0, 0.78)
        _FresnelPower ("Fresnel Power", Range(0.5, 8.0)) = 3.2
        _RimIntensity ("Rim Intensity", Range(0.5, 4.0)) = 1.65
        _PulseSpeed ("Pulse Speed", Range(0.0, 10.0)) = 3.0
        _PulseAmount ("Pulse Amount", Range(0.0, 0.5)) = 0.12
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "AegisBubbleForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _CoreColor;
                half4 _RimColor;
                half _FresnelPower;
                half _RimIntensity;
                half _PulseSpeed;
                half _PulseAmount;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

                half ndotv = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(1.0h - ndotv, _FresnelPower);
                half pulse = 1.0h + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                half3 rgb = lerp(_CoreColor.rgb, _RimColor.rgb * (_RimIntensity * pulse), fresnel);
                half alpha = saturate(_CoreColor.a + fresnel * _RimColor.a * pulse);

                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
