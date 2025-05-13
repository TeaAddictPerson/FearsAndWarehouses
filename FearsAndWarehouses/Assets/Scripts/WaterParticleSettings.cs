using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class WaterParticleSettings : MonoBehaviour
{
    private void Start()
    {
        var particleSystem = GetComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.startLifetime = 1f;
        main.startSpeed = 5f;
        main.startSize = 0.1f;
        main.startColor = new Color(0.7f, 0.9f, 1f, 0.8f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 1f;

        var emission = particleSystem.emission;
        emission.rateOverTime = 50f;

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.1f;

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