using UnityEngine;

/// <summary>
/// SoundManager handles background music and sound effects.
/// Uses high-quality imported audio clips with full procedural synthesis fallback.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private AudioClip bgmClip;
    private AudioClip collectClip;
    private AudioClip jumpClip;
    private AudioClip deathClip;
    private AudioClip unlockClip;
    private AudioClip winClip;
    private AudioClip clickClip;
    private AudioClip footstepClip;

    private bool isMuted = false;
    public static bool IsMuted => Instance != null && Instance.isMuted;

    public static void Ensure()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("SoundManager");
            Instance = go.AddComponent<SoundManager>();
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(go);
            }
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
        }

        // Setup audio sources with clear, audible volume
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.45f;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.volume = 0.85f;
        sfxSource.playOnAwake = false;

        LoadOrSynthesizeClips();

        if (bgmClip != null)
        {
            musicSource.clip = bgmClip;
            musicSource.Play();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMute();
        }
    }

    public static void ToggleMute()
    {
        Ensure();
        Instance.isMuted = !Instance.isMuted;
        Instance.musicSource.mute = Instance.isMuted;
        Instance.sfxSource.mute = Instance.isMuted;
    }

    public static void Play(string soundName)
    {
        Ensure();
        if (Instance == null || Instance.sfxSource == null) return;

        AudioClip clipToPlay = null;
        switch (soundName.ToLower())
        {
            case "collect":
            case "crystal":
                clipToPlay = Instance.collectClip;
                break;
            case "jump":
                clipToPlay = Instance.jumpClip;
                break;
            case "death":
            case "die":
            case "hit":
                clipToPlay = Instance.deathClip;
                break;
            case "unlock":
            case "portal":
                clipToPlay = Instance.unlockClip;
                break;
            case "win":
            case "victory":
                clipToPlay = Instance.winClip;
                break;
            case "click":
            case "button":
                clipToPlay = Instance.clickClip;
                break;
            case "footstep":
                clipToPlay = Instance.footstepClip;
                break;
        }

        if (clipToPlay != null)
        {
            Instance.sfxSource.PlayOneShot(clipToPlay);
        }
    }

    private void LoadOrSynthesizeClips()
    {
        // 1. Try loading from Resources/Audio/
        bgmClip = Resources.Load<AudioClip>("Audio/bgm");
        collectClip = Resources.Load<AudioClip>("Audio/crystal_pickup");
        jumpClip = Resources.Load<AudioClip>("Audio/jump");
        deathClip = Resources.Load<AudioClip>("Audio/death");
        unlockClip = Resources.Load<AudioClip>("Audio/portal_unlock");
        winClip = Resources.Load<AudioClip>("Audio/level_win");
        clickClip = Resources.Load<AudioClip>("Audio/button_click");
        footstepClip = Resources.Load<AudioClip>("Audio/footstep");

        // 2. Synthesize fallbacks if any are missing
        if (bgmClip == null) bgmClip = SynthesizeMelody("ProceduralBGM", new float[] { 261.63f, 329.63f, 392f, 523.25f, 440f, 349.23f, 392f, 329.63f }, 0.4f);
        if (collectClip == null) collectClip = SynthesizeTone("ProceduralCollect", new float[] { 880f, 1175f, 1568f, 2093f }, 0.09f, 0.6f);
        if (jumpClip == null) jumpClip = SynthesizeSweep("ProceduralJump", 220f, 650f, 0.25f);
        if (deathClip == null) deathClip = SynthesizeDeath("ProceduralDeath", 0.6f);
        if (unlockClip == null) unlockClip = SynthesizeTone("ProceduralUnlock", new float[] { 523f, 659f, 784f, 1047f, 1318f }, 0.16f, 0.7f);
        if (winClip == null) winClip = SynthesizeTone("ProceduralWin", new float[] { 523f, 659f, 784f, 1047f, 1319f, 1568f }, 0.22f, 0.8f);
        if (clickClip == null) clickClip = SynthesizeTone("ProceduralClick", new float[] { 900f }, 0.04f, 0.5f);
        if (footstepClip == null) footstepClip = SynthesizeNoiseThump("ProceduralFootstep", 0.08f);
    }

    private AudioClip SynthesizeTone(string name, float[] notes, float noteDuration, float volume)
    {
        int rate = 22050;
        int count = Mathf.CeilToInt(notes.Length * noteDuration * rate);
        float[] data = new float[count];
        for (int n = 0; n < notes.Length; n++)
        {
            int noteSamples = (int)(noteDuration * rate);
            for (int j = 0; j < noteSamples; j++)
            {
                int k = n * noteSamples + j;
                if (k >= count) break;
                float t = j / (float)rate;
                float env = Mathf.Exp(-t * 8f);
                data[k] = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * env * volume;
            }
        }
        AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip SynthesizeSweep(string name, float startFreq, float endFreq, float duration)
    {
        int rate = 22050;
        int count = Mathf.CeilToInt(duration * rate);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)rate;
            float freq = Mathf.Lerp(startFreq, endFreq, t / duration);
            float env = Mathf.Sin(Mathf.PI * (t / duration));
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.7f;
        }
        AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip SynthesizeDeath(string name, float duration)
    {
        int rate = 22050;
        int count = Mathf.CeilToInt(duration * rate);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)rate;
            float freq = Mathf.Lerp(350f, 60f, t / duration);
            float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 12f) * 0.4f;
            float tone = Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Exp(-t * 4f) * 0.6f;
            data[i] = Mathf.Clamp(tone + noise, -0.95f, 0.95f);
        }
        AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip SynthesizeNoiseThump(string name, float duration)
    {
        int rate = 22050;
        int count = Mathf.CeilToInt(duration * rate);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)rate;
            float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 30f) * 0.2f;
            float thump = Mathf.Sin(2f * Mathf.PI * 90f * t) * Mathf.Exp(-t * 25f) * 0.3f;
            data[i] = noise + thump;
        }
        AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip SynthesizeMelody(string name, float[] notes, float noteDuration)
    {
        int rate = 22050;
        int count = Mathf.CeilToInt(notes.Length * noteDuration * rate);
        float[] data = new float[count];
        for (int n = 0; n < notes.Length; n++)
        {
            int noteSamples = (int)(noteDuration * rate);
            for (int j = 0; j < noteSamples; j++)
            {
                int k = n * noteSamples + j;
                if (k >= count) break;
                float t = j / (float)rate;
                float env = Mathf.Sin(Mathf.PI * (j / (float)noteSamples));
                data[k] = (Mathf.Sin(2f * Mathf.PI * notes[n] * t) + 0.3f * Mathf.Sin(4f * Mathf.PI * notes[n] * t)) * env * 0.35f;
            }
        }
        AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
