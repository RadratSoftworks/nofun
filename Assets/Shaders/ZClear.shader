Shader "Unlit/ZClear"
{
    Properties
    {
        _ClearValue ("Clear value", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        ZWrite On
        ZTest Always
        ColorMask 0
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            uniform float _ClearValue;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = (v.vertex * 2) - 1;
                o.vertex.zw = float2(_ClearValue, 1);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed(0);
            }
            ENDCG
        }
    }
}
