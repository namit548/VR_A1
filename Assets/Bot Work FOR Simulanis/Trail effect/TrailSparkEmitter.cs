using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class TrailSparkEmitter : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform trailTarget;

    [Header("Emission")]
    [SerializeField] private float minDistancePerSpark = 0.05f;
    [SerializeField] private int sparksPerStep = 1;
    [SerializeField] private float minSpeedToEmit = 0.2f;
    [SerializeField] private int extraBurstSparks = 2;
    [SerializeField] private float burstChance = 0.35f;

    [Header("Lifetime")]
    [SerializeField] private Vector2 lifetimeRange = new Vector2(0.08f, 0.22f);
    [SerializeField] private Vector2 sizeRange = new Vector2(0.03f, 0.08f);

    [Header("Velocity")]
    [SerializeField] private float backwardVelocity = 0.5f;
    [SerializeField] private float sidewaysVelocity = 0.2f;
    [SerializeField] private float upwardVelocity = 0.08f;
    [SerializeField] private float radialBurstVelocity = 0.35f;

    [Header("Color")]
    [SerializeField] private Color startColor = new Color(1f, 0.95f, 1f, 1f);
    [SerializeField] private Color endColor = new Color(0.6f, 0.9f, 1.4f, 0f);
    [SerializeField] private Color flashColor = new Color(1f, 0.98f, 0.85f, 1f);

    private ParticleSystem particleSystemCache;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule;
    private ParticleSystem.MinMaxGradient colorGradient;
    private Vector3 lastPosition;
    private float distanceAccumulator;
    private bool initialized;

    private void Reset()
    {
        trailTarget = transform;
        ConfigureParticleSystem();
    }

    private void Awake()
    {
        particleSystemCache = GetComponent<ParticleSystem>();
        mainModule = particleSystemCache.main;
        emissionModule = particleSystemCache.emission;
        colorOverLifetimeModule = particleSystemCache.colorOverLifetime;

        if (trailTarget == null)
        {
            trailTarget = transform;
        }

        ConfigureParticleSystem();
        lastPosition = trailTarget.position;
        initialized = true;
    }

    private void LateUpdate()
    {
        if (!initialized || trailTarget == null)
        {
            return;
        }

        Vector3 currentPosition = trailTarget.position;
        Vector3 delta = currentPosition - lastPosition;
        float distance = delta.magnitude;
        float speed = distance / Mathf.Max(Time.deltaTime, 0.0001f);

        if (speed >= minSpeedToEmit)
        {
            distanceAccumulator += distance;
            Vector3 direction = delta.sqrMagnitude > 0.000001f ? delta.normalized : trailTarget.forward;

            while (distanceAccumulator >= minDistancePerSpark)
            {
                EmitSparkCluster(currentPosition, direction, speed);
                distanceAccumulator -= minDistancePerSpark;
            }
        }
        else
        {
            distanceAccumulator = 0f;
        }

        lastPosition = currentPosition;
    }

    private void ConfigureParticleSystem()
    {
        if (particleSystemCache == null)
        {
            particleSystemCache = GetComponent<ParticleSystem>();
        }

        mainModule = particleSystemCache.main;
        emissionModule = particleSystemCache.emission;
        colorOverLifetimeModule = particleSystemCache.colorOverLifetime;

        mainModule.loop = false;
        mainModule.playOnAwake = true;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
        mainModule.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeRange.x, lifetimeRange.y);
        mainModule.startSize = new ParticleSystem.MinMaxCurve(sizeRange.x, sizeRange.y);
        mainModule.startSpeed = 0f;
        mainModule.maxParticles = 512;

        emissionModule.enabled = false;

        colorOverLifetimeModule.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(startColor, 0f),
                new GradientColorKey(Color.Lerp(startColor, endColor, 0.35f), 0.35f),
                new GradientColorKey(endColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(startColor.a, 0f),
                new GradientAlphaKey(0.8f, 0.2f),
                new GradientAlphaKey(0f, 1f)
            });
        colorGradient = new ParticleSystem.MinMaxGradient(gradient);
        colorOverLifetimeModule.color = colorGradient;
    }

    private void EmitSparkCluster(Vector3 currentPosition, Vector3 direction, float speed)
    {
        Vector3 side = Vector3.Cross(direction, Vector3.forward);
        if (side.sqrMagnitude < 0.0001f)
        {
            side = Vector3.Cross(direction, Vector3.up);
        }
        side.Normalize();

        Vector3 upAxis = Vector3.Cross(side, direction).normalized;
        if (upAxis.sqrMagnitude < 0.0001f)
        {
            upAxis = Vector3.up;
        }

        for (int i = 0; i < sparksPerStep; i++)
        {
            EmitSingleSpark(currentPosition, direction, side, upAxis, speed, false);
        }

        if (Random.value <= burstChance)
        {
            for (int i = 0; i < extraBurstSparks; i++)
            {
                EmitSingleSpark(currentPosition, direction, side, upAxis, speed, true);
            }
        }
    }

    private void EmitSingleSpark(
        Vector3 currentPosition,
        Vector3 direction,
        Vector3 side,
        Vector3 upAxis,
        float speed,
        bool isBurstSpark)
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();

        Vector3 trailingVelocity = (-direction * backwardVelocity)
            + (side * Random.Range(-sidewaysVelocity, sidewaysVelocity))
            + (upAxis * Random.Range(0f, upwardVelocity));

        Vector3 radialVelocity = (side * Random.Range(-1f, 1f) + upAxis * Random.Range(-1f, 1f)).normalized;
        if (radialVelocity.sqrMagnitude < 0.0001f)
        {
            radialVelocity = side;
        }

        Vector3 velocity = trailingVelocity;
        if (isBurstSpark)
        {
            velocity += radialVelocity * radialBurstVelocity;
        }

        float sizeMultiplier = isBurstSpark ? 1.35f : 1f;
        float lifetimeMultiplier = isBurstSpark ? 0.8f : 1f;

        emitParams.position = currentPosition + Random.insideUnitSphere * (isBurstSpark ? 0.025f : 0.015f);
        emitParams.velocity = velocity * Mathf.Clamp(speed * 0.2f, 0.5f, 2f);
        emitParams.startLifetime = Random.Range(lifetimeRange.x, lifetimeRange.y) * lifetimeMultiplier;
        emitParams.startSize = Random.Range(sizeRange.x, sizeRange.y) * sizeMultiplier;
        emitParams.startColor = isBurstSpark ? flashColor : startColor;

        particleSystemCache.Emit(emitParams, 1);
    }
}
