Shader "Custom/HighlightRing"
{
    Properties
    {
        // BaseColor and BaseMap removed as requested; shader renders only the wave highlight
        // Keep a _MainTex property so Unity UI system doesn't warn (Canvas expects _MainTex)
        _MainTex("Main Texture", 2D) = "white" {}
        _WaveColor("Wave Color", Color) = (1, 1, 1, 1)
        _WaveSpeed("Wave Speed", Float) = 1
        _WaveMaxRadius("Wave Max Radius", Range(0,1)) = 0.5
        _WaveCenter("Wave Center", Vector) = (0.5, 0.5, 0, 0)
        _MinAlpha("Min Alpha", Range(0,1)) = 0.1
        _MaxAlpha("Max Alpha", Range(0,1)) = 0.8
        _CircleRadius("Circle Radius", Range(0,1)) = 0.5
    }

    SubShader
    {
        // Use Transparent tags/queue so alpha blending works when used on UI
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // keep a MainTex declaration so UI/CanvasRenderer stops warning; we won't sample it.
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                // removed _BaseColor and _BaseMap_ST
                float4 _WaveColor;
                float4 _WaveCenter;
                float _WaveSpeed;
                float _WaveMaxRadius;
                float _MinAlpha;
                float _MaxAlpha;
                float _CircleRadius;
                float _UnscaledTime;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // No base texture/color: start from transparent background (no base color variable needed)

                // Calculate radial distance from the configured wave center (UV space)
                float2 center = _WaveCenter.xy;
                float dist = distance(IN.uv, center);

                // Hard clip outside a circle so the UI never shows a square silhouette.
                clip(_CircleRadius - dist);

                // Time-driven sawtooth wave position: expand from 0 -> _WaveMaxRadius, then jump back to 0
                float t = frac(_UnscaledTime * _WaveSpeed);
                // Ensure the wave peak never exceeds the circle radius
                float maxR = min(_WaveMaxRadius, _CircleRadius);
                float wavePos = t * maxR;

                // (removed previous inner/edgeFalloff attenuation - using only user formula below)

                // User-specified alpha behavior:
                // radius = _CircleRadius, x* = wavePos, x = dist
                float radius = _CircleRadius;
                float x = dist;
                float xStar = wavePos;
                float baseAlpha;
                if (x <= xStar)
                {
                    // lerp(minAlpha, maxAlpha, (radius-x*+x)/radius)
                    float fracVal = (radius - xStar + x) / radius;
                    baseAlpha = lerp(_MinAlpha, _MaxAlpha, saturate(fracVal));
                }
                else
                {
                    baseAlpha = _MinAlpha;
                }

                // Use only the user's baseAlpha as final alpha (optionally modulated by wave color alpha)
                float finalAlpha = baseAlpha;

                half4 waveCol = _WaveColor;
                half3 rgb = waveCol.rgb * finalAlpha;
                float outA = finalAlpha * waveCol.a;
                half4 outCol = half4(rgb, outA);

                return outCol;
            }
            ENDHLSL
        }
    }
}
