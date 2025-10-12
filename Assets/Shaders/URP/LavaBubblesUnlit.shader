Shader "Custom/URP/LavaBubblesUnlit"
{
    Properties
    {
        [HDR]_HotColor ("Hot Lava Color", Color) = (1.0, 0.38, 0.08, 1.0)
        [HDR]_CoolColor ("Cool Lava Color", Color) = (0.08, 0.02, 0.01, 1.0)
        [HDR]_FoamColor ("Bubble Crest Color", Color) = (1.0, 0.85, 0.55, 1.0)
        _Tiling ("World Tiling", Float) = 0.25
        _BubbleDensity ("Bubble Density", Float) = 1.3
        _BubbleContrast ("Bubble Contrast", Float) = 3.5
        _FlowSpeed ("Flow Speed", Float) = 0.35
        _Distortion ("Distortion Strength", Float) = 0.45
        _RippleScale ("Ripple Scale", Float) = 2.4
        _RippleSpeed ("Ripple Speed", Float) = 1.1
        _EmissionIntensity ("Emission Intensity", Float) = 3.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalRenderPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _HotColor;
                float4 _CoolColor;
                float4 _FoamColor;
                float _Tiling;
                float _BubbleDensity;
                float _BubbleContrast;
                float _FlowSpeed;
                float _Distortion;
                float _RippleScale;
                float _RippleSpeed;
                float _EmissionIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float2x2 m = float2x2(1.7, -1.3, 1.3, 1.7);

                [unroll]
                for (int i = 0; i < 5; i++)
                {
                    value += noise(p) * amplitude;
                    p = mul(m, p);
                    amplitude *= 0.5;
                }

                return value;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float3 positionWS = IN.positionWS;
                float3 normalWS = normalize(IN.normalWS);

                float2 worldXZ = positionWS.xz * _Tiling;
                float time = _Time.y * _FlowSpeed;

                float2 flow1 = worldXZ * _BubbleDensity + float2(time * 0.6, time * 0.3);
                float2 flow2 = worldXZ * (_BubbleDensity * 1.7) - float2(time * 0.25, time * 0.4);

                float baseLayer = fbm(flow1);
                float detailLayer = fbm(flow2 + _Distortion * float2(
                    fbm(worldXZ * 2.3 + time),
                    fbm(worldXZ * 2.3 - time)
                ));

                float bubblePattern = saturate(baseLayer * 0.75 + detailLayer * 0.35);
                float bubbleMask = saturate(pow(bubblePattern, _BubbleContrast));

                float ripple = sin((worldXZ.x + worldXZ.y) * _RippleScale + time * _RippleSpeed);
                ripple = ripple * 0.25 + 0.5;

                float bubblePeaks = saturate(pow(bubbleMask * ripple, 3.5));
                float3 lavaColor = lerp(_CoolColor.rgb, _HotColor.rgb, bubbleMask);
                float3 bubbleHighlight = lerp(lavaColor, _FoamColor.rgb, bubblePeaks);

                float3 viewDirWS = normalize(GetWorldSpaceViewDir(positionWS));
                float rim = pow(saturate(1.0 - dot(normalWS, viewDirWS)), 2.5);

                float3 emission = bubbleHighlight;
                emission += _FoamColor.rgb * rim * 0.2;
                emission *= _EmissionIntensity;

                return half4(emission, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
