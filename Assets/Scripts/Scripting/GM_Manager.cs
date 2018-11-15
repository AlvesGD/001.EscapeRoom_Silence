using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GM_Manager : MonoBehaviour
{
    public enum ENUM_Event
    {
        Thunder, Train, NB_ENTRIES
    }

    public enum ENUM_EventProperty
    {
        Event, Announce, NB_ENTRIES
    }

    [System.Serializable]
    public struct STRUCT_Event
    {
        public AudioClip audioEvent;
        public float delayToAppear;
        public float duration;
        public AudioClip audioAnnounce;
        public ENUM_Event eventType;
    }

    //References
    [SerializeField] private AudioSource[] m_audioSourceRef = new AudioSource[(int)ENUM_EventProperty.NB_ENTRIES];
    [SerializeField] private Image m_noiseImgRef;
    [SerializeField] private Transform m_zmbRef;
    [SerializeField] private Image m_fadingImgRef;
    [SerializeField] private Player m_playerRef;
    //Properties
    [SerializeField] private float mFlt_eventFrequency;
    [SerializeField] private float mFlt_evFreqVariance;
    [SerializeField] private STRUCT_Event[] m_events = new STRUCT_Event[(int)ENUM_Event.NB_ENTRIES];
    private float mFlt_eventCoolDown;
    private float mFlt_permissionDelay;
    private bool mB_eventHappening;
    private bool mB_happened;
    private ENUM_EventProperty m_eventState;
    private ENUM_Event m_currentEventType;

    // Use this for initialization
    void Start ()
    {
        m_eventState = ENUM_EventProperty.Announce;
        mFlt_eventCoolDown = mFlt_eventFrequency * 4; //The first event takes 4 times the time of an usual event to happen
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(mB_eventHappening)//If an event is currently happening
        {
            switch (m_eventState)//Depending on its current state (announcement or the event itself)
            {
                case ENUM_EventProperty.Announce://Announcement
                    if (!m_audioSourceRef[(int)ENUM_EventProperty.Announce].isPlaying)
                    {
                        m_currentEventType = GetRandomEvent();//Starts with a random event
                        Debug.Log("Event happening : " + m_currentEventType);
                        m_audioSourceRef[(int)ENUM_EventProperty.Announce].clip = GetAudioEventClip(ENUM_EventProperty.Announce, false, m_currentEventType);//Gets the announcement audio event clip
                        m_audioSourceRef[(int)ENUM_EventProperty.Announce].Play();
                        Invoke("NextEventTypeState", m_events[(int)m_currentEventType].delayToAppear);//Calls an Invoke method updating the current event state depending on the delay to appear value
                        Debug.Log("The event will appear in " + m_events[(int)m_currentEventType].delayToAppear);
                        mFlt_permissionDelay = m_events[(int)m_currentEventType].delayToAppear;
                    }
                    if(mFlt_permissionDelay > 0.0f)//Updating from red to green to prevent the player of the permission
                    {
                        mFlt_permissionDelay -= Time.deltaTime;
                        m_noiseImgRef.color = new Color((mFlt_permissionDelay / m_events[(int)m_currentEventType].delayToAppear), 1-(mFlt_permissionDelay / m_events[(int)m_currentEventType].delayToAppear), 0f, 1f);
                    }
                    break;
                case ENUM_EventProperty.Event://Event
                    if (!m_audioSourceRef[(int)ENUM_EventProperty.Event].isPlaying && !mB_happened)
                    {
                        m_zmbRef.GetComponent<AudioSource>().Play();
                        m_zmbRef.GetComponent<Animator>().SetBool("Shake", true);
                        Debug.Log("True event happening");
                        mB_happened = true;
                        mFlt_permissionDelay = m_events[(int)m_currentEventType].duration;

                        m_audioSourceRef[(int)ENUM_EventProperty.Event].clip = GetAudioEventClip(ENUM_EventProperty.Event, false, m_currentEventType);//Gets the "event" audio event clip
                        m_audioSourceRef[(int)ENUM_EventProperty.Event].Play();
                        if((m_audioSourceRef[(int)ENUM_EventProperty.Announce].clip.length - m_audioSourceRef[(int)ENUM_EventProperty.Announce].time) > 0.0f &&
                            (m_audioSourceRef[(int)ENUM_EventProperty.Announce].clip.length - m_audioSourceRef[(int)ENUM_EventProperty.Announce].time) >
                            (m_audioSourceRef[(int)ENUM_EventProperty.Event].clip.length - m_audioSourceRef[(int)ENUM_EventProperty.Event].time))
                        {
                            Invoke("ResetbHappening", m_audioSourceRef[(int)ENUM_EventProperty.Announce].clip.length - m_audioSourceRef[(int)ENUM_EventProperty.Announce].time);
                            Debug.Log("Announcement not over, new event in " + (m_audioSourceRef[(int)ENUM_EventProperty.Announce].clip.length - m_audioSourceRef[(int)ENUM_EventProperty.Announce].time));
                        }
                        else
                        {
                            Invoke("ResetbHappening", m_audioSourceRef[(int)ENUM_EventProperty.Event].clip.length - m_audioSourceRef[(int)ENUM_EventProperty.Event].time);
                            Debug.Log("Announcement over, new event in " + (m_audioSourceRef[(int)ENUM_EventProperty.Event].clip.length - m_audioSourceRef[(int)ENUM_EventProperty.Event].time));
                        }
                        //Invoke the reset method with a delay of the time remaining on the Announcement audio clip
                    }
                    if(mFlt_permissionDelay > 0.0f)
                    {
                        mFlt_permissionDelay -= Time.deltaTime;
                        if(mFlt_permissionDelay < m_events[(int)m_currentEventType].duration)
                            m_noiseImgRef.color = new Color(1-(mFlt_permissionDelay / m_events[(int)m_currentEventType].duration), (mFlt_permissionDelay / m_events[(int)m_currentEventType].duration), 0f, 1f);
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (mFlt_eventCoolDown > 0.0f)
            {
                mFlt_eventCoolDown -= Time.deltaTime;
            }else
            {
                Debug.Log("NEW EVENT");
                //Resetting values
                mB_happened = false;
                mFlt_eventCoolDown = mFlt_eventFrequency;
                m_eventState = ENUM_EventProperty.Announce;
                mB_eventHappening = true;
            }
        }
	}

    /*
     * Returns an audio clip considering the parameters :
     * 
     * property (is it the event itself or his announcement)
     * random (should the event be selected randomly)
     * eventType (if random is false, it defines which event is wanted)
    */
    private AudioClip GetAudioEventClip(ENUM_EventProperty property, bool random, ENUM_Event eventType)
    {
        if(random)
        {
            switch (property)
            {
                case ENUM_EventProperty.Announce:
                    return m_events[(int)GetRandomEvent()].audioAnnounce;
                case ENUM_EventProperty.Event:
                    return m_events[(int)GetRandomEvent()].audioEvent;
                default:
                    return null;
            }
        }else
        {
            switch (property)
            {
                case ENUM_EventProperty.Announce:
                    return m_events[(int)eventType].audioAnnounce;
                case ENUM_EventProperty.Event:
                    return m_events[(int)eventType].audioEvent;
                default:
                    return null;
            }
        }
    }

    //Returns a random ENUM_Event value
    private ENUM_Event GetRandomEvent()
    {
        return (ENUM_Event)Random.Range(0, ((int)ENUM_Event.NB_ENTRIES));
    }

    private void NextEventTypeState()
    {
        if (m_eventState == ENUM_EventProperty.Announce)
            m_eventState = ENUM_EventProperty.Event;
    }

    private void ResetbHappening()
    {
        mB_eventHappening = false;
        m_zmbRef.GetComponent<Animator>().SetBool("Shake", false);
        m_zmbRef.GetComponent<Zombie>().StopCollision();
    }

    public void Restart()
    {
        FadeScreen();
        m_playerRef.Respawn();
    }

    public void FadeScreen()
    {
        m_fadingImgRef.GetComponent<Animator>().SetTrigger("fade");
    }
}
