using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitalSalvage.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Clips (15 Procedural Recipes)")]
        [SerializeField] private AudioClip sfxRatchet;
        [SerializeField] private AudioClip sfxThruster;
        [SerializeField] private AudioClip sfxDebrisBounce;
        [SerializeField] private AudioClip sfxPinballBumper;
        [SerializeField] private AudioClip sfxMultiplier;
        [SerializeField] private AudioClip sfxAdder;
        [SerializeField] private AudioClip sfxBooster;
        [SerializeField] private AudioClip sfxFlapper;
        [SerializeField] private AudioClip sfxSwallow;
        [SerializeField] private AudioClip sfxQuotaGrowth;
        [SerializeField] private AudioClip sfxButtonClick;
        [SerializeField] private AudioClip sfxTabSwitch;
        [SerializeField] private AudioClip sfxBackupPod;
        [SerializeField] private AudioClip sfxVictory;
        [SerializeField] private AudioClip sfxDefeat;

        [Header("Settings")]
        [SerializeField] private bool isMuted = false;
        [SerializeField] private float masterVolume = 0.85f;
        [SerializeField] private int audioSourcePoolSize = 12;

        private List<AudioSource> sourcePool = new List<AudioSource>();
        private int currentSourceIndex = 0;

        private float lastRatchetTime = 0f;
        private float lastBounceTime = 0f;
        private float lastSwallowTime = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitSourcePool();
            LoadAudioClipsIfNull();
        }

        private void InitSourcePool()
        {
            for (int i = 0; i < audioSourcePoolSize; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f; // 2D audio
                sourcePool.Add(src);
            }
        }

        private void LoadAudioClipsIfNull()
        {
            if (sfxRatchet == null) sfxRatchet = Resources.Load<AudioClip>("Audio/sfx_centrifuge_ratchet");
            if (sfxThruster == null) sfxThruster = Resources.Load<AudioClip>("Audio/sfx_thruster_burst");
            if (sfxDebrisBounce == null) sfxDebrisBounce = Resources.Load<AudioClip>("Audio/sfx_debris_wall_bounce");
            if (sfxPinballBumper == null) sfxPinballBumper = Resources.Load<AudioClip>("Audio/sfx_debris_pinball_bumper");
            if (sfxMultiplier == null) sfxMultiplier = Resources.Load<AudioClip>("Audio/sfx_multiplier_pass");
            if (sfxAdder == null) sfxAdder = Resources.Load<AudioClip>("Audio/sfx_adder_pass");
            if (sfxBooster == null) sfxBooster = Resources.Load<AudioClip>("Audio/sfx_booster_pass");
            if (sfxFlapper == null) sfxFlapper = Resources.Load<AudioClip>("Audio/sfx_flapper_clack");
            if (sfxSwallow == null) sfxSwallow = Resources.Load<AudioClip>("Audio/sfx_singularity_swallow");
            if (sfxQuotaGrowth == null) sfxQuotaGrowth = Resources.Load<AudioClip>("Audio/sfx_quota_growth");
            if (sfxButtonClick == null) sfxButtonClick = Resources.Load<AudioClip>("Audio/sfx_ui_button_click");
            if (sfxTabSwitch == null) sfxTabSwitch = Resources.Load<AudioClip>("Audio/sfx_ui_tab_switch");
            if (sfxBackupPod == null) sfxBackupPod = Resources.Load<AudioClip>("Audio/sfx_recovery_pod_launch");
            if (sfxVictory == null) sfxVictory = Resources.Load<AudioClip>("Audio/sfx_victory_fanfare");
            if (sfxDefeat == null) sfxDefeat = Resources.Load<AudioClip>("Audio/sfx_defeat_whimper");
        }

        private AudioSource GetNextSource()
        {
            var src = sourcePool[currentSourceIndex];
            currentSourceIndex = (currentSourceIndex + 1) % sourcePool.Count;
            return src;
        }

        public void PlayClip(AudioClip clip, float volumeScale = 1.0f, float pitch = 1.0f)
        {
            if (isMuted || clip == null) return;
            var src = GetNextSource();
            src.pitch = pitch;
            src.volume = masterVolume * volumeScale;
            src.PlayOneShot(clip);
        }

        public void PlayRatchet(float pitchMultiplier = 1.0f)
        {
            if (Time.time - lastRatchetTime < 0.045f) return;
            lastRatchetTime = Time.time;
            PlayClip(sfxRatchet, 0.6f, Mathf.Clamp(pitchMultiplier, 0.7f, 1.4f));
        }

        public void PlayThruster()
        {
            PlayClip(sfxThruster, 0.8f, UnityEngine.Random.Range(0.95f, 1.05f));
        }

        public void PlayDebrisBounce(float impactVelocity)
        {
            if (Time.time - lastBounceTime < 0.048f) return;
            lastBounceTime = Time.time;
            float vol = Mathf.Clamp(impactVelocity * 0.12f, 0.2f, 0.8f);
            float pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            PlayClip(sfxDebrisBounce, vol, pitch);
        }

        public void PlayBumper()
        {
            PlayClip(sfxPinballBumper, 0.85f, UnityEngine.Random.Range(0.98f, 1.04f));
        }

        public void PlayMultiplier()
        {
            PlayClip(sfxMultiplier, 0.9f, 1.0f);
        }

        public void PlayAdder()
        {
            PlayClip(sfxAdder, 0.85f, 1.0f);
        }

        public void PlayBooster()
        {
            PlayClip(sfxBooster, 0.9f, 1.0f);
        }

        public void PlayFlapper()
        {
            PlayClip(sfxFlapper, 0.75f, UnityEngine.Random.Range(0.95f, 1.05f));
        }

        public void PlaySwallow()
        {
            if (Time.time - lastSwallowTime < 0.035f) return;
            lastSwallowTime = Time.time;
            PlayClip(sfxSwallow, 0.95f, UnityEngine.Random.Range(0.95f, 1.05f));
        }

        public void PlayQuotaGrowth()
        {
            PlayClip(sfxQuotaGrowth, 0.9f, 1.0f);
        }

        public void PlayButtonClick()
        {
            PlayClip(sfxButtonClick, 0.8f, 1.0f);
        }

        public void PlayTabSwitch()
        {
            PlayClip(sfxTabSwitch, 0.7f, 1.0f);
        }

        public void PlayBackupPod()
        {
            PlayClip(sfxBackupPod, 0.95f, 1.0f);
        }

        public void PlayVictory()
        {
            PlayClip(sfxVictory, 1.0f, 1.0f);
        }

        public void PlayDefeat()
        {
            PlayClip(sfxDefeat, 0.9f, 1.0f);
        }

        public void ToggleMute()
        {
            isMuted = !isMuted;
        }
    }
}
