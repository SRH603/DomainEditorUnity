Shader "Custom/StylizedCrystal" {
    Properties {
        _MainColor ("Main Color", Color) = (0.5, 0.8, 1, 0.8)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.2
        _Transparency ("Transparency", Range(0, 1)) = 0.6
        _Specular ("Specular", Range(0, 5)) = 2.0
        _FresnelPower ("Fresnel Power", Range(0, 5)) = 3.0
        _WaveSpeed ("Wave Speed", Range(0, 2)) = 0.5
        _ParticleSpeed ("Particle Speed", Range(0, 2)) = 0.3
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _ParticleTex ("Particle Texture", 2D) = "white" {}
    }

    SubShader {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Crystal alpha:fade
        #pragma target 3.0

        struct Input {
            float3 viewDir;
            float2 uv_NoiseTex;
            float2 uv_ParticleTex;
        };

        fixed4 _MainColor;
        half _GlowIntensity;
        half _Transparency;
        half _Specular;
        half _FresnelPower;
        half _WaveSpeed;
        half _ParticleSpeed;
        sampler2D _NoiseTex;
        sampler2D _ParticleTex;

        half4 LightingCrystal(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
            half3 h = normalize(lightDir + viewDir);
            half diff = max(0, dot(s.Normal, lightDir));
            
            float fresnel = pow(1.0 - saturate(dot(s.Normal, viewDir)), _FresnelPower) * 1.5;
            float waveEffect = s.Gloss * 2.0;
            
            half4 c;
            c.rgb = (s.Albedo * _LightColor0.rgb * diff) + 
                   (_LightColor0.rgb * _Specular * pow(max(0, dot(s.Normal, h)), _Specular*50)) +
                    s.Emission * (1 + fresnel) +
                    _MainColor.rgb * fresnel * _GlowIntensity * waveEffect;
            c.a = s.Alpha * (0.8 + fresnel * 0.2);
            return c;
        }

        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = _MainColor.rgb;
            o.Alpha = _Transparency * _MainColor.a;

            float2 waveUV = IN.uv_NoiseTex + _Time.y * _WaveSpeed * float2(0.1, 0.2);
            float wave = tex2D(_NoiseTex, waveUV).r;
            
            float2 particleUV = IN.uv_ParticleTex + _Time.y * _ParticleSpeed * float2(-0.1, 0.15);
            float particles = tex2D(_ParticleTex, particleUV).r;
            
            o.Emission = _MainColor.rgb * (_GlowIntensity * 0.5 + wave * 0.3 + particles * 0.2);
            o.Specular = _Specular + wave * 0.2;
            o.Gloss = saturate(wave * 0.8 + particles * 0.5);
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}