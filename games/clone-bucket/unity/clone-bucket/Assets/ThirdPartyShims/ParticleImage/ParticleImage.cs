using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AssetKits
{
    public class ParticleImage : MaskableGraphic
    {
        [SerializeField] private ParticleImage _main;
        [SerializeField] private ParticleImage[] _children;
        [SerializeField] private Enumerations.Simulation _space;
        [SerializeField] private Enumerations.TimeScale _timeScale;
        [SerializeField] private Module _emitterConstraintEnabled;
        [SerializeField] private Transform _emitterConstraintTransform;
        [SerializeField] private Enumerations.EmitterShape _shape;
        [SerializeField] private Enumerations.SpreadType _spread;
        [SerializeField] private float _spreadLoop;
        [SerializeField] private float _startDelay;
        [SerializeField] private float _radius;
        [SerializeField] private float _width;
        [SerializeField] private float _height;
        [SerializeField] private float _angle;
        [SerializeField] private float _length;
        [SerializeField] private bool _fitRect;
        [SerializeField] private bool _emitOnSurface;
        [SerializeField] private float _emitterThickness;
        [SerializeField] private bool _loop;
        [SerializeField] private float _duration;
        [SerializeField] private Enumerations.PlayMode _playMode;
        [SerializeField] private SeparatedMinMaxCurve _startSize;
        [SerializeField] private ParticleSystem.MinMaxGradient _startColor;
        [SerializeField] private ParticleSystem.MinMaxCurve _lifetime;
        [SerializeField] private ParticleSystem.MinMaxCurve _startSpeed;
        [SerializeField] private ParticleSystem.MinMaxGradient _colorOverLifetime;
        [SerializeField] private ParticleSystem.MinMaxGradient _colorBySpeed;
        [SerializeField] private SpeedRange _colorSpeedRange;
        [SerializeField] private SeparatedMinMaxCurve _sizeOverLifetime;
        [SerializeField] private SeparatedMinMaxCurve _sizeBySpeed;
        [SerializeField] private SpeedRange _sizeSpeedRange;
        [SerializeField] private SeparatedMinMaxCurve _startRotation;
        [SerializeField] private SeparatedMinMaxCurve _rotationOverLifetime;
        [SerializeField] private SeparatedMinMaxCurve _rotationBySpeed;
        [SerializeField] private SpeedRange _rotationSpeedRange;
        [SerializeField] private ParticleSystem.MinMaxCurve _speedOverLifetime;
        [SerializeField] private bool _alignToDirection;
        [SerializeField] private ParticleSystem.MinMaxCurve _gravity;
        [SerializeField] private Module _targetModule;
        [SerializeField] private Transform _attractorTarget;
        [SerializeField] private ParticleSystem.MinMaxCurve _toTarget;
        [SerializeField] private Enumerations.AttractorType _targetMode;
        [SerializeField] private Module _noiseModule;
        [SerializeField] private Module _gravityModule;
        [SerializeField] private Module _vortexModule;
        [SerializeField] private Module _velocityModule;
        [SerializeField] private Enumerations.Simulation _velocitySpace;
        [SerializeField] private SeparatedMinMaxCurve _velocityOverLifetime;
        [SerializeField] private ParticleSystem.MinMaxCurve _vortexStrength;
        [SerializeField] private Module _sheetModule;
        [SerializeField] private Vector2Int _textureTile;
        [SerializeField] private Enumerations.SheetType _sheetType;
        [SerializeField] private ParticleSystem.MinMaxCurve _frameOverTime;
        [SerializeField] private ParticleSystem.MinMaxCurve _startFrame;
        [SerializeField] private SpeedRange _frameSpeedRange;
        [SerializeField] private int _textureSheetFPS;
        [SerializeField] private int _textureSheetCycles;
        [SerializeField] private float _rate;
        [SerializeField] private float _rateOverLifetime;
        [SerializeField] private float _rateOverDistance;
        [SerializeField] private List<Burst> _bursts = new List<Burst>();
        [SerializeField] private ParticleTrailRenderer _particleTrailRenderer;
        [SerializeField] private Module _trailModule;
        [SerializeField] private ParticleSystem.MinMaxCurve _trailWidth;
        [SerializeField] private float _trailLifetime;
        [SerializeField] private float _minimumVertexDistance;
        [SerializeField] private ParticleSystem.MinMaxGradient _trailColorOverLifetime;
        [SerializeField] private ParticleSystem.MinMaxGradient _trailColorOverTrail;
        [SerializeField] private bool _inheritParticleColor;
        [SerializeField] private bool _dieWithParticle;
        [SerializeField] private float _trailRatio;
        [SerializeField] private int _noiseOctaves;
        [SerializeField] private float _noiseFrequency;
        [SerializeField] private float _noiseStrength;
        [SerializeField] private UnityEvent _onStart;
        [SerializeField] private UnityEvent _onFirstParticleFinish;
        [SerializeField] private UnityEvent _onParticleFinish;
        [SerializeField] private UnityEvent _onLastParticleFinish;
        [SerializeField] private UnityEvent _onStop;
        public bool moduleEmitterFoldout;
        public bool moduleParticleFoldout;
        public bool moduleMovementFoldout;
        public bool moduleEventsFoldout;
        [SerializeField] private Texture m_Texture;

        private readonly List<Particle> particles = new List<Particle>();
        private bool playing;
        private bool emitting;
        private float time;
        private float emitAccumulator;
        private int burstIndex;

        public bool IsPlaying { get { return playing; } }
        public int ParticleCount { get { return particles.Count; } }

        public override Texture mainTexture
        {
            get { return m_Texture != null ? m_Texture : base.mainTexture; }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_playMode == Enumerations.PlayMode.OnEnable) Play();
        }

        protected virtual void Update()
        {
            if (!playing) return;

            float dt = _timeScale == Enumerations.TimeScale.Unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            time += dt;

            if (emitting && time >= _startDelay)
            {
                float emitTime = time - _startDelay;

                if (_duration > 0.0f && emitTime >= _duration)
                {
                    if (_loop)
                    {
                        time -= _duration;
                        emitTime -= _duration;
                        burstIndex = 0;
                    }
                    else
                    {
                        emitting = false;
                    }
                }

                if (emitting)
                {
                    emitAccumulator += _rate * dt;
                    while (emitAccumulator >= 1.0f)
                    {
                        emitAccumulator -= 1.0f;
                        Emit();
                    }

                    while (burstIndex < _bursts.Count && emitTime >= _bursts[burstIndex].time)
                    {
                        for (int i = 0; i < _bursts[burstIndex].count; i++) Emit();
                        burstIndex += 1;
                    }
                }
            }

            UpdateParticles(dt);

            if (!emitting && particles.Count == 0)
            {
                FinishPlaying();
            }

            SetVerticesDirty();
        }

        public void Play()
        {
            playing = true;
            emitting = true;
            time = 0.0f;
            emitAccumulator = 0.0f;
            burstIndex = 0;
            particles.Clear();

            if (_onStart != null) _onStart.Invoke();

            if (_children != null)
            {
                for (int i = 0; i < _children.Length; i++)
                {
                    if (_children[i] != null && _children[i] != this) _children[i].Play();
                }
            }

            SetVerticesDirty();
        }

        public void Stop()
        {
            if (!playing) return;

            emitting = false;

            if (_children != null)
            {
                for (int i = 0; i < _children.Length; i++)
                {
                    if (_children[i] != null && _children[i] != this) _children[i].Stop();
                }
            }

            if (particles.Count == 0) FinishPlaying();
        }

        private void FinishPlaying()
        {
            playing = false;
            if (_onLastParticleFinish != null && wasEmittingParticles) _onLastParticleFinish.Invoke();
            wasEmittingParticles = false;
            if (_onStop != null) _onStop.Invoke();
        }

        private bool wasEmittingParticles;
        private bool firstParticleFinished;

        private void UpdateParticles(float dt)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var particle = particles[i];
                particle.age += dt;

                if (particle.age >= particle.lifetime)
                {
                    particles.RemoveAt(i);
                    if (!firstParticleFinished)
                    {
                        firstParticleFinished = true;
                        if (_onFirstParticleFinish != null) _onFirstParticleFinish.Invoke();
                    }
                    if (_onParticleFinish != null) _onParticleFinish.Invoke();
                    continue;
                }

                float t = particle.age / particle.lifetime;
                Vector2 acceleration = Vector2.down * _gravity.Evaluate(t, particle.random);

                if (_targetModule != null && _targetModule.enabled && _attractorTarget != null)
                {
                    Vector2 target = rectTransform.InverseTransformPoint(_attractorTarget.position);
                    float pull = Mathf.Clamp01(_toTarget.Evaluate(t, particle.random) * dt);
                    particle.position = Vector2.Lerp(particle.position, target, pull);
                }

                particle.velocity += acceleration * dt;
                particle.position += particle.velocity * dt;
            }
        }

        private void Emit()
        {
            var particle = new Particle();
            particle.random = Random.value;
            particle.lifetime = Mathf.Max(0.01f, _lifetime.Evaluate(0.0f, Random.value));

            Vector2 direction;
            particle.position = SpawnPosition(out direction);

            float speed = _startSpeed.Evaluate(0.0f, Random.value);
            particle.velocity = direction * speed;
            particle.size = _startSize.Evaluate(0.0f, Random.value);
            particle.rotation = _startRotation.Evaluate(0.0f, Random.value);
            particle.color = _startColor.Evaluate(0.0f, Random.value);

            particles.Add(particle);
            wasEmittingParticles = true;
            firstParticleFinished = false;
        }

        private Vector2 SpawnPosition(out Vector2 direction)
        {
            switch (_shape)
            {
                case Enumerations.EmitterShape.Circle:
                {
                    float angle = Random.value * Mathf.PI * 2.0f;
                    direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    return direction * (_radius * Random.value);
                }
                case Enumerations.EmitterShape.Rectangle:
                {
                    direction = Vector2.up;
                    return new Vector2((Random.value - 0.5f) * _width, (Random.value - 0.5f) * _height);
                }
                case Enumerations.EmitterShape.Line:
                {
                    direction = Vector2.up;
                    return new Vector2((Random.value - 0.5f) * _length, 0.0f);
                }
                case Enumerations.EmitterShape.Directional:
                {
                    direction = Quaternion.Euler(0.0f, 0.0f, _angle) * Vector2.up;
                    return Vector2.zero;
                }
                default:
                {
                    float angle = Random.value * Mathf.PI * 2.0f;
                    direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    return Vector2.zero;
                }
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            for (int i = 0; i < particles.Count; i++)
            {
                var particle = particles[i];
                float half = particle.size * 0.5f;
                float radians = particle.rotation * Mathf.Deg2Rad;
                float cos = Mathf.Cos(radians);
                float sin = Mathf.Sin(radians);

                Vector2[] corners = new Vector2[4];
                corners[0] = new Vector2(-half, -half);
                corners[1] = new Vector2(half, -half);
                corners[2] = new Vector2(half, half);
                corners[3] = new Vector2(-half, half);

                Vector2[] uvs = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
                Color32 color = (particle.color * this.color);

                int first = vh.currentVertCount;
                for (int c = 0; c < 4; c++)
                {
                    var p = corners[c];
                    var rotated = new Vector2(p.x * cos - p.y * sin, p.x * sin + p.y * cos) + particle.position;

                    UIVertex vertex = UIVertex.simpleVert;
                    vertex.position = rotated;
                    vertex.color = color;
                    vertex.uv0 = uvs[c];
                    vh.AddVert(vertex);
                }

                vh.AddTriangle(first, first + 1, first + 2);
                vh.AddTriangle(first + 2, first + 3, first);
            }
        }
    }
}
