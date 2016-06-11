using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour{

    //Here is a private reference only this class can access
    private static SoundManager _instance;

    //This is the public reference that other classes will use
    public static SoundManager instance
    {
        get
        {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<SoundManager>();
            return _instance;
        }
    }

    AudioSource music;
    public float musicVolume;
    public List<AudioClip> battleMusic = new List<AudioClip>();

    public AudioClip selectSound;
    public AudioClip backSound;

	// Use this for initialization
	void Start () {

        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void PlayBattleMusic()
    {
        int index = Random.Range(0, battleMusic.Count);
        music.clip = battleMusic[index];
        music.volume = musicVolume;
        music.Play();
    }

    public void PlaySelectSound()
    {
        PlaySoundEffect(selectSound, 1);
    }

    public void PlayBackSound()
    {
        PlaySoundEffect(backSound, 1);
    }

    public void PlaySoundEffect(AudioClip clip, float volume)
    {
        //create a new audio source for the effect
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.volume = volume;

        StartCoroutine(PlayAudioOneShot(newSource));
    }

    //Used to play a audio source and destroy it on completion
    IEnumerator PlayAudioOneShot(AudioSource source)
    {
        source.Play();
        yield return new WaitForSeconds(source.clip.length);
        Destroy(source);
    }

}
