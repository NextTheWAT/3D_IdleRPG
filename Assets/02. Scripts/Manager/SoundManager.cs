using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/*
// 업그레이드 성공 시
SoundManager.Instance.PlaySfx(SoundManager.SfxId.Upgrade);

// 캐논 발사
SoundManager.Instance.PlaySfx(SoundManager.SfxId.CannonShot);

// 폭발
SoundManager.Instance.PlaySfx(SoundManager.SfxId.Explosion);

// 힐/스핀어택
SoundManager.Instance.PlaySfx(SoundManager.SfxId.Healing);
SoundManager.Instance.PlaySfx(SoundManager.SfxId.SpinAttack);
 */

public class SoundManager : Singleton<SoundManager>
{
    [Header("Mixer (선택)")]
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("UI 슬라이더(선택)")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("BGM")]
    [SerializeField] private AudioClip backGround; // BackGround.mp3
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.8f;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip healing;       // Healing.wav
    [SerializeField] private AudioClip spinAttack;    // SpinAttack.wav
    [SerializeField] private AudioClip upgrade;       // Upgrade.wav
    [SerializeField] private AudioClip cannonShot;    // CannonShot.mp3
    [SerializeField] private AudioClip explosion;     // Explosion.mp3

    [Header("SFX 설정")]
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField][Range(0f, 0.5f)] private float pitchVariance = 0.05f;
    [SerializeField] private int sfxPoolSize = 10;

    private AudioSource bgmSource;
    private readonly List<AudioSource> sfxPool = new List<AudioSource>();

    public enum SfxId { Healing, SpinAttack, Upgrade, CannonShot, Explosion }

    private Dictionary<SfxId, AudioClip> sfxMap;

    private bool _inited = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        EnsureInit();
    }

    void Start()
    {
        // 기본 BGM 재생
        PlayBgm(backGround);
    }

    private void EnsureInit()
    {
        if (_inited) return;

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.outputAudioMixerGroup = bgmGroup;
            bgmSource.volume = musicVolume;
        }

        if (sfxPool.Count == 0)
        {
            int count = Mathf.Max(1, sfxPoolSize);
            for (int i = 0; i < count; i++)
            {
                var src = new GameObject($"SFX_{i}").AddComponent<AudioSource>();
                src.transform.SetParent(transform);
                src.playOnAwake = false;
                src.loop = false;
                src.outputAudioMixerGroup = sfxGroup;
                src.spatialBlend = 0f; // 2D
                sfxPool.Add(src);
            }
        }

        if (sfxMap == null)
        {
            sfxMap = new Dictionary<SfxId, AudioClip>
            {
                { SfxId.Healing,     healing   },
                { SfxId.SpinAttack,  spinAttack},
                { SfxId.Upgrade,     upgrade   },
                { SfxId.CannonShot,  cannonShot},
                { SfxId.Explosion,   explosion },
            };
        }

        // (선택) 슬라이더 리스너도 여기서 보장
        if (bgmSlider) { bgmSlider.onValueChanged.RemoveListener(SetMusicVolume); bgmSlider.onValueChanged.AddListener(SetMusicVolume); }
        if (sfxSlider) { sfxSlider.onValueChanged.RemoveListener(SetSfxVolume); sfxSlider.onValueChanged.AddListener(SetSfxVolume); }

        _inited = true;
    }

    // ===================== BGM =====================

    public void PlayBgm(AudioClip clip, float fadeTime = 0.25f)
    {
        if (clip == null) return;
        if (!isActiveAndEnabled) return;
        StopAllCoroutines();
        StartCoroutine(CoFadeTo(clip, fadeTime));
    }

    public void StopBgm(float fadeTime = 0.25f)
    {
        if (!isActiveAndEnabled) return;
        StopAllCoroutines();
        StartCoroutine(CoFadeTo(null, fadeTime));
    }

    System.Collections.IEnumerator CoFadeTo(AudioClip next, float t)
    {
        float startVol = bgmSource.volume;
        for (float e = 0; e < t; e += Time.unscaledDeltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVol, 0f, e / t);
            yield return null;
        }
        bgmSource.volume = 0f;

        if (next == null)
        {
            bgmSource.Stop();
        }
        else
        {
            bgmSource.clip = next;
            bgmSource.Play();
        }

        for (float e = 0; e < t; e += Time.unscaledDeltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, musicVolume, e / t);
            yield return null;
        }
        bgmSource.volume = musicVolume;
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        bgmSource.volume = musicVolume;
    }

    // ===================== SFX =====================

    public void PlaySfx(SfxId id, float volumeScale = 1f)
    {
        EnsureInit();
        if (!sfxMap.TryGetValue(id, out var clip) || clip == null) return;
        var src = GetFreeSfxSource();
        src.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        src.volume = sfxVolume * Mathf.Clamp01(volumeScale);
        src.clip = clip;
        src.Play();
    }

    private AudioSource GetFreeSfxSource()
    {
        foreach (var s in sfxPool)
        {
            if (!s.isPlaying) return s;
        }
        // 모두 재생 중이면 첫 번째 소스 재사용
        return sfxPool[0];
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        // 재생 중인 소스들도 즉시 반영
        foreach (var s in sfxPool) s.volume = sfxVolume;
    }
}
