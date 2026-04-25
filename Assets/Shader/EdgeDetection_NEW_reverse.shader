Shader "Custom/EdgeDetection_reverse"
{
    Properties
    {
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)
        _InverseEdgeColor ("Inverse Edge Color", Color) = (0.9, 0.87, 0.82, 1)
        _NormalThreshold ("Normal Threshold", Float) = 0.4
        _DepthThreshold ("Depth Threshold", Float) = 0.05
        _Thickness ("Thickness", Float) = 1.0
        _DepthFadeStart ("Depth Fade Start", Float) = 50.0
        _DepthFadeEnd ("Depth Fade End", Float) = 100.0
        _InverseLumThreshold ("Inverse Lum Threshold", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque" 
        }

        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            Name "EDGE DETECTION"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            float4 _EdgeColor;
            float4 _InverseEdgeColor;
            float _NormalThreshold;
            float _DepthThreshold;
            float _Thickness;
            float _DepthFadeStart;
            float _DepthFadeEnd;
            float _InverseLumThreshold;

            float SampleLinearDepth(float2 uv)
            {
                return LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);

                float2 texelSize = _BlitTexture_TexelSize.xy * _Thickness;
                float2 uvB = uv + float2(-texelSize.x,  texelSize.y);
                float2 uvC = uv + float2( texelSize.x,  texelSize.y);
                float2 uvD = uv + float2(-texelSize.x, -texelSize.y);
                float2 uvE = uv + float2( texelSize.x, -texelSize.y);

                // 法线检测
                float3 nB = SampleSceneNormals(uvB);
                float3 nC = SampleSceneNormals(uvC);
                float3 nD = SampleSceneNormals(uvD);
                float3 nE = SampleSceneNormals(uvE);
                float normalEdge = max(
                    step(_NormalThreshold, length(nB - nE)),
                    step(_NormalThreshold, length(nC - nD))
                );

                // 深度检测
                float depthEdge = 0.0;
                float depthCenter = SampleLinearDepth(uv);
                if (normalEdge < 0.5)
                {
                    float depthAvg = (SampleLinearDepth(uvB) + SampleLinearDepth(uvC)
                                    + SampleLinearDepth(uvD) + SampleLinearDepth(uvE)) * 0.25;
                    float depthDiff = abs(depthAvg - depthCenter) / (depthCenter + 0.001);
                    depthEdge = step(_DepthThreshold, depthDiff);
                }

                float edge = saturate(normalEdge + depthEdge);

                // 远处淡出
                edge *= 1.0 - smoothstep(_DepthFadeStart, _DepthFadeEnd, depthCenter);

                // 暗部反射颜
                float luminance = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));
                float4 edgeColor = lerp(_InverseEdgeColor, _EdgeColor,
                                        step(_InverseLumThreshold, luminance));

                return lerp(col, edgeColor, edge);
            }
            ENDHLSL
        }
    }
}