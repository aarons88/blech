using System.IO;
using UnityEditor;
using UnityEngine;

namespace Blech.Editor
{
    public static class ShaderBuilder
    {
        const string ShaderDir = "Assets/Blech/Art/Shaders";
        const string MatDir = "Assets/Blech/Art/Materials";

        public static void BuildAll()
        {
            AssetFactory.EnsureFolder(ShaderDir);
            AssetFactory.EnsureFolder(MatDir);

            // 1. Intestine — soft pulse
            WriteShader("Intestine_Organic", "0.902,0.494,0.549,1", 0.4f, "0.4,0.13,0.16,1", "");
            // 2. Stomach — glossy, no pulse
            WriteShader("Stomach_Wall", "0.561,0.659,0.337,1", 0.6f, "0,0,0,1", "");
            // 3. Throat — subtle vertex sway
            WriteShader("Throat_Tissue", "0.420,0.180,0.271,1", 0.5f, "0,0,0,1",
                "IN.positionOS.xyz += IN.normalOS * sin(_Time.y * 1.2 + IN.positionOS.y * 0.5) * 0.015;");
            // 4. Mucus — translucent slip (kept opaque for Lit; alpha handled via emission shimmer)
            WriteShader("Mucus_Slip", "0.773,0.863,0.627,1", 0.95f, "0.4,0.5,0.3,1", "");
            // 5. Acid — neon
            WriteShader("Acid_Surface", "0.714,1,0.231,1", 0.7f, "0.6,1,0.2,1", "");
            // 6. Tongue — wet pink
            WriteShader("Tongue", "1,0.604,0.671,1", 0.85f, "0,0,0,1", "");

            AssetDatabase.Refresh();

            BuildMaterial("Intestine_Organic", new Color(0.902f, 0.494f, 0.549f), 0.4f, new Color(0.6f, 0.2f, 0.25f), 0.6f, 1.2f);
            BuildMaterial("Stomach_Wall",     new Color(0.561f, 0.659f, 0.337f), 0.6f, Color.black, 0f, 0f);
            BuildMaterial("Throat_Tissue",    new Color(0.420f, 0.180f, 0.271f), 0.5f, Color.black, 0f, 0f);
            BuildMaterial("Mucus_Slip",       new Color(0.773f, 0.863f, 0.627f), 0.95f, new Color(0.4f, 0.5f, 0.3f), 0.2f, 0f, scrollX: 0.05f, scrollY: 0.1f);
            BuildMaterial("Acid_Surface",     new Color(0.714f, 1f,     0.231f), 0.7f, new Color(0.6f, 1f, 0.2f), 1.2f, 0f);
            BuildMaterial("Tongue",           new Color(1f,     0.604f, 0.671f), 0.85f, Color.black, 0f, 0f);

            AssetDatabase.SaveAssets();
        }

        static void WriteShader(string name, string colorRgba, float smoothness, string emissionRgba, string vertFragment)
        {
            string path = $"{ShaderDir}/{name}.shader";
            File.WriteAllText(path, ShaderTemplates.LitShader(name, colorRgba, smoothness, emissionRgba, vertFragment));
        }

        static void BuildMaterial(string name, Color baseColor, float smoothness, Color emission,
            float pulseStrength, float pulseSpeed, float scrollX = 0, float scrollY = 0)
        {
            var shader = Shader.Find($"Blech/{name}");
            if (shader == null)
            {
                Debug.LogError($"[Blech] Shader Blech/{name} not found — using URP/Lit fallback");
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }
            var mat = AssetFactory.CreateOrUpdateMaterial($"{MatDir}/M_{name}.mat", shader);
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission);
            if (mat.HasProperty("_PulseStrength")) mat.SetFloat("_PulseStrength", pulseStrength);
            if (mat.HasProperty("_PulseSpeed")) mat.SetFloat("_PulseSpeed", pulseSpeed);
            if (mat.HasProperty("_ScrollSpeed")) mat.SetVector("_ScrollSpeed", new Vector4(scrollX, scrollY, 0, 0));
            EditorUtility.SetDirty(mat);
        }
    }
}
