Shader "Hidden/Pixellate Effect" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
    _ResolutionX ("X Resolution", float) = 0.0
    _ResolutionY ("Y Resolution", float) = 0.0
}

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off

CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform float _ResolutionX;
uniform float _ResolutionY;

fixed4 frag (v2f_img i) : SV_Target
{

    float screenPixelX = i.uv.x * floor(_ResolutionX);
    float screenPixelY = i.uv.y * floor(_ResolutionY);

    screenPixelX = floor(screenPixelX);
    screenPixelY = floor(screenPixelY);

    screenPixelX = screenPixelX / _ResolutionX;
    screenPixelY = screenPixelY / _ResolutionY;

	return tex2D(_MainTex, float2(screenPixelX, screenPixelY));
}
ENDCG

	}
}

Fallback off

}
