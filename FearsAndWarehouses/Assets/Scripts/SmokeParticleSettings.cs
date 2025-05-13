using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SmokeParticleSettings : MonoBehaviour
{
    private void Start()
    {
        var particleSystem = GetComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.startLifetime = 3f;
        main.startSpeed = 0.5f;
        main.startSize = 0.2f;
        main.startColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.1f;

        var emission = particleSystem.emission;
        emission.rateOverTime = 10f;

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        // Создаем материал для частиц
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            // Используем встроенный шейдер для частиц
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            if (renderer.material == null)
            {
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            }
            renderer.sortingOrder = 1;
        }
    }
} 