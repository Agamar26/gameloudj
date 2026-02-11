Shader "Custom/PixelRetroToonOutline"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _Palette ("Palette Texture (1D)", 2D) = "white" {}
        _BaseColor ("Color Tint", Color) = (1,1,1,1)

        _PixelSize ("Pixel Size", Range(16,512)) = 128
        _ShadeSteps ("Cel Steps", Range(2,6)) = 3

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Size", Range(0.001,0.05)) = 0.02
        _OutlineDistanceFade ("Outline Distance Fade", Range(0.1,5)) = 1

        _LightThreshold ("Light Threshold", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        // ================= OUTLINE PASS =================
        Pass
        {
            Name "Outline"
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _OutlineWidth;
            float _OutlineDistanceFade;
            float4 _OutlineColor;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                float dist = distance(_WorldSpaceCameraPos, worldPos);

                float dynamicWidth = _OutlineWidth * saturate(1 / (dist * _OutlineDistanceFade));

                float3 pos = IN.positionOS.xyz + normalize(IN.normalOS) * dynamicWidth;
                OUT.positionHCS = TransformObjectToHClip(float4(pos,1));
                OUT.positionWS = worldPos;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // ================= TOON PIXEL PASS =================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_Palette);
            SAMPLER(sampler_Palette);

            float4 _BaseMap_ST;
            float4 _BaseColor;
            float _PixelSize;
            float _ShadeSteps;
            float _LightThreshold;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            float3 ApplyPalette(float3 color)
            {
                float luminance = dot(color, float3(0.299,0.587,0.114));
                float2 paletteUV = float2(luminance, 0.5);
                return SAMPLE_TEXTURE2D(_Palette, sampler_Palette, paletteUV).rgb;
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // ===== Pixelisation =====
                float2 uv = IN.uv * _PixelSize;
                uv = floor(uv) / _PixelSize;

                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                tex *= _BaseColor;

                // ===== Toon Lighting =====
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalize(IN.normalWS), normalize(mainLight.direction)));
                float toon = floor(NdotL * _ShadeSteps) / _ShadeSteps;

                float3 litColor = tex.rgb * toon * mainLight.color;

                // ===== Palette Mapping =====
               // if (_Palette != 0)
              //  litColor = ApplyPalette(litColor);

                return half4(litColor, tex.a);
            }

            ENDHLSL
        }
    }
}