Shader "Unlit/EdgeTransparency"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Transparency ("Transparency", Range(0.0, 1.0)) = 1.0
        _CenterPoint ("Center Point", Vector) = (0.5, 0.5, 0.0, 0.0)
        _MinTransparency("Minimum Transparency", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Tags {  "Queue" = "Transparent" "RenderType" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Transparency;
            float _MinTransparency;
            float4 _CenterPoint;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col.w = col.w * _Transparency;
                float dist = (1 - 2 * distance(i.uv, float2(_CenterPoint.x, _CenterPoint.y)));

                if (dist < 0) {
                    dist = 0;
                }

                col.w = dist * _Transparency + _MinTransparency;

                col = float4(i.uv.x, i.uv.y, 0, 1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
