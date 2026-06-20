Shader "Custom/ScreenNoiseOverlay"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
        _Opacity ("Opacity", Range(0,1)) = 0.12
        _NoiseScale ("Noise Scale", Range(10,1000)) = 260
        _NoiseSpeed ("Noise Speed", Range(0,50)) = 12
        _NoiseStrength ("Noise Strength", Range(0,1)) = 1
        _GrainPower ("Grain Power", Range(0.1,8)) = 4
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Tint;
            float _Opacity;
            float _NoiseScale;
            float _NoiseSpeed;
            float _NoiseStrength;
            float _GrainPower;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // 让噪点随时间轻微变化
                float t = _Time.y * _NoiseSpeed;

                // 把屏幕切成小格子，每格一个随机值
                float2 cell = floor(uv * _NoiseScale);
                float n1 = Hash21(cell + t);
                float n2 = Hash21(cell + 17.23 - t * 0.73);

                // 混合两层噪点，避免太规律
                float grain = (n1 + n2) * 0.5;

                // 让大多数区域很淡，偶尔有亮一点的颗粒
                grain = pow(saturate(grain), _GrainPower);

                float alpha = grain * _Opacity * _NoiseStrength;

                return fixed4(_Tint.rgb, alpha);
            }
            ENDCG
        }
    }
}