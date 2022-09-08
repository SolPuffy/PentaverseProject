using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class SoundManager : MonoBehaviour
{
    public Sound[] Sounds;

    public static SoundManager instance;

    
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

      foreach(Sound s in Sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.playOnAwake = false;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;           
            s.source.loop = s.loop;
        }
        if (!Application.isBatchMode)
        {
            Sounds[10].source.Play();
        }
    }

    

   
}
