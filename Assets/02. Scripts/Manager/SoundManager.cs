using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/*
// ���׷��̵� ���� ��
SoundManager.Instance.PlaySfx(SoundManager.SfxId.Upgrade);

// ĳ�� �߻�
SoundManager.Instance.PlaySfx(SoundManager.SfxId.CannonShot);

// ����
SoundManager.Instance.PlaySfx(SoundManager.SfxId.Explosion);

// ��/���ɾ���
SoundManager.Instance.PlaySfx(SoundManager.SfxId.Healing);
SoundManager.Instance.PlaySfx(SoundManager.SfxId.SpinAttack);
 */

public class SoundManager : Singleton<SoundManager>
{
    [Header("Mixer (����)")]
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("UI �����̴�(����)")]
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

    [Header("SFX ����")]
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField][Range(0f, 0.5f)] private float pitchVariance = 0.05f;
    [SerializeField] private int sfxPoolSize = 10;

    private AudioSource bgmSource;
    private readonly List<AudioSource> sfxPool = new List<AudioSource>();

    public enum SfxId { Healing, SpinAttack, Upgrade, CannonShot, Explosion }

    private Dictionary<SfxId, AudioClip> sfxMap;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // BGM �ҽ�
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.outputAudioMixerGroup = bgmGroup;
        bgmSource.volume = musicVolume;

        // SFX Ǯ
        for (int i = 0; i < sfxPoolSize; i++)
        {
            var src = new GameObject($"SFX_{i}").AddComponent<AudioSource>();
            src.transform.SetParent(transform);
            src.playOnAwake = false;
            src.loop = false;
            src.outputAudioMixerGroup = sfxGroup;
            src.spatialBlend = 0f; // 2D
            sfxPool.Add(src);
        }

        // SFX ����
        sfxMap = new Dictionary<SfxId, AudioClip>
        {
            { SfxId.Healing,     healing   },
            { SfxId.SpinAttack,  spinAttack},
            { SfxId.Upgrade,     upgrade   },
            { SfxId.CannonShot,  cannonShot},
            { SfxId.Explosion,   explosion },
        };

        // �����̴� ����(����)
        if (bgmSlider) bgmSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    void Start()
    {
        // �⺻ BGM ���
        PlayBgm(backGround);
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
        // ��� ��� ���̸� ù ��° �ҽ� ����
        return sfxPool[0];
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        // ��� ���� �ҽ��鵵 ��� �ݿ�
        foreach (var s in sfxPool) s.volume = sfxVolume;
    }
}
