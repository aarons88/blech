namespace Blech.Editor
{
    public static class ShaderTemplates
    {
        // URP Lit shader template with Color, Emission, Smoothness, optional UV scroll + time pulse.
        // The customVertFragment string is inserted before the standard vertex computation; use it
        // to add per-shader vertex perturbations (e.g. throat tissue sway).
        public static string LitShader(string blechName, string defaultColorRgba, float defaultSmoothness,
            string emissionDefaultRgba, string customVertFragment)
        {
            return
"Shader \"Blech/" + blechName + "\"\n" +
"{\n" +
"    Properties\n" +
"    {\n" +
"        _BaseColor (\"Base Color\", Color) = (" + defaultColorRgba + ")\n" +
"        _BaseMap (\"Base Map\", 2D) = \"white\" {}\n" +
"        _Smoothness (\"Smoothness\", Range(0,1)) = " + defaultSmoothness.ToString("F2") + "\n" +
"        _Metallic (\"Metallic\", Range(0,1)) = 0\n" +
"        _EmissionColor (\"Emission\", Color) = (" + emissionDefaultRgba + ")\n" +
"        _ScrollSpeed (\"Scroll Speed\", Vector) = (0,0,0,0)\n" +
"        _PulseStrength (\"Pulse Strength\", Range(0,2)) = 0\n" +
"        _PulseSpeed (\"Pulse Speed\", Range(0,8)) = 0\n" +
"    }\n" +
"    SubShader\n" +
"    {\n" +
"        Tags { \"RenderType\"=\"Opaque\" \"RenderPipeline\"=\"UniversalPipeline\" \"Queue\"=\"Geometry\" }\n" +
"        LOD 200\n" +
"\n" +
"        Pass\n" +
"        {\n" +
"            Name \"ForwardLit\"\n" +
"            Tags { \"LightMode\"=\"UniversalForward\" }\n" +
"\n" +
"            HLSLPROGRAM\n" +
"            #pragma vertex vert\n" +
"            #pragma fragment frag\n" +
"            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS\n" +
"            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE\n" +
"            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS\n" +
"            #pragma multi_compile _ _SHADOWS_SOFT\n" +
"            #pragma multi_compile_fog\n" +
"            #pragma multi_compile_instancing\n" +
"\n" +
"            #include \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl\"\n" +
"            #include \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl\"\n" +
"\n" +
"            struct Attributes\n" +
"            {\n" +
"                float4 positionOS : POSITION;\n" +
"                float3 normalOS   : NORMAL;\n" +
"                float2 uv         : TEXCOORD0;\n" +
"                UNITY_VERTEX_INPUT_INSTANCE_ID\n" +
"            };\n" +
"\n" +
"            struct Varyings\n" +
"            {\n" +
"                float4 positionHCS : SV_POSITION;\n" +
"                float3 positionWS  : TEXCOORD1;\n" +
"                float3 normalWS    : TEXCOORD2;\n" +
"                float2 uv          : TEXCOORD0;\n" +
"                float fogFactor    : TEXCOORD3;\n" +
"                UNITY_VERTEX_INPUT_INSTANCE_ID\n" +
"            };\n" +
"\n" +
"            CBUFFER_START(UnityPerMaterial)\n" +
"                float4 _BaseColor;\n" +
"                float4 _BaseMap_ST;\n" +
"                float  _Smoothness;\n" +
"                float  _Metallic;\n" +
"                float4 _EmissionColor;\n" +
"                float4 _ScrollSpeed;\n" +
"                float  _PulseStrength;\n" +
"                float  _PulseSpeed;\n" +
"            CBUFFER_END\n" +
"\n" +
"            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);\n" +
"\n" +
"            Varyings vert(Attributes IN)\n" +
"            {\n" +
"                Varyings OUT;\n" +
"                UNITY_SETUP_INSTANCE_ID(IN);\n" +
"                " + customVertFragment + "\n" +
"                VertexPositionInputs vp = GetVertexPositionInputs(IN.positionOS.xyz);\n" +
"                OUT.positionHCS = vp.positionCS;\n" +
"                OUT.positionWS  = vp.positionWS;\n" +
"                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);\n" +
"                OUT.uv          = IN.uv * _BaseMap_ST.xy + _BaseMap_ST.zw + _ScrollSpeed.xy * _Time.y;\n" +
"                OUT.fogFactor   = ComputeFogFactor(vp.positionCS.z);\n" +
"                return OUT;\n" +
"            }\n" +
"\n" +
"            half4 frag(Varyings IN) : SV_Target\n" +
"            {\n" +
"                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);\n" +
"                half3 albedo = tex.rgb * _BaseColor.rgb;\n" +
"                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;\n" +
"                half3 emission = _EmissionColor.rgb * (1.0 + pulse * _PulseStrength);\n" +
"\n" +
"                InputData input = (InputData)0;\n" +
"                input.positionWS = IN.positionWS;\n" +
"                input.normalWS = normalize(IN.normalWS);\n" +
"                input.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);\n" +
"                input.fogCoord = IN.fogFactor;\n" +
"\n" +
"                SurfaceData s = (SurfaceData)0;\n" +
"                s.albedo = albedo;\n" +
"                s.metallic = _Metallic;\n" +
"                s.smoothness = _Smoothness;\n" +
"                s.alpha = 1;\n" +
"                s.emission = emission;\n" +
"                s.occlusion = 1;\n" +
"\n" +
"                half4 color = UniversalFragmentPBR(input, s);\n" +
"                color.rgb = MixFog(color.rgb, IN.fogFactor);\n" +
"                return color;\n" +
"            }\n" +
"            ENDHLSL\n" +
"        }\n" +
"    }\n" +
"    FallBack \"Universal Render Pipeline/Lit\"\n" +
"}\n";
        }
    }
}
