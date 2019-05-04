Shader "Legacy Shaders/LUTs Fast Mobile Shader 2.0" {
	Properties
	{
		_MainTex("_MainTex", any) = "" {}
		_LUT1("_LUT1", 2D) = "defaulttexture" {}
		_LUT1_params("_LUT1_params", Vector) = (0,0,0,0)
		_LUT1_Amount("_LUT1_Amount", Float) = 1.0

		_LUT2("_LUT2", 2D) = "defaulttexture" {}
		_LUT2_params("_LUT2_params", Vector) = (0,0,0,0)
		_LUT2_Amount("_LUT2_Amount", Float) = 1.0

		_bright("_bright", Float) = 1.0
		_sat("_sat", Float) = 1.0
		_cont("_cont", Float) = 1.0
		_glowAmount("_glowAmount", Float) = 1.0
		_glowRadius("_glowRadius", Float) = 1.0
		_glowTreshold("_glowTreshold", Float) = 0.75
	}

	SubShader
	{
		Pass 
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma multi_compile __ USE_LUT2
			#pragma multi_compile __ USE_BRIGHT
			#pragma multi_compile __ USE_GLOW
			#pragma multi_compile __ FIX_OVEREXPO
			#pragma multi_compile __ GLOW_BLEACH
			
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma fragmentoption ARB_precision_hint_fastest

			UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
			uniform fixed4 _MainTex_ST;

			uniform sampler2D _LUT1;
			//uniform fixed4 _LUT1_ST;
			uniform fixed4 _LUT1_params;
			uniform fixed _LUT1_Amount;

	#if USE_LUT2
			uniform sampler2D _LUT2;
			uniform fixed4 _LUT2_params;
			uniform fixed _LUT2_Amount;
	#endif




	#if USE_GLOW
			fixed _glowAmount;
			fixed _glowRadius;
			fixed _glowTreshold;
	#endif


	#if USE_BRIGHT
			fixed _bright;
			fixed _sat;
			fixed _cont;
			static const fixed3 LuminaceCoeff = fixed3(0.2125, 0.7153, 0.1721);
			static const fixed3 AvgLimin = fixed3(0.5, 0.5, 0.5);
	#endif

			struct appdata_t {
				fixed4 vertex : POSITION;
				fixed2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};//appdata_t

			struct v2f {
				fixed4 vertex : SV_POSITION;
				fixed2 uv : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};//v2f

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv.xy, _MainTex);
				return o;
			}//vert




			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 color = saturate(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv));


				fixed3 lut1result = color;
#if !UNITY_COLORSPACE_GAMMA
				lut1result = LinearToGammaSpace(lut1result);
#endif

#if defined(FIX_OVEREXPO)
				fixed3 maxvalue1 = saturate(lut1result - 1);
				lut1result = saturate(lut1result);
#endif
				fixed3 scaleOffset1 = _LUT1_params.xyz;
				lut1result.z *= scaleOffset1.z;
				fixed shift1 = floor(lut1result.z);
				lut1result.xy = (lut1result.xy * scaleOffset1.z)*scaleOffset1.xy;
				lut1result.x += shift1 * scaleOffset1.y;
				lut1result = tex2D(_LUT1, lut1result.xy).rgb;
#if defined(FIX_OVEREXPO)
				lut1result += maxvalue1;
#endif

#if !UNITY_COLORSPACE_GAMMA
				lut1result = GammaToLinearSpace(lut1result);
#endif
				color.rgb = lerp(color.rgb, lut1result, _LUT1_Amount);

	#if USE_LUT2

				fixed3 lut2result = color;
#if !UNITY_COLORSPACE_GAMMA
				lut2result = LinearToGammaSpace(lut2result);
#endif

#if defined(FIX_OVEREXPO)
				fixed3 maxvalue2 = saturate(lut2result - 1);
				lut2result = saturate(lut2result);
#endif
				fixed3 scaleOffset2 = _LUT2_params.xyz;
				lut2result.z *= scaleOffset2.z;
				fixed shift2 = floor(lut2result.z);
				lut2result.xy = (lut2result.xy * scaleOffset2.z)*scaleOffset2.xy;
				lut2result.x += shift2 * scaleOffset2.y;
				lut2result = tex2D(_LUT2, lut2result.xy).rgb;
#if defined(FIX_OVEREXPO)
				lut2result += maxvalue2;
#endif
				color.rgb = lerp(color.rgb, lut2result, _LUT2_Amount);

#if !UNITY_COLORSPACE_GAMMA
				color = GammaToLinearSpace(color);
#endif
	#endif





#if USE_GLOW
				//color.b = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, fixed2(i.uv.x + 0.0025, i.uv.y)).b;
				//color.g = tex2D(_MainTex, fixed2(i.uv.x - 0.0005, i.uv.y)).g;
				fixed offset = 0.004 * _glowRadius;
				fixed4 dt = fixed4(1,1,1,1);
				fixed v1 = dot(dt, fixed4( UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, fixed2(i.uv.x + offset, i.uv.y)).r,
				UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, fixed2(i.uv.x - offset, i.uv.y)).r,
				UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, fixed2(i.uv.x, i.uv.y + offset)).r,
				UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, fixed2(i.uv.x, i.uv.y - offset)).r));

				v1 = saturate(v1 - _glowTreshold) / saturate(2 - _glowTreshold);
				//fixed res = max(max(max(v1, v2), v3), v4);
				/*if (v1 > 0.65) {
					color.r += v1 * v1 / 2 * _glowAmount / 7;
					color.g += v1 * v1 / 10 * _glowAmount / 7;
				}*/

#if !GLOW_BLEACH
				color +=  color * v1 * _glowAmount;
#else
				color +=  v1 * _glowAmount;
#endif
#endif



	#if USE_BRIGHT
			

				fixed3 brtColor = lerp(AvgLimin, color.rgb, _cont) * _bright;

				fixed intensityf = dot(brtColor, LuminaceCoeff);
				fixed3 int33 = fixed3(intensityf, intensityf, intensityf);

				fixed lrpbrt = (_sat - 1)*(color.b*color.r*0.5 + 0.5) + 1;
				color.rgb = lerp(int33, brtColor, lrpbrt);
	#endif




				return color;
			}//frag




			ENDCG
		}//Pass

	}//SubShader

	Fallback Off
}//Shader
