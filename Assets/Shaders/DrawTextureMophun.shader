// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)


Shader "Normal/DrawTextureMophun"
{
    Properties { _MainTex ("Texture", any) = "" {} }

    CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 2.0

    #include "UnityCG.cginc"

    struct appdata_t {
        float4 vertex : POSITION;
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    sampler2D _MainTex;
    float4 _MainTex_ST;

    uniform float4x4 _TexToLocal;
    uniform float4 _Color;
    uniform float _Black_Transparent;

    v2f vert (appdata_t v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.texcoord = mul(_TexToLocal, float4(TRANSFORM_TEX(v.texcoord, _MainTex), 0, 1)).xy;
        o.color = _Color;

        return o;
    }

    fixed4 frag (v2f i) : SV_Target
    {
        fixed4 result = tex2D(_MainTex, i.texcoord);
        if (_Black_Transparent >= 0.5f) {
          if (all(result.rgb == fixed3(0.0, 0.0, 0.0))) {
            result.a = 0.0;
          }
        }
        return result * i.color;
    }
    ENDCG

    SubShader {

        Tags { "RenderType"="Overlay" }

        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha, One One
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }

    SubShader {

        Tags { "RenderType"="Overlay" }

        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }

    Fallback off
}
