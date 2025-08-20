Shader "Hidden/Blackout/AnimationCurveGrid" 
{
    Properties
   {
      [Header(Colors)]
      _PrimaryColor("Main Grid Color", Color) = (1, 1.0, 1.0, 0.5)
      _SecondaryColor("Secondary Grid Color", Color) = (1, 1, 1, 0.1)
      
      [Header(Grid)]      
      _GridPixels("Pixels Per Cell", Vector) = (50, 50, 0, 0)
      _PrimaryOffset("Primary Line Cell Offset", Int) = 5
      _Thickness("Lines Thickness (pixels)", Vector) = (2.0, 2.0, 0.0, 0.0)
      
      [HideInInspector] _Color("Main Color", Color) = (1.0, 1.0, 1.0, 1.0)
      [PerRendererData] [HideInInspector] _MainTex ("Sprite Texture", 2D) = "white" {}
      [HideInInspector] _MaskTexture("Texture", 2D) = "white" {}
      [HideInInspector] _Stencil("Stencil ID", Float) = 0
      [HideInInspector] _StencilComp("StencilComp", Float) = 8
      [HideInInspector] _StencilOp("StencilOp", Float) = 0
      [HideInInspector] _StencilReadMask("StencilReadMask", Float) = 255
      [HideInInspector] _StencilWriteMask("StencilWriteMask", Float) = 255
      [HideInInspector] _ColorMask("ColorMask", Float) = 15
   }
   SubShader
   {
      LOD 0
      
      Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }  
          
      Stencil
      {
         Ref [_Stencil]
         ReadMask [_StencilReadMask]
         WriteMask [_StencilWriteMask]
         Comp [_StencilComp]
         Pass [_StencilOp]
      }  

      Cull Off
      Lighting Off
      ZWrite Off
      ZTest [unity_GUIZTestMode]
      Blend One OneMinusSrcAlpha
      ColorMask [_ColorMask]

      ZWrite On // We need to write in depth to avoid tearing issues
      Blend SrcAlpha OneMinusSrcAlpha

      Pass
      {
         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag
         #pragma target 3.0

         #include "UnityCG.cginc"
         #include "UnityUI.cginc"

         #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
         #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

         struct appdata
         {
            float4 vertex  : POSITION;
            float4 color   : COLOR;
            float2 uv      : TEXCOORD0;
            float2 uv1     : TEXCOORD1;
            UNITY_VERTEX_INPUT_INSTANCE_ID
         };

         struct v2f
         {
            float4 vertex        : SV_POSITION;
            fixed4 color         : COLOR;
            float2 uv            : TEXCOORD0;
            float4 worldPosition : TEXCOORD1;
            float4 mask          : TEXCOORD2;
            UNITY_VERTEX_OUTPUT_STEREO
         };

         fixed4 _Color;
         sampler2D _MainTex;
         fixed4 _TextureSampleAdd;
         float4 _ClipRect;
         float4 _MainTex_ST;
         float _UIMaskSoftnessX;
         float _UIMaskSoftnessY;

         float2 _Cells;
         float2 _CellUV;         
         float2 _LocalScale;         
         float2 _Thickness;
         fixed4 _PrimaryColor;
         fixed4 _SecondaryColor;

         v2f vert (appdata v)
         {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            
            v.vertex.xyz +=  float3( 0, 0, 0 );

            float4 vPosition = UnityObjectToClipPos(v.vertex);
            
            o.worldPosition = v.vertex;
            o.vertex = vPosition;

            float2 pixelSize = vPosition.w;
            pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));            
            
            float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
            
            o.uv = v.uv - 0.5f;            
            
            o.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

            o.color = v.color * _Color;

            return o;
         }

         float2 distanceToLine(float2 uv, float2 p, float2 thickness)
         {
            return abs(uv - p) < thickness ? 1.0 : 0.0;
         }
         
         float invLerp(float from, float to, float value){
            return clamp((value - from) / (to - from), 0.0, 1.0);
         }

         fixed4 frag (v2f i) : SV_Target
         {            
            half4 color = half4(0,0,0,0);

            float2 primaryUv = (_CellUV * 10.0) * -1.0;
            float2 closestLine = (round(i.uv/primaryUv) * primaryUv) + (primaryUv * 0.5);
            float2 pos = distanceToLine(i.uv, closestLine, _Thickness);

            float minScale = min(_LocalScale.x, _LocalScale.y);
            
            // Primary center axis lines every 10 cells
            if (pos.x == 1 || pos.y == 1)            
               color = _PrimaryColor;            
            else
            {
               // Secondary lines every cell
               closestLine = round(i.uv/_CellUV) * _CellUV;               
               pos = distanceToLine(i.uv, closestLine, _Thickness * 0.5f);  
               if (pos.x == 1 || pos.y == 1)               
               {
                  color = _SecondaryColor;
                  color.a *= clamp(invLerp(0.0, 2.5, minScale), 0.0, 1.0);
               }
               else
               {
                  // Inner cells (10 per cell)
                  float2 inner = _CellUV * 0.1;                  
                  closestLine = round(i.uv/inner) * inner;
                  pos = distanceToLine(i.uv, closestLine, _Thickness * 0.5f);  
                  if (pos.x == 1 || pos.y == 1)               
                  {
                     color = _SecondaryColor;
                     color.a *= 0.5;
                     color.a *= clamp(invLerp(1.25, 5.0, minScale), 0.0, 1.0);
                  }                  
               }                           
            }

            #ifdef UNITY_UI_CLIP_RECT
            half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(i.mask.xy)) * i.mask.zw);
            color.a *= m.x * m.y;
            #endif

            #ifdef UNITY_UI_ALPHACLIP
            clip (color.a - 0.001);
            #endif
            
            return color * i.color.a;
         }

         ENDCG
      }
   }
}
