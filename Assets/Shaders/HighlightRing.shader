Shader "Custom/HighlightRing"
{
    Properties
    {
        // BaseColor and BaseMap removed as requested; shader renders only the wave highlight
        _WaveColor("Wave Color", Color) = (1, 1, 1, 1)
        _WaveSpeed("Wave Speed", Float) = 1
        _WaveMaxRadius("Wave Max Radius", Range(0,1)) = 0.5
        _WaveThickness("Wave Thickness", Float) = 0.05
        _WaveCenter("Wave Center", Vector) = (0.5, 0.5, 0, 0)
        _WaveIntensity("Wave Intensity", Float) = 1
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

            // no base texture

            CBUFFER_START(UnityPerMaterial)
                // removed _BaseColor and _BaseMap_ST
                float4 _WaveColor;
                float4 _WaveCenter;
                float _WaveSpeed;
                float _WaveMaxRadius;
                float _WaveThickness;
                float _WaveIntensity;
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

                // Time-driven ping-pong wave position (0 -> _WaveMaxRadius -> 0)
                float t = frac(_Time.y * _WaveSpeed);
                float wavePos = abs(t * 2.0 - 1.0) * _WaveMaxRadius;

                // Inner-to-wave alpha: highest at center, lower at the wave edge
                float inner = 0.0;
                if (wavePos > 0.0001)
                {
                    inner = saturate((wavePos - dist) / wavePos);
                }

                // Make the wave edge have a softer falloff based on thickness
                float edgeFalloff = 1.0 - smoothstep(wavePos - _WaveThickness, wavePos, dist);

                // Combined alpha for the wave: stronger near center, weaker at the ring
                float waveAlpha = inner * edgeFalloff * _WaveIntensity;

                // Compute final color and alpha. Since we render to a transparent target,
                // output alpha should represent wave visibility so blending works.
                half4 waveCol = _WaveColor;
                half3 rgb = waveCol.rgb * waveAlpha * waveCol.a;
                float alpha = waveAlpha * waveCol.a;
                half4 outCol = half4(rgb, alpha);

                return outCol;
            }
            ENDHLSL
        }
    }
}
