using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BoatWake : MonoBehaviour
{
    [SerializeField] private Transform boatTransform;
    [SerializeField] private float heightOffset = -0.55f;
    [SerializeField] private float trailBackDistance = 3.0f;

    [Header("Emission")]
    [SerializeField] private float emissionRate = 8f;
    [SerializeField] private float particleLifetime = 8f;
    [SerializeField] private Vector2 startSizeRange = new Vector2(0.4f, 0.7f);
    [SerializeField] private float particleSpeed = 0.4f;

    [Header("Shape")]
    [SerializeField] private float coneAngle = 0f;
    [SerializeField] private float coneRadius = 0.05f;

    [Header("Noise")]
    [SerializeField] private bool enableNoise = false;
    [SerializeField] private float noiseStrength = 0.05f;
    [SerializeField] private float noiseFrequency = 0.3f;

    [Header("Foam Textures asset material")]
    [SerializeField] private Material foamMaterial;

    private void Awake()
    {
        var ps = GetComponent<ParticleSystem>();

        var main = ps.main;
        main.loop = true;
        main.startLifetime = particleLifetime;
        main.startSpeed = particleSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(startSizeRange.x, startSizeRange.y);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        // The custom quad mesh below is already authored flat in the local XZ
        // plane (normal +Y) and elongated along local Z to look like a streak —
        // keep yaw fixed (no random rotation) so every quad stays aligned with the
        // trail direction instead of crossing over each other.
        main.startRotation3D = true;
        main.startRotationX = 0f;
        main.startRotationY = 0f;
        main.startRotationZ = 0f;

        var emission = ps.emission;
        emission.rateOverTime = emissionRate;

        // Narrow cone behind the boat, pointed opposite travel direction via this transform's rotation.
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = coneAngle;
        shape.radius = coneRadius;
        shape.arc = 30f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.4f),
            new Keyframe(0.1f, 1f),
            new Keyframe(1f, 0.7f)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Note: the asset's material uses the Standard shader, which doesn't read
        // per-particle vertex color, so fade-out comes from sizeOverLifetime above.

        var noise = ps.noise;
        noise.enabled = enableNoise;
        noise.strength = noiseStrength;
        noise.frequency = noiseFrequency;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.mesh = BuildQuadMesh();
        renderer.alignment = ParticleSystemRenderSpace.Local;
        renderer.sortMode = ParticleSystemSortMode.OldestInFront;
        renderer.sharedMaterial = foamMaterial;
    }

    private void LateUpdate()
    {
        if (boatTransform == null) return;

        // Flatten forward to the horizontal plane for positioning so the boat's
        // bob/tilt pitch (BoatBob.cs) never drags the wake's height up/down or
        // below the water surface — only yaw (actual travel heading) should matter.
        Vector3 flatForward = boatTransform.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.0001f) flatForward = Vector3.forward;
        flatForward.Normalize();

        transform.position = boatTransform.position + flatForward * trailBackDistance + Vector3.up * heightOffset;
        transform.rotation = Quaternion.LookRotation(flatForward, Vector3.up);
    }

    private static Mesh BuildQuadMesh()
    {
        var mesh = new Mesh();
        float halfWidth = 0.5f;
        float length = 2f;

        mesh.vertices = new[]
        {
            new Vector3(-halfWidth, 0f, 0f),
            new Vector3(halfWidth, 0f, 0f),
            new Vector3(halfWidth, 0f, length),
            new Vector3(-halfWidth, 0f, length),
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
        };
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        mesh.RecalculateBounds();

        return mesh;
    }
}
