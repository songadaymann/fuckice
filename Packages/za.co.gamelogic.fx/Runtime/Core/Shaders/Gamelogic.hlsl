#ifndef GAMELOGIC_HLSL_INCLUDED
#define GAMELOGIC_HLSL_INCLUDED
/*
	Pipeline specifics
*/
#if defined(UNITY_PIPELINE_URP)
	//#error "UNITY_PIPELINE_URP"

	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	#define SAMPLE(tex, uv) SAMPLE_TEXTURE2D(tex, sampler##tex, uv)
	#define TO_CLIP(pos) TransformObjectToHClip(pos)
	#define INPUT Varyings

	#define DECLARE_TEX(name) \
	TEXTURE2D(name);     \
	SAMPLER(sampler##name);

	
	struct Attributes
	{
		float4 positionOS : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct Varyings
	{
		float4 positionHCS : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	Varyings vert(Attributes input)
	{
		Varyings o;
		o.positionHCS = TO_CLIP(input.positionOS.xyz);
		o.uv = input.uv;
		return o;
	}

	TEXTURE2D(_MainTex);
	SAMPLER(sampler_MainTex);
	

#elif defined(UNITY_PIPELINE_HDRP)
	//#error "UNITY_PIPELINE_HDRP"

	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassForward.hlsl"
	#define SAMPLE(tex, uv) SAMPLE_TEXTURE2D(tex, sampler_##tex, uv)
	#define TO_CLIP(pos) TransformObjectToHClip(pos)

#else
	//#error "UNITY_PIPELINE_BUILTIN"

	#include "UnityCG.cginc"
	#define SAMPLE(tex, uv) tex2D(tex, uv)
	#define TO_CLIP(pos) UnityObjectToClipPos(pos)
	
	#define DECLARE_TEX(name) \
	sampler2D name;

	#define PI 3.14159265359


	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	v2f vert(appdata v)
	{
		v2f o;
		o.pos = TO_CLIP(v.vertex);
		o.uv = v.uv;

		return o;
	}

	#define INPUT v2f

	sampler2D _MainTex;

#endif

/*
	Constants.
*/

#define PHI	1.61803398875


#define RED float4(1, 0, 0, 1)
#define GREEN float4(0, 1, 0, 1)
#define BLUE float4(0, 0, 1, 1)
#define WHITE float4(1, 1, 1, 1)
#define BLACK float4(0, 0, 0, 1)
#define YELLOW float4(1, 1, 0, 1)
#define CYAN float4(0, 1, 1, 1)
#define MAGENTA float4(1, 0, 1, 1)

/*
	Macros.
*/

#define RGB1(color) float4((color).rgb, 1)
#define RGB0(color) float4((color).rgb, 0)

/*
	Functions.
*/
float3 adjust_gamma(float3 color, float gamma)
{
	return pow(color.rgb, 1.0 / gamma);
}

float4 adjust_gamma(float4 color, float gamma)
{
	return float4(adjust_gamma(color.rgb, gamma), 1);
}

float to_luminosity(float3 color)
{
	const float3 luma = float3(0.299, 0.587, 0.114);
	return dot(color, luma);
}

float to_luminosity(float4 color)
{
	return to_luminosity(color.rgb);
}


float3 desaturate(float3 color)
{
	float luminosity = to_luminosity(color);	
	return float3(luminosity, luminosity, luminosity);
}

float4 desaturate(float4 color)
{
	return float4(desaturate(color.rgb), 1);
}

float3 quantize(float3 color, int3 level_count)
{
	return floor(color * level_count) / level_count;
}

float3 quantize_smooth(float3 color, int3 level_count, float smoothness)
{
	smoothness = saturate(smoothness);
	
	int3 threshold_count = level_count - 1;

	if (smoothness <= 0.0001)
	{
		return round(color * threshold_count) / threshold_count;
	}

	float3 level_frac_part = frac(color * threshold_count);
	float left_edge = 0.5 - smoothness * 0.5;
	float right_edge = 0.5 + smoothness * 0.5;
	float3 edge_transition = smoothstep(left_edge, right_edge, level_frac_part);

	float3 quantized = floor(color * threshold_count) / threshold_count;
	float3 next_quantized = (floor(color * threshold_count) + 1.0) / threshold_count;
	float3 smoothed = lerp(quantized, next_quantized, edge_transition);

	return smoothed;
}

float2 rotate(float2 uv, float angle)
{
	float cos_a = cos(angle);
	float sin_a = sin(angle);
	return float2(uv.x * cos_a - uv.y * sin_a, uv.x * sin_a + uv.y * cos_a);
}

float4 quantize_smooth(float4 color, int3 level_count, float smoothness)
{
	return float4(quantize_smooth(color.rgb, level_count, smoothness), 1);
}

float2 pixelate_uv(float2 uv, float2 factor, float2 texel_size)
{
	float2 pixel_size = texel_size * factor;
	return floor(uv / pixel_size) * pixel_size;
}

float2 apply_tiling(float2 uv, float4 tiling)
{
	return uv * tiling.xy + tiling.zw;
}

/*
	Internal Functions.
*/

/**
	Maps a color to a gradient defined by three colors (low, mid, high) based on the luminosity of the input color.

	@param color The input color to be mapped.
	@param low_color The color representing the low end of the gradient.
	@param mid_color The color representing the middle of the gradient.
	@param high_color The color representing the high end of the gradient.
	@param low_value The luminosity value corresponding to the low_color.
	@param mid_value The luminosity value corresponding to the mid_color.
	@param high_value The luminosity value corresponding to the high_color.
	@return The resulting color after mapping to the gradient.
*/
float4 tri_tone_map(float4 color, float4 low_color, float4 mid_color, float4 high_color, float low_value, float mid_value, float high_value)
{
	float4 result;
	float luminosity = to_luminosity(color.rgb);

	if (luminosity < mid_value)
	{
		// TODO @herman this line below represents an inverse_lerp(a, b, t). Should we add such function?
		// e.g: float t = inverse_lerp(_LowValue, _MidValue, luminosity);
		float t = (luminosity - low_value) / (mid_value - low_value);
		t = saturate(t);
		result = lerp(low_color, mid_color, t);
		result = lerp(result, color, 1 - result.a);
	}
	else
	{
		float t = (luminosity - mid_value) / (high_value - mid_value);
		t = saturate(t);
		result = lerp(mid_color, high_color, t);
		result = lerp(result, color, 1 - result.a);
	}

	result.a = 1.0;
	return result;
}

float3 rgb_to_hsl(float3 color)
{
	float r = color.r;
	float g = color.g;
	float b = color.b;

	float hue, saturation, luminance;

	float max_rgb = max(r, max(g, b));

	float tolerance = 0.01;
				
	if (max_rgb <= tolerance)
	{
		hue = 0;
		saturation = 0;
		luminance = 0;
							
		return float3(hue, saturation, luminance);
	}

	float min_rgb = min(r, min(g, b));
	float dif = max_rgb - min_rgb;

	if (dif > tolerance)
	{
		if (g >= r && g >= b)
		{
			hue = (b - r) / dif * 60.0 + 120.0;
		}
		else if (b >= g && b >= r)
		{
			hue = (r - g) / dif * 60.0 + 240.0;
		}
		else if (b > g)
		{
			hue = (g - b) / dif * 60.0 + 360.0;
		}
		else
		{
			hue = (g - b) / dif * 60.0;
		}
		if (hue < 0)
		{
			hue = hue + 360.0;
		}
	}
	else
	{
		hue = 0;
	}

	hue *= 1.0 / 360.0;
	saturation = (dif / max_rgb) * 1;
	luminance = max_rgb;

	hue = clamp(hue, 0, 1);
	saturation = clamp(saturation, 0, 1);
	luminance = clamp(luminance, 0, 1);

	return float3(hue, saturation, luminance);
}

float3 hsl_to_rgb(float3 hsl)
{
	float hue = hsl.x;
	float saturation = hsl.y;
	float luminance = hsl.z;
	
	float r = luminance;
	float g = luminance;
	float b = luminance;

	if (!(saturation > 0))
	{
		return saturate(float3(r, g, b));
	}

	float max = luminance;
	float dif = luminance * saturation;
	float min = luminance - dif;

	float hh = hue * 360;

	if (hh < 60)
	{
		r = max;
		g = hh * dif / 60 + min;
		b = min;
	}
	else if (hh < 120)
	{
		r = -(hh - 120) * dif / 60 + min;
		g = max;
		b = min;
	}
	else if (hh < 180)
	{
		r = min;
		g = max;
		b = (hh - 120) * dif / 60 + min;
	}
	else if (hh < 240)
	{
		r = min;
		g = -(hh - 240) * dif / 60 + min;
		b = max;
	}
	else if (hh < 300)
	{
		r = (hh - 240) * dif / 60 + min;
		g = min;
		b = max;
	}
	else if (hh <= 360)
	{
		r = max;
		g = min;
		b = -(hh - 360) * dif / 60 + min;
	}
	else
	{
		r = 0;
		g = 0;
		b = 0;
	}

	return saturate(float3(r, g, b));
}

float linear_error(float3 a, float3 b)
{
	float3 diff = a - b;
	return dot(diff, diff);
}

float2 make_quad_coherent(float2 uv, float tileSize)
{
	float2 screen_size = _ScreenParams.xy;
	float2 pixel = uv * screen_size;
	pixel = floor(pixel / tileSize) * tileSize;
	return pixel / screen_size;
}


#endif
