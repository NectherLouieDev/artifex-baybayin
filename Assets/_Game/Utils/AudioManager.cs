using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM Settings")]
    [SerializeField] private AudioSource _bgmSource1;
    [SerializeField] private AudioSource _bgmSource2;
    [SerializeField] private float _bgmFadeDuration = 2f;

    [Header("SFX Pool Settings")]
    [SerializeField] private int _initialPoolSize = 10;
    [SerializeField] private GameObject _sfxPrefab;

    private Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
    private AudioSource _currentBGMSource;
    private AudioSource _nextBGMSource;
    private Coroutine _fadeCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        // Initialize BGM sources if not set
        if (_bgmSource1 == null) _bgmSource1 = gameObject.AddComponent<AudioSource>();
        if (_bgmSource2 == null) _bgmSource2 = gameObject.AddComponent<AudioSource>();

        _bgmSource1.loop = true;
        _bgmSource2.loop = true;
        _bgmSource1.playOnAwake = false;
        _bgmSource2.playOnAwake = false;

        _currentBGMSource = _bgmSource1;
        _nextBGMSource = _bgmSource2;

        // Initialize SFX pool
        InitializeSFXPool();
    }

    void InitializeSFXPool()
    {
        for (int i = 0; i < _initialPoolSize; i++)
        {
            CreateNewSFXObject();
        }
    }

    AudioSource CreateNewSFXObject()
    {
        AudioSource newSource;

        if (_sfxPrefab != null)
        {
            GameObject newObj = Instantiate(_sfxPrefab, transform);
            newSource = newObj.GetComponent<AudioSource>();
        }
        else
        {
            GameObject newObj = new GameObject("SFX_" + (_sfxPool.Count + 1));
            newObj.transform.SetParent(transform);
            newSource = newObj.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
        }

        _sfxPool.Enqueue(newSource);
        return newSource;
    }

    AudioSource GetSFXSource()
    {
        if (_sfxPool.Count == 0)
        {
            CreateNewSFXObject();
        }

        AudioSource source = _sfxPool.Dequeue();
        source.gameObject.SetActive(true);
        return source;
    }

    void ReturnSFXSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        _sfxPool.Enqueue(source);
    }

    public AudioSource PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false, float delay = 0)
    {
        if (clip == null)
            return null;

        AudioSource source = GetSFXSource();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.spatialBlend = 0f;

        if (delay <= 0)
        {
            source.Play();

            if (!loop)
            {
                StartCoroutine(ReturnSFXWhenFinished(source));
            }
        }
        else
        {
            StartCoroutine(ExecuteDelayed(source, delay, loop));
        }

        return source;
    }

    IEnumerator ExecuteDelayed(AudioSource source, float delay, bool loop)
    {
        yield return new WaitForSeconds(delay);

        source.Play();

        if (!loop)
        {
            StartCoroutine(ReturnSFXWhenFinished(source));
        }
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) 
            return;

        AudioSource source = GetSFXSource();
        source.transform.position = position;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.spatialBlend = 1f; // 3D audio
        source.Play();

        StartCoroutine(ReturnSFXWhenFinished(source));
    }

    IEnumerator ReturnSFXWhenFinished(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        ReturnSFXSource(source);
    }

    public void StopSFX(AudioSource source)
    {
        if (source != null && source.isPlaying)
        {
            ReturnSFXSource(source);
        }
    }

    public void PlayBGM(AudioClip bgmClip = null, float volume = 1f, bool fade = true)
    {
        if (bgmClip == null || bgmClip == _currentBGMSource.clip) return;

        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(SwitchBGM(bgmClip, volume, fade));
    }

    IEnumerator SwitchBGM(AudioClip newClip, float volume, bool fade)
    {
        _nextBGMSource.clip = newClip;
        _nextBGMSource.volume = fade ? 0f : volume;
        _nextBGMSource.Play();

        if (fade)
        {
            // Fade out current BGM while fading in new BGM
            float timer = 0f;
            float currentVolume = _currentBGMSource.volume;

            while (timer < _bgmFadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / _bgmFadeDuration;

                _currentBGMSource.volume = Mathf.Lerp(currentVolume, 0f, t);
                _nextBGMSource.volume = Mathf.Lerp(0f, volume, t);

                yield return null;
            }

            _currentBGMSource.Stop();
        }
        else
        {
            _currentBGMSource.Stop();
            _nextBGMSource.volume = volume;
        }

        // Swap references
        AudioSource temp = _currentBGMSource;
        _currentBGMSource = _nextBGMSource;
        _nextBGMSource = temp;

        _fadeCoroutine = null;
    }

    public void FadeOutBGM(float duration = -1f)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(FadeOutCurrentBGM(duration));
    }

    IEnumerator FadeOutCurrentBGM(float duration)
    {
        float fadeDuration = duration > 0 ? duration : _bgmFadeDuration;
        float startVolume = _currentBGMSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            _currentBGMSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        _currentBGMSource.Stop();
        _currentBGMSource.volume = startVolume;
        _fadeCoroutine = null;
    }

    public void SetBGMVolume(float volume)
    {
        _currentBGMSource.volume = volume;
    }

    public void PauseBGM()
    {
        _currentBGMSource.Pause();
    }

    public void ResumeBGM()
    {
        _currentBGMSource.UnPause();
    }

    public void StopBGM()
    {
        _currentBGMSource.Stop();
        _nextBGMSource.Stop();
    }

    public bool IsBGMPlaying()
    {
        return _currentBGMSource.isPlaying;
    }

    public AudioClip GetCurrentBGM()
    {
        return _currentBGMSource.clip;
    }

    public float GetBGMVolume()
    {
        return _currentBGMSource.volume;
    }

    public int GetActiveSFXCount()
    {
        int count = 0;
        foreach (AudioSource source in GetComponentsInChildren<AudioSource>())
        {
            if (source.isPlaying && source != _currentBGMSource && source != _nextBGMSource)
            {
                count++;
            }
        }
        return count;
    }
}