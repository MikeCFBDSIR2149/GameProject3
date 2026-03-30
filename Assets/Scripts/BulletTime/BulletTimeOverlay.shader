Shader "Custom/BulletTimeOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DarkColor ("Dark Color", Color) = (0.1,0.1,0.1,0.3)
        _Intensity ("Intensity", Range(0,1)) = 0
        _WaveCenter ("Wave Center", Vector) = (0.5, 0.5, 0, 0)
        _WaveRadius ("Wave Radius", Float) = 0.0
        _WaveWidth ("Wave Width", Float) = 0.1
        _WaveStrength ("Wave Strength", Float) = 0.5
        _WaveColor ("Wave Color", Color) = (1, 1, 0, 1) // 黄色
        _GlitchStrength ("Glitch Strength", Range(0,1)) = 0
        _GlitchLines ("Glitch Lines", Range(10,400)) = 120
        _GlitchSpeed ("Glitch Speed", Range(0,30)) = 10
        _GlitchBlockiness ("Glitch Blockiness", Range(1,40)) = 12
        _GlitchAmount ("Glitch Amount(px-ish)", Range(0,0.05)) = 0.02
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _DarkColor;
            float _Intensity;
            float4 _WaveCenter;
            float _WaveRadius;
            float _WaveWidth;
            float _WaveStrength;
            fixed4 _WaveColor;   // 新增
            float _GlitchStrength;
            float _GlitchLines;
            float _GlitchSpeed;
            float _GlitchBlockiness;
            float _GlitchAmount;
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
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 后面是给 UI 覆盖用的，这里只关心自身颜色
                float2 uv = i.uv;
                // ---------- Glitch / Tear (screen-space horizontal tearing) ----------
                float t = _Time.y * _GlitchSpeed;
                // 1) 横向条纹：把屏幕分成很多条
                float lineId = floor(uv.y * _GlitchLines);
                // 2) 让相邻条纹形成“块状分组”，更有撕裂感
                float blockId = floor(lineId / max(1.0, _GlitchBlockiness));
                // 3) 一个简单hash噪声（无需贴图）
                float n = frac(sin(blockId * 12.9898 + t * 78.233) * 43758.5453);
                // 4) 撕裂：对 x 做偏移（正负跳）
                float tear = (n * 2 - 1) * _GlitchAmount * _GlitchStrength;
                // 可选：让撕裂不是每条都有，做个阈值“间歇性断裂”
                float gate = step(0.55, n); // n>0.55 才撕裂
                uv.x += tear * gate;
                // 轻微的y方向抖动（可选，少量）
                float n2 = frac(sin((blockId + 17.0) * 4.13 + t * 31.7) * 951.1357);
                uv.y += (n2 * 2 - 1) * (_GlitchAmount * 0.15) * _GlitchStrength * gate;
                // 基础暗色（全屏变暗）
                fixed4 col = _DarkColor;
                col.a *= _Intensity; // 由 Intensity 控制透明度

                // 波纹：计算距离中心的半径
                float2 center = _WaveCenter.xy;
                float dist = distance(uv, center);

                // 在 [radius - width, radius + width] 范围内作为波纹带
                float band = smoothstep(_WaveRadius - _WaveWidth, _WaveRadius, dist)
                           - smoothstep(_WaveRadius, _WaveRadius + _WaveWidth, dist);

                // 防止 radius 收回到 0 时 band 仍残留
                if (_WaveRadius <= 0.001)
                {band = 0;
                }
                // 波纹的亮度（让这一圈略微比背景亮一些）
                fixed4 waveColor = _WaveColor * _WaveStrength;
                waveColor.a *= band * _Intensity; // 也受全局 Intensity 影响

                // 最终颜色 = 暗色 + 波纹
                fixed4 finalCol = col;
                // 叠加波纹的透明度/亮度
                finalCol.rgb += waveColor.rgb * waveColor.a;

                return finalCol;
            }
            ENDCG
        }
    }
}