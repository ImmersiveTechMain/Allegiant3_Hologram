Shader "Unlit/TornadoStripShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_YSpeed ("YSpeed", float) = 1
		_XSpeed ("XSpeed", float) = 1

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
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
			float _XSpeed;
			float _YSpeed;
			float4 _Color;

            v2f vert (appdata v)
            {
			   if(v.vertex.y < 0)
			   {
			   	   v.vertex.x *= 0;
			   	   v.vertex.z *= 0;
			   }

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
			float2 uv = i.uv;
			uv.x -= _Time.y * _XSpeed;
			uv.y -= _Time.y * _YSpeed;
			uv.x = uv.x - floor(uv.x);
			uv.y = uv.y - floor(uv.y);
                // sample the texture
                float4 col = tex2D(_MainTex, uv) * _Color;
                
                
                return col;
            }
            ENDCG
        }
    }
}
