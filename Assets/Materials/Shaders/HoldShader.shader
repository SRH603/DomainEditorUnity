Shader "Custom/CutoffShader"
{
    Properties
    {
        _Cutoff("Cutoff X", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            float _Cutoff;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float clipX : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.clipX = v.vertex.x - _Cutoff; // �޳�x����С��_Cutoff�Ĳ���
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                clip(i.clipX); // ʹ��clip�����޳�
                return fixed4(1,1,1,1); // ��Ⱦʣ�ಿ��
            }
            ENDCG
        }
    }
}
