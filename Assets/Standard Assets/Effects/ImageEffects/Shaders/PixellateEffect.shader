Shader "Hidden/Pixellate Effect" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
    _ResolutionX ("X Resolution", float) = 0.0
    _ResolutionY ("Y Resolution", float) = 0.0

	_PaletteColorCount ("Palette Color Count", float) = 0.0
	_Palette ("Base (RGB)", 2D) = "white" {}

}

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			Texture2D _MainTex;
			SamplerState _MainTex_Point_Repeat_Sampler;

			uniform float _ResolutionX;
			uniform float _ResolutionY;

			uniform float _PaletteColorCount;
			uniform sampler2D _Palette;

			float FastDistance(float4 a, float4 b){
				float4 d = a - b;
				return (d.x * d.x) + (d.y * d.y) + (d.z * d.z);
			}

			fixed4 frag (v2f_img i) : SV_Target {
			    float screenPixelX = i.uv.x * floor(_ResolutionX);
			    float screenPixelY = i.uv.y * floor(_ResolutionY);

			    screenPixelX = floor(screenPixelX);
			    screenPixelY = floor(screenPixelY);

			    screenPixelX = screenPixelX / _ResolutionX;
			    screenPixelY = screenPixelY / _ResolutionY;

				float4 col = _MainTex.Sample(_MainTex_Point_Repeat_Sampler, float2(screenPixelX, screenPixelY));

				if(_PaletteColorCount <= 0){
					return col;
				} else {
					float halfPalletePixelWidth = (1.0 / _PaletteColorCount) / 2.0f;
					float nearestColorDistance = 999999.0;
					float4 nearestColor = float4(0.0, 0.0, 0.0, 0.0);

					int paletteColorCountInt = (int)(_PaletteColorCount);

					for(int p = 0; p < paletteColorCountInt; p++){
						float2 palleteUv = float2((p / _PaletteColorCount) + halfPalletePixelWidth, 0.5);
						float4 paletteColor = tex2D(_Palette, palleteUv);
						float colorDistance = FastDistance(col, paletteColor);

						if(colorDistance <= nearestColorDistance){
							nearestColorDistance = colorDistance;
							nearestColor = paletteColor;
						}
					}

					return nearestColor;
				}
			}
		ENDCG

		}
	}

	Fallback off
}
