Shader "VolumetricLine/SingleLine-URP"
{
    Properties
    {
        [NoScaleOffset] _MainTex("Base (RGB)", 2D) = "white" {}
        _LineWidth("Line Width", Range(0.01, 100)) = 1.0
        _LineScale("Line Scale", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "ForceNoShadowCasting" = "True"
        }

        LOD 200

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            Lighting Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _MainTex;
            float _LineWidth;
            float _LineScale;

            // Use URP built-in _Time instead of custom
            // float4 _Time; // DO NOT define this!

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.pos = TransformObjectToHClip(IN.vertex);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = tex2D(_MainTex, IN.uv);
                
                // Optional: animate alpha with built-in _Time.x
                col.a *= sin(_Time.x * 2.0) * 0.5 + 0.5;

                return col;
            }

            ENDHLSL
        }
    }

    FallBack "Universal Forward"
}
