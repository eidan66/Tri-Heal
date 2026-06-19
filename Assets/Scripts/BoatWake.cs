using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BoatWake : MonoBehaviour
{
    [SerializeField] private float emissionRate = 12f;
    [SerializeField] private float particleSpeed = 1.8f;
    [SerializeField] private float particleLifetime = 2.2f;
    [SerializeField] private Vector3 driftDirection = Vector3.left;

    [Header("Foam Textures asset material")]
    [SerializeField] private Material foamMaterial;

    private static Mesh quadMesh;

    private void Awake()
    {
        var ps = GetComponent<ParticleSystem>();

        var main = ps.main;
        main.loop = true;
        main.startLifetime = particleLifetime;
        main.startSpeed = particleSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(1.0f, 2.0f);
        main.startColor = new Color(1f, 1f, 1f, 0.85f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        // Lay the quads flat on the water surface instead of facing the camera.
        main.startRotation3D = true;
        main.startRotationX = 90f * Mathf.Deg2Rad;
        main.startRotationY = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startRotationZ = 0f;

        var emission = ps.emission;
        emission.rateOverTime = emissionRate;

        // Narrow cone right behind the boat that fans out gradually into a Kelvin-wake "V".
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 14f;
        shape.radius = 0.1f;
        shape.rotation = Quaternion.LookRotation(driftDirection.normalized).eulerAngles;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.3f),
            new Keyframe(0.15f, 1f),
            new Keyframe(1f, 0.55f)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Note: the asset's material uses the Standard shader, which doesn't read
        // per-particle vertex color, so fade-out comes from sizeOverLifetime above.

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.04f;
        noise.frequency = 0.3f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.mesh = GetQuadMesh();
        renderer.alignment = ParticleSystemRenderSpace.Local;
        renderer.sortMode = ParticleSystemSortMode.OldestInFront;

        renderer.sharedMaterial = foamMaterial;
    }

    private static Mesh GetQuadMesh()
    {
        if (quadMesh != null) return quadMesh;

        var temp = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadMesh = Object.Instantiate(temp.GetComponent<MeshFilter>().sharedMesh);
        Object.Destroy(temp);
        return quadMesh;
    }
}
