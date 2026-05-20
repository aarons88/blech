using UnityEditor;
using UnityEngine;

namespace Blech.Editor
{
    public static class ParticleBuilder
    {
        const string FxDir = "Assets/Blech/Prefabs/FX";

        public static void BuildAll()
        {
            AssetFactory.EnsureFolder(FxDir);
            BuildBubbles();
            BuildAcidSplash();
            BuildWindStreaks();
            BuildConfetti();
            AssetDatabase.SaveAssets();
        }

        static void BuildBubbles()
        {
            var go = new GameObject("FX_Bubbles");
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 5f; main.loop = true; main.startLifetime = 3f;
            main.startSpeed = 1f; main.startSize = 0.15f;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.8f, 1f, 0.9f, 0.6f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission; emission.rateOverTime = 20f;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Hemisphere; shape.radius = 1.5f;

            var vel = ps.velocityOverLifetime; vel.enabled = true;
            vel.y = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);

            var color = ps.colorOverLifetime; color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(MakeFadeGradient(new Color(0.8f, 1f, 0.9f, 0.6f)));

            var size = ps.sizeOverLifetime; size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0, 0.5f, 1, 1.2f));

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            AssignDefaultMaterial(renderer);

            AssetFactory.SavePrefab(go, $"{FxDir}/FX_Bubbles.prefab");
        }

        static void BuildAcidSplash()
        {
            var go = new GameObject("FX_AcidSplash");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1f; main.loop = false; main.startLifetime = 0.5f;
            main.startSpeed = 4f; main.startSize = 0.1f;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.7f, 1f, 0.2f, 1f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var emission = ps.emission; emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f; shape.radius = 0.1f;

            var vel = ps.velocityOverLifetime; vel.enabled = true;
            vel.y = new ParticleSystem.MinMaxCurve(2f, 5f);

            var color = ps.colorOverLifetime; color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(MakeFadeGradient(new Color(0.7f, 1f, 0.2f, 1f)));

            AssignDefaultMaterial(ps.GetComponent<ParticleSystemRenderer>());

            AssetFactory.SavePrefab(go, $"{FxDir}/FX_AcidSplash.prefab");
        }

        static void BuildWindStreaks()
        {
            var go = new GameObject("FX_WindStreaks");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 2f; main.loop = true; main.startLifetime = 0.5f;
            main.startSpeed = 5f; main.startSize = 0.05f;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0.4f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var emission = ps.emission; emission.rateOverTime = 60f;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(3, 3, 0.1f);

            var vel = ps.velocityOverLifetime; vel.enabled = true;
            vel.z = new ParticleSystem.MinMaxCurve(5f);

            var color = ps.colorOverLifetime; color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(MakeFadeGradient(new Color(1f, 1f, 1f, 0.4f)));

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 4f;
            AssignDefaultMaterial(renderer);

            AssetFactory.SavePrefab(go, $"{FxDir}/FX_WindStreaks.prefab");
        }

        static void BuildConfetti()
        {
            var go = new GameObject("FX_Confetti");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 2f; main.loop = false; main.startLifetime = 2f;
            main.startSpeed = 6f; main.startSize = 0.1f;
            main.startColor = new ParticleSystem.MinMaxGradient(Color.HSVToRGB(0, 0.8f, 1f), Color.HSVToRGB(1, 0.8f, 1f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 1f;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 100) });

            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 60f;

            AssignDefaultMaterial(ps.GetComponent<ParticleSystemRenderer>());

            AssetFactory.SavePrefab(go, $"{FxDir}/FX_Confetti.prefab");
        }

        static void AssignDefaultMaterial(ParticleSystemRenderer renderer)
        {
            if (renderer == null) return;
            var mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
            if (mat == null) mat = new Material(Shader.Find("Sprites/Default"));
            renderer.sharedMaterial = mat;
        }

        static Gradient MakeFadeGradient(Color c)
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(c.a, 0.2f), new GradientAlphaKey(0f, 1f) }
            );
            return g;
        }
    }
}
