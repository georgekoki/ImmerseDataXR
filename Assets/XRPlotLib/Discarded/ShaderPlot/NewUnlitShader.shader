Shader "Unlit/NewUnlitShader"
{

    Properties
    {
         _testPoint("Test Point", Vector) = (0, 0, 0, 1)
         _TexData("Data", 2D) = "white" {}
        
    }
        SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct v2f {

            };

            v2f vert(
                float4 vertex : POSITION, // vertex position input
                out float4 outpos : SV_POSITION // clip space position output
                )
            {
                v2f o;
                outpos = UnityObjectToClipPos(vertex);
                return o;
            }

            float4 _testPoint;
            sampler2D _TexData;
            half4 _TexData_TexelSize;
            float4 _TexData_ST;

            fixed4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
            {
                float4 currPoint = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float4 c = float4(0.0f, 0.0f, 0.0f, 0.0f);

                float normalizedScreenPosX = screenPos.x / _ScreenParams.x;
                float normalizedScreenPosY = screenPos.y / _ScreenParams.y;
                [loop]
                for (int x = 0; x < _TexData_TexelSize.z; x++) {
                    [loop]
                    for (int y = 0; y < _TexData_TexelSize.w; y++) {

                        float normalizedX = x / _TexData_TexelSize.z;
                        float normalizedY = y / _TexData_TexelSize.w;

                        currPoint = tex2D(_TexData, float2(normalizedX, normalizedY));
                        currPoint.x = (currPoint.x ) * 2;
                        currPoint.y = (currPoint.y ) * 2;
                        currPoint.z = (currPoint.z ) * 2;

                        fixed4 offsetPos = ComputeScreenPos(UnityObjectToClipPos(currPoint));
                        offsetPos = offsetPos / offsetPos.w;


                        if (normalizedScreenPosX - offsetPos.x < 1 / _ScreenParams.x &&
                            normalizedScreenPosY - offsetPos.y < 1 / _ScreenParams.y &&
                            normalizedScreenPosX - offsetPos.x > - 1 / _ScreenParams.x &&
                            normalizedScreenPosY - offsetPos.y > -1 / _ScreenParams.y

                            ) {
                            return currPoint;
                        }
                    }
                }

                return c;
            }
            ENDCG
        }
    }

}

/*


{
    Properties
    {
        _testPoint("formula Offset", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 screenPos : TEXCOORD1;
            };

            float4 _testPoint;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.screenPos = ComputeScreenPos(o.pos);

                return o;
            }

            fixed4 frag (v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
            {
                fixed4 col = ComputeScreenPos(UnityObjectToClipPos(_testPoint)) - screenPos;
                return col;
            }
            ENDCG
        }
    }
}


*/