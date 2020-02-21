Shader "CamShaderTwo"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_DistortAmount("Amount", range(0, 1000)) = 10
		_SobelAmount("Sobel Operator", range(0, 1)) = 0.1
		_LineWidth("Line Width", range(0.5, 2)) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

			static const fixed3 greyFunction = fixed3(0.2126, 0.7152, 0.0722);
			static const fixed4 black = fixed4(0, 0, 0, 1);
            sampler2D _MainTex;
			float _DistortAmount;
			float _SobelAmount;
			float _LineWidth;

			void SobelOperator(inout float n[9], sampler2D tex, float2 pos) {
				float w = 1.0f/_ScreenParams.x;
				float h = 1.0f/_ScreenParams.y;

				fixed4 pixel = tex2D(tex, pos + float2(-w, h));
				n[0] = dot(pixel.rgb, greyFunction);

				pixel = tex2D(tex, pos + float2(0, h));
				n[1] = dot(pixel.rgb, greyFunction);

				pixel = tex2D(tex, pos + float2(w, h));
				n[2] = dot(pixel.rgb, greyFunction);

				pixel = tex2D(tex, pos + float2(-w, 0));
				n[3] = dot(pixel.rgb, greyFunction);

				pixel = tex2D(tex, pos + float2(w, 0));
				n[5] = dot(pixel.rgb, greyFunction);

				pixel = tex2D(tex, pos + float2(-w, -h));
				n[6] = dot(pixel.rgb, greyFunction);

				pixel = tex2D(tex, pos + float2(0, -h));
				n[7] = dot(pixel.rgb, greyFunction);

				pixel = tex2D(tex, pos + float2(w, -h));
				n[8] = dot(pixel.rgb, greyFunction);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float n[9];
				SobelOperator(n, _MainTex, i.uv);

				half Sx = (n[2] + (2.0 * n[5]) + n[8] - n[0] - (2.0 * n[3]) - n[6])* _LineWidth;
				half Sy = (n[6] + (2.0 * n[7]) + n[8] - n[0] - (2.0 * n[1]) - n[2])*_LineWidth;
				half sobel = sqrt((Sx * Sx) + (Sy * Sy));
				half4 greyscale = lerp(col, black, step(0, sobel - _SobelAmount));
                return greyscale;
            }
            ENDCG
        }
    }
}
