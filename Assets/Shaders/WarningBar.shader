Shader "Custom/WarningBar"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _BaseTextureFactor("Base Texture Factor", Range(0,1)) = 0
        _WarningBarColor1("Warning Bar Color 1", Color) = (0.9, 0.9, 0, 1)
        _WarningBarColor2("Warning Bar Color 2", Color) = (0, 0, 0, 1)
        _WarningStripeTiling("Warning Stripe Tiling", Float) = 12
        _WarningStripeAngle("Warning Stripe Angle", Range(-180, 180)) = 45
        _WarningStripeOffset("Warning Stripe Offset", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

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

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _BaseTextureFactor;
                half4 _WarningBarColor1;
                half4 _WarningBarColor2;
                float _WarningStripeTiling;
                float _WarningStripeAngle;
                float _WarningStripeOffset;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                float stripeAngleRad = radians(_WarningStripeAngle);
                float2 stripeDirection = float2(cos(stripeAngleRad), sin(stripeAngleRad));
                float stripeCoord = dot(IN.uv, stripeDirection) * _WarningStripeTiling + _WarningStripeOffset;

                // Alternate two colors in diagonal bands.
                half stripeMask = step(0.5, frac(stripeCoord));
                half4 warningColor = lerp(_WarningBarColor1, _WarningBarColor2, stripeMask);

                return lerp(warningColor, baseColor, saturate(_BaseTextureFactor));
            }
            ENDHLSL
        }
    }
}
