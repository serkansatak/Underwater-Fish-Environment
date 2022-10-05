Shader "Unlit/variedTransparent" {
	Properties{
		_Color("Color", Color) = (1, 1, 1, 0.5)
		_TranspModify("Change Transparency", Range(0, 1)) = 1
		// user-specified RGBA color including opacity
	}
		SubShader{
			Tags{ "Queue" = "Transparent" }
			// draw after all opaque geometry has been drawn
			Pass{
			ZWrite Off // don't occlude other objects
			Blend SrcAlpha OneMinusSrcAlpha // standard alpha blending

			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag 

			#include "UnityCG.cginc"

			uniform float4 _Color; // define shader property for shaders
			float _TranspModify;

			struct vertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float3 normal : TEXCOORD0;
				float3 viewDir : TEXCOORD1;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				float4x4 modelMatrix = unity_ObjectToWorld;
				float4x4 modelMatrixInverse = unity_WorldToObject;

				output.normal = normalize(
					mul(float4(input.normal, 0.0), modelMatrixInverse).xyz);
				output.viewDir = normalize(_WorldSpaceCameraPos
					- mul(modelMatrix, input.vertex).xyz);

				output.pos = UnityObjectToClipPos(input.vertex);
				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				float3 normalDirection = normalize(input.normal);
				float3 viewDirection = normalize(input.viewDir);

				float newOpacity = 1 - min(1.0, _TranspModify
					/ abs(dot(viewDirection, normalDirection)));

		

				return float4(_Color.rgb, newOpacity * _Color.a);
			}

			ENDCG
			}
		}
}