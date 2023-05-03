Shader "Unlit/StateConfigurableDraw"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TextureBlendMode ("Texture blend mode", Float) = 4.5
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _SourceBlendFactor("Source blend factor", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestBlendFactor("Dest blend factor", Float) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZTest [_ZTest]
        Cull [_Cull]
        Blend [_SourceBlendFactor] [_DestBlendFactor], Zero One

        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color: COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color: COLOR;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            uniform float _Textureless;
            uniform float _TextureBlendMode;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.vertex.z = clamp(o.vertex.z, -o.vertex.w + 0.001, o.vertex.w - 0.001);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = (_Textureless > 0.5f) ? i.color : tex2D(_MainTex, i.uv);
                if (_Textureless > 0.5f) {
                    if (_TextureBlendMode >= 2) {
                        col *= i.color;
                    }
                    else if (_TextureBlendMode >= 4) {
                        col += i.color;
                    }
                }
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
