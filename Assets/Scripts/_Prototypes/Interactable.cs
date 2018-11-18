using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioPlayer))]
public class Interactable : MonoBehaviour
{

        //REFERENCES
    [SerializeField] protected Player_Proto_001 m_playerRef;
    private AudioPlayer m_audioPlayer;
        //PROPERTIES
    private bool mB_beingUsed;
    private float m_usedIntensity;
    private List<AudioSource> m_audioSources;
    
        //CUSTOM
    [SerializeField] private bool mB_singleShot;
    [SerializeField] private float mFlt_noiseImpact;
    
    [SerializeField] private bool mB_blockZMovement;
    [SerializeField] private bool mB_blockYMovement;
    [SerializeField] private bool mB_blockXMovement;

    [SerializeField] private List<AudioClip> m_audioSamples;
	// Use this for initialization
	private void Start()
    {
        Initialization();
    }

    private void Initialization()
    {
        m_audioSources = new List<AudioSource>(this.GetComponents<AudioSource>());//Getting the audio sources attached to this gameObject.
        m_audioPlayer = this.GetComponent<AudioPlayer>();//Referencing the audio player attached to this gameObject.
        mB_beingUsed = false;
    }

    private void Update()
    {
        NoiseGeneration();//Calls the function responsible for the noise generation.
    }

    private void NoiseGeneration()
    {
        m_usedIntensity = m_playerRef.InputTriggerIntens();

        if(!mB_singleShot)//If this object has not a single shot interaction.
        {
            if(mB_beingUsed)//And is currently being used.
            {
                if(Mathf.Abs(m_usedIntensity) > 0f)//Checking the input intensity of the player and playing the right sounds.
                    m_audioPlayer.TryPlaySound(m_audioSources[0], m_audioSamples[0]);//Light sound
                else
                    StopSourceAtIndex(new int[] {0, 1, 2});

                if(m_usedIntensity >= 0.33f)
                    m_audioPlayer.TryPlaySound(m_audioSources[1], m_audioSamples[1]);//Mid sound
                else
                    StopSourceAtIndex(new int[] {1, 2});

                if(m_usedIntensity >= 0.66f && m_usedIntensity < 1f)
                    m_audioPlayer.TryPlaySound(m_audioSources[2], m_audioSamples[2]);//Heavy sound
                else
                    StopSourceAtIndex(new int[] {2});
            }//Otherwise if an audio source is still playing, it will stop.
            else
            {
                if(m_audioSources[0].isPlaying || m_audioSources[1].isPlaying || m_audioSources[2].isPlaying)
                {
                    StopSourceAtIndex(new int[] {0, 1, 2});
                }
            }
        }
    }

    //Stop the source indicated by the indexes in the audio sources list.
    private void StopSourceAtIndex(int[] indexes)
    {
        for(int i = 0 ; i < indexes.Length ; i++)
        {
            m_audioPlayer.StartFadeSource(m_audioSources[indexes[i]], 0.3f);
        }
    }

    public virtual void Interact()
    {
        Interactable[] temp = this.GetComponents<Interactable>();
        for(int i = 0 ; i < temp.Length ; i++)
        {
            if(temp[i] != this)
            {
                temp[i].enabled = false;
            }
        }

        mB_beingUsed = true;
        //Other interactions functions
        if(mB_singleShot)//If the object has a singleshot interaction and doesn't need an input sensitivity, it turns off.
            mB_beingUsed = false;
    }

    public virtual void Release()
    {
        Interactable[] temp = this.GetComponents<Interactable>();
        for(int i = 0 ; i < temp.Length ; i++)
        {
            if(temp[i] != this)
            {
                temp[i].enabled = true;
            }
        }
        mB_beingUsed = false;
        m_usedIntensity = 0;
    }
    
        //ACCESSORS
    
    public float GetNoiseImpact()
    {
        return mFlt_noiseImpact;
    }

    public bool IsDragged()
    {
        return mB_beingUsed;
    }

    public bool X_Blocked()
    {
        return mB_blockXMovement;
    }

    public bool Y_Blocked()
    {
        return mB_blockYMovement;
    }

    public bool Z_Blocked()
    {
        return mB_blockZMovement;
    }

        //MUTATORS
    public void SetUsedIntensity(float intensity)
    {
        m_usedIntensity = intensity;
    }
}
