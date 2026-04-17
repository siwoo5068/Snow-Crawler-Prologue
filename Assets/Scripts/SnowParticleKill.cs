using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SnowParticleKill : MonoBehaviour
{
    [Header("Kill Zone")]
    public float killBelowY = -0.3f;

    [Header("Auto-detect Ground (optional)")]
    public bool autoDetectGround = true;
    public string groundObjectName = "Ground";
    public float groundOffset = 0.1f;

    private ParticleSystem _ps;
    private ParticleSystem.Particle[] _particles;
    private int _lastMaxParticles;

    void Awake()
    {
        _ps = GetComponent<ParticleSystem>();

        if (autoDetectGround)
        {
            var g = GameObject.Find(groundObjectName);
            if (g != null)
                killBelowY = g.transform.position.y + groundOffset;
        }

        _lastMaxParticles = _ps.main.maxParticles;
        _particles = new ParticleSystem.Particle[_lastMaxParticles];
    }

    void EnsureBufferCapacity()
    {
        int max = _ps.main.maxParticles;
        if (max != _lastMaxParticles)
        {
            _lastMaxParticles = max;
            _particles = new ParticleSystem.Particle[max];
        }
    }

    void LateUpdate()
    {
        EnsureBufferCapacity();

        int count = _ps.GetParticles(_particles);
        bool changed = false;

        for (int i = 0; i < count; i++)
        {
            if (_particles[i].position.y <= killBelowY)
            {
                _particles[i].remainingLifetime = 0f;
                changed = true;
            }
        }

        if (changed)
            _ps.SetParticles(_particles, count);
    }
}
