Shader "UI/ImageWithAlphaMask"
{
    Properties
    {
        _MainTex ("Color Image (B)", 2D) = "white" {}
        _MaskTex ("Alpha Mask (A)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
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
            sampler2D _MaskTex;

            float4 _MainTex_ST; // 关键：主图的缩放 + 偏移
            float4 _MaskTex_ST; // 遮罩图的缩放 + 偏移（通常不变，但加上更安全）

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uvMain : TEXCOORD0;
                float2 uvMask : TEXCOORD1;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // 关键：将UV坐标转换为贴图的实际裁剪区域
                o.uvMain = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvMask = TRANSFORM_TEX(v.uv, _MaskTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uvMain);
                fixed4 mask  = tex2D(_MaskTex, i.uvMask);

                color.a *= mask.a; // 遮罩 alpha 相乘
                return color;
            }
            ENDCG
        }
    }
}
