Shader "Custom/FogShaderTransparent"
{
    Properties
    {
        _BaseMap("Base Texture", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _FogColor("Fog Color", Color) = (0.5,0.5,0.5,1)
        _FogStart("Fog Start Distance", Float) = 10
        _FogEnd("Fog End Distance", Float) = 50
    }

    SubShader
    {
		
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        LOD 100

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
		
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionH : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _BaseColor;
            float4 _FogColor;
            float _FogStart;
            float _FogEnd;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionH = TransformObjectToHClip(IN.positionOS);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 camPos = _WorldSpaceCameraPos;
                float dist = distance(IN.worldPos, camPos);

                //float fogFactor = saturate((_FogEnd - dist) / (_FogEnd - _FogStart));
				float fogDensity = 0.05; // adjust for how thick the fog is
				float fogFactor = exp(-dist * fogDensity);


                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                float3 finalColor = lerp(_FogColor.rgb, texColor.rgb, fogFactor);

                return float4(finalColor, texColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
