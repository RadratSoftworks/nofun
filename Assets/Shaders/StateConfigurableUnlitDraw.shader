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
            //#pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color: COLOR;
                float3 normal: NORMAL;
                float2 uv : TEXCOORD0;
                float4 specColor: TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color: COLOR;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal: NORMAL;
                float3 positionOrg: TEXCOORD1;
                float4 specColor: TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            uniform float _Textureless;
            uniform float _TextureBlendMode;
            uniform float _TransparentTest;

            // Lighting
            uniform float4x4 _LightMatrix;

            uniform float4 _GlobalAmbient;
            uniform float4 _CameraPos;

            uniform float4 _MaterialDiffuse;
            uniform float4 _MaterialSpecular;
            uniform float4 _MaterialAmbient;
            uniform float4 _MaterialEmission;
            uniform float _MaterialShininess;

            uniform float4 _LightPos[8];
            uniform float4 _LightDir[8];
            uniform float4 _LightDiffuse[8];
            uniform float4 _LightSpecular[8];
            uniform float _LightRRange[8];
            uniform float _LightExponent[8];
            uniform float _LightCutoffAngle[8];
            uniform float _LightType[8];
            uniform int _LightCount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.vertex.z = clamp(o.vertex.z, -o.vertex.w + 0.001, o.vertex.w - 0.001);
                o.color = v.color;
                o.specColor = v.specColor;
                o.normal = mul(transpose(_LightMatrix), v.normal);
                o.positionOrg = mul(transpose(_LightMatrix), v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f vt) : SV_Target
            {
                // sample the texture
                float4 texColour = float4(0, 0, 0, 0);
                if (_Textureless <= 0.5f) {
                    texColour = tex2D(_MainTex, vt.uv);
                    if ((_TransparentTest >= 0.5f) && (texColour.a == 0)) {
                        discard;
                    }
                }
                float4 fragmentColour = vt.color;
                if ((_LightCount >= 0) && (_TextureBlendMode >= 2))
                {
                    fragmentColour = _MaterialEmission + _GlobalAmbient * _MaterialAmbient;

                    for (int i = 0; i < _LightCount; i++) {
                        // Point = 1, directional = 2, spot = 4
                        int type = floor(_LightType[i]);
                        float3 dir = _LightDir[i].xyz;
                        
                        if (type != 2) {
                            dir = normalize(_LightPos[i].xyz - vt.positionOrg.xyz);
                        } else {
                            dir = normalize(-dir);
                        }
                        
                        float diffuseFactor = max(dot(dir, normalize(vt.normal)), 0);

                        // Calculate specular beforehand
                        float4 specular = float4(0, 0, 0, 0);

                        if (_CameraPos.w > 0.0f) {
                            float3 viewDir = normalize(_CameraPos - vt.positionOrg);
                            float3 reflectDir = reflect(-dir, vt.normal);
                            float specularFactor = max(dot(viewDir, reflectDir), 0);
                            specularFactor = exp(_MaterialShininess * log(specularFactor));

                            specular = _MaterialSpecular * _LightSpecular[i] * specularFactor * vt.specColor;
                        }

                        float spotConstant = 1.0;
                        if (type >= 4) {
                            // Spot direction, reversed to origin at the plane side out
                            float spotAngleCos = dot(dir, normalize(-_LightDir[i].xyz));
                            if (spotAngleCos < cos(_LightCutoffAngle[i])) {
                                // Outside of the spot cone
                                spotConstant = 0.0;
                            } else {
                                spotConstant = pow(spotAngleCos, _LightExponent[i]);
                            }
                        }

                        float power = 1.0;
                        if (type == 1) {
                            float dist = length(_LightPos[i].xyz - vt.positionOrg.xyz);
                            power = max((1.0 / _LightRRange[i] - dist) * _LightRRange[i], 0);
                        }

                        float4 diffuse = _MaterialDiffuse * _LightDiffuse[i] * diffuseFactor * vt.color;

                        fragmentColour += spotConstant * power * (diffuse + specular);
                    }
                }
                if (_Textureless <= 0.5f) {
                    if (_TextureBlendMode >= 4) {
                        fragmentColour += texColour;
                    } else if (_TextureBlendMode >= 2) {
                        fragmentColour *= texColour;
                    } else {
                        fragmentColour = texColour;
                    }
                }
                // apply fog
                UNITY_APPLY_FOG(vt.fogCoord, fragmentColour);
                return fragmentColour;
            }
            ENDCG
        }
    }
}
