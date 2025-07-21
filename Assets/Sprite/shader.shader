Shader "Custom/SpriteBrightKeepColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Brightness ("Brightness", Range(0.1, 5.0)) = 1.0
        _Saturation ("Saturation", Range(0, 3)) = 1.5
        _GlowIntensity ("Glow Intensity", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha  // Alpha blend để giữ màu
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Brightness;
            float _Saturation;
            float _GlowIntensity;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                
                // Giữ màu gốc, chỉ tăng độ sáng
                col.rgb *= _Color.rgb * _Brightness;
                
                // Tăng độ bão hòa màu
                float3 gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(gray, col.rgb, _Saturation);
                
                // Thêm một chút glow nhẹ
                col.rgb += col.rgb * _GlowIntensity * col.a;
                
                // Clamp để tránh quá sáng
                col.rgb = saturate(col.rgb);
                
                col.a *= _Color.a;
                
                return col;
            }
            ENDCG
        }
    }
}