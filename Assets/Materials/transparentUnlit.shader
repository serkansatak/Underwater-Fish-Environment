Shader "Unlit/transparentUnlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" }
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Pass
		{

		Cull Back // now render the front faces
		ZWrite Off // don't write to depth buffer 
				   // in order not to occlude other objects
		Blend SrcAlpha OneMinusSrcAlpha
		// blend based on the fragment's alpha value
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			float4 _Color;
		sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
            };



            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

			float4 frag (v2f i) : SV_Target
            {
				float4 textureColor = tex2D(_MainTex, i.uv.xy);
                // sample the texture
				//float4 col = _Color;

                // apply fog

                return textureColor * _Color;
            }
            ENDCG
        }
    }
}
