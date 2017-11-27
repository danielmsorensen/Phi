using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

    public AudioClip clickDown, clickUp, rollover, disabled;
    public AudioClip music;
    public float musicVolume=1;
    public float sfxVolume=1;

    public static SoundManager instance;

    AudioSource source2D, source3D;

    AudioSource[] musicSources;
    int musicIndex;

    public bool isMusicPlaying {
        get {
            for (int i = 0; i <= 1; i++) {
                if (musicSources[i].isPlaying) {
                    return true;
                }
            }
            return false;
        }
    }

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        source2D = gameObject.AddComponent<AudioSource>();
        source2D.spatialBlend = 0;
        source2D.playOnAwake = false;

        source3D = new GameObject("AudioSource3D").AddComponent<AudioSource>();
        source3D.transform.SetParent(transform);
        source3D.spatialBlend = 1;

        musicSources = new AudioSource[2];
        for (int i = 0; i <= 1; i++) {
            musicSources[i] = new GameObject("MusicSource" + (i + 1)).AddComponent<AudioSource>();
            musicSources[i].transform.SetParent(transform);
            musicSources[i].spatialBlend = 0;
            musicSources[i].loop = true;
        }
    }

    void Start() {
        if (!isMusicPlaying) PlayMusic(music, 0, 0, musicVolume);
    }

    #region UI Sound Functions
    public void ClickDown() {
        PlaySound2D(clickDown, 0, 1 / sfxVolume);
    }
    public void ClickUp() {
        PlaySound2D(clickUp, 0, 1 / sfxVolume);
    }
    public void Rollover() {
        PlaySound2D(rollover, 0, 1 / sfxVolume);
    }
    public void PlayDisabledTone() {
        PlaySound2D(disabled, 0, 8 / sfxVolume);
    }
    #endregion

    public void PlaySound2D(AudioClip clip, float point=0, float volume=1) {
        if (Application.isPlaying) {
            source2D.PlayOneShot(clip, volume * sfxVolume);
            source2D.time = point;
        }
    }

    public void PlaySound3D(AudioClip clip, Vector3 pos, float point=0, float volume=1) {
        if (Application.isPlaying) {
            source3D.Stop();
            source3D.clip = clip;
            source3D.transform.position = pos;
            source3D.Play();
            source3D.time = point;
            source3D.volume = volume * sfxVolume;
        }
    }

    public void PlayMusic(AudioClip clip, float fadeDuration=0, float point=0, float volume=1) {
        if (Application.isPlaying) {
            musicIndex = 1 - musicIndex;
            musicSources[musicIndex].clip = clip;
            musicSources[musicIndex].Play();
            musicSources[musicIndex].time = point;

            StartCoroutine(MusicFade(fadeDuration, volume));
        }
    }

    IEnumerator MusicFade(float duration, float volume) {
        float perc = 0;

        while(perc < 1) {
            perc += Time.deltaTime * (1 / duration);

            musicSources[musicIndex].volume = Mathf.Lerp(0, volume, perc);
            musicSources[1 - musicIndex].volume = Mathf.Lerp(volume, 0, perc);

            yield return null;
        }

        musicSources[1 - musicIndex].Stop();
    }
}
