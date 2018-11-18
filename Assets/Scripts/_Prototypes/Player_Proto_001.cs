using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ENUM_PlayerActions
{
    land, hit, NB_ENTRIES
}

public enum ENUM_PlayerStates
{
    none, interacting, climbing, falling, NB_ENTRIES
}

public enum ENUM_Object
{
    climbable, draggable, NB_ENTRIES
}

[RequireComponent(typeof(AudioPlayer))]
public class Player_Proto_001 : MonoBehaviour
{
        //CONSTS
    //private const float JUMP_NOISEIMPACT_DURATION
    public const int NB_AUDIO_SOURCES = 7;
    public const int NB_FTSTEP_CLIPS = 4;

        //REFERENCES
    //[SerializeField] private GM_Manager m_gmRef;
            //HUD
    [SerializeField] private Image mImg_noiseLevelBar;
    [SerializeField] private Transform m_hintParentRef;
    [SerializeField] private Transform m_noiseLvlBcgRef;
            //SCRIPTS
    private PLAY_LookAround m_lookAroundRef;    //Look around scripts references
    [SerializeField] private PLAY_LookAround m_camLookAroundRef;
            //OBJECTS
    private Transform m_aimedObject;
    private Draggable m_draggedObject;
    private Climbable m_climbedObject;
            //AUDIO
    private List<AudioSource> mAudio_refs;
    private AudioPlayer m_audioPlayerRef;
        //PROPERTIES
    //Convert these SerializeFields to constants
    [SerializeField] private float m_spawnDelay;
    [SerializeField] private float m_speed;
    [SerializeField] private float m_spdNoiseImpact;
    [SerializeField] private AudioClip[] m_audioSamples = new AudioClip[NB_AUDIO_SOURCES];
    private List<GameObject> m_overlappedActors;

    private Vector3 m_spawnPosition;
    private Quaternion m_spawnRotation;

        //TRIGGERS
    //private bool mB_jumpTrig;
    private bool mB_deathTrig;

    //private bool mB_isGrounded;
    private ENUM_PlayerStates m_playerState;
    private Vector3 m_playerStartClimbingPos;
    private bool mB_blocked;
    private float mFlt_landCooling;
    private float m_noiseLevel;
    private float m_currentSpeed;
    private bool m_gamepad;

    private void Awake()
    {
        Initialization();

        if (Input.GetJoystickNames().Length == 0)
            m_gamepad = false;
        else
            m_gamepad = true;
    }

    private void Initialization()
    {
        for(int i = 0 ; i < NB_AUDIO_SOURCES ; i++)
        {
            AudioSource temp = this.gameObject.AddComponent<AudioSource>();
            temp.spatialBlend = 0.8f;
            temp.minDistance = 0.001f;
            temp.maxDistance = 0.01f;
        }
        mAudio_refs = new List<AudioSource>(this.GetComponents<AudioSource>());
        m_overlappedActors = new List<GameObject>();
        m_lookAroundRef = this.GetComponent<PLAY_LookAround>();
        m_audioPlayerRef = this.GetComponent<AudioPlayer>();

        m_draggedObject = null;
        m_climbedObject = null;
        m_aimedObject = null;

        mB_deathTrig = false;
        mB_blocked = false;
        Cursor.visible = false;

        m_currentSpeed = 0.0f;
        m_playerState = ENUM_PlayerStates.none;

        m_lookAroundRef.FreeAllAxis();
        m_camLookAroundRef.FreeAllAxis();

        m_spawnPosition = this.transform.position;
        m_spawnRotation = this.transform.rotation;
    }
    // Update is called once per frame
    void Update ()
    {
        //Camera.main.transform.localEulerAngles = new Vector3( Mathf.Clamp( Camera.main.transform.localEulerAngles.x, 60, 360), 0, 0);
        AimingRay();
        Action();
        Movement();
        UpdateNoiseLevel();
	}

    private void AimingRay()
    {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;
        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if(m_playerState == ENUM_PlayerStates.none)
        {
            if (Physics.Raycast(m_camLookAroundRef.transform.position, m_camLookAroundRef.transform.forward * 3000, out hit, Mathf.Infinity, layerMask))
            {
                    m_aimedObject = hit.collider.gameObject.transform;
                    if(ObjectOverlapped(m_aimedObject.gameObject))
                    {
                        if(FirstObjInOverlapped(ENUM_Object.climbable) != null)
                        {
                            m_climbedObject = m_aimedObject.GetComponent<Climbable>();
                            m_hintParentRef.gameObject.SetActive(true);
                            if(!m_hintParentRef.GetChild(0).GetChild(1).gameObject.activeInHierarchy)
                                m_hintParentRef.GetChild(0).GetChild(1).gameObject.SetActive(true);
                        }else
                        {
                            Debug.Log("Not climbable");
                            m_hintParentRef.gameObject.SetActive(false);
                            if(m_hintParentRef.GetChild(0).GetChild(1).gameObject.activeInHierarchy)
                                m_hintParentRef.GetChild(0).GetChild(1).gameObject.SetActive(false);
                            m_climbedObject = null;
                        }
                        if(FirstObjInOverlapped(ENUM_Object.draggable) != null)
                        {
                            m_hintParentRef.gameObject.SetActive(true);
                            m_draggedObject = m_aimedObject.GetComponent<Draggable>();
                            if(!m_hintParentRef.GetChild(0).GetChild(0).gameObject.activeInHierarchy)
                                m_hintParentRef.GetChild(0).GetChild(0).gameObject.SetActive(true);

                        }else
                        {
                            Debug.Log("Not climbable");
                            m_hintParentRef.gameObject.SetActive(false);
                            if(m_hintParentRef.GetChild(0).GetChild(0).gameObject.activeInHierarchy)
                                m_hintParentRef.GetChild(0).GetChild(0).gameObject.SetActive(false);
                            m_draggedObject = null;
                        }
                    }else
                    {
                        if(!m_hintParentRef.GetChild(0).GetChild(1).gameObject.activeInHierarchy)
                            m_hintParentRef.GetChild(0).GetChild(1).gameObject.SetActive(false);
                        if(!m_hintParentRef.GetChild(0).GetChild(0).gameObject.activeInHierarchy)
                            m_hintParentRef.GetChild(0).GetChild(0).gameObject.SetActive(false);
                        m_hintParentRef.gameObject.SetActive(false);
                        m_climbedObject = null;
                        m_draggedObject = null;
                    }
            }else
            {
                m_aimedObject = null;
            }
        }
    }

    private void Movement()
    {
        Vector3 movementForce = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

        if(!mB_blocked)
        {
            //Adds the speed impact of the dragged object
            if (m_playerState == ENUM_PlayerStates.interacting)
            {
                if (m_draggedObject.X_Blocked())
                    movementForce.x = 0f;
                if (m_draggedObject.Z_Blocked())
                    movementForce.z = 0f;
                movementForce *= Time.deltaTime * (m_speed / m_draggedObject.GetSpeedImpact());
            }
            movementForce *= Time.deltaTime * m_speed;
            this.transform.Translate(movementForce);
            UpdateCurrentSpeed();
        }
    }
    
    //If any absolute input is positive, then m_currentSpeed is defined as the most important absolute input, otherwise m_currentSpeed is set to 0
    private void UpdateCurrentSpeed()
    {
        //FIX ME : If the player is blocked by the object he's dragging (X blocked for example) he mustn't generate useless noise
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.0f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.0f)
            m_currentSpeed = (Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Abs(Input.GetAxis("Vertical"))) ? Mathf.Abs(Input.GetAxis("Horizontal")) : Mathf.Abs(Input.GetAxis("Vertical"));
        else
            m_currentSpeed = 0.0f;
    }

    private void Action()
    {
        if(Input.GetAxis("Gamepad_Trigger") < 0f)//Player pressing right trigger
        {
            if (m_draggedObject != null && m_playerState == ENUM_PlayerStates.none)
            {
                m_playerState = ENUM_PlayerStates.interacting;
                m_draggedObject.Interact();//Drags the object
                if(m_lookAroundRef != null && m_camLookAroundRef != null) //Blocks the looking axis rotations
                {
                    m_lookAroundRef.BlockAxis(PLAY_LookAround.ENUM_RotationAxis.Y);
                    m_camLookAroundRef.BlockAxis(PLAY_LookAround.ENUM_RotationAxis.X);
                }
            }
        }else//Player not pressing the right trigger
        {
            if(Input.GetAxis("Gamepad_Trigger") > 0f)//Player pressing right trigger
            {
                if(m_climbedObject != null)
                {
                    if (m_playerState == ENUM_PlayerStates.none)//If the climbing phase has just started.
                    {
                        m_playerState = ENUM_PlayerStates.climbing;
                        mB_blocked = true;
                        this.GetComponent<Rigidbody>().useGravity = false;
                        m_playerStartClimbingPos = this.transform.position;
                        m_climbedObject.Interact();
                    }
                    if(m_playerState == ENUM_PlayerStates.climbing)
                    {
                        //If the player hasn't climbed the summit of the object.
                        if(this.transform.position.y < m_climbedObject.transform.GetChild(0).position.y + this.GetComponent<CapsuleCollider>().height / 2)
                        {
                            float input = Input.GetAxis("Gamepad_Trigger");
                            float speed = Vector3.Distance(m_playerStartClimbingPos, m_climbedObject.transform.GetChild(0).position) / (m_climbedObject.GetClimbDuration() * 100);
                        
                            this.transform.position += new Vector3(0, input*speed, 0);
                        }else
                        {
                            m_playerState = ENUM_PlayerStates.none;
                            mB_blocked = false;
                            m_climbedObject.Release();
                            this.GetComponent<Rigidbody>().useGravity = true;
                            m_overlappedActors.Remove(m_climbedObject.gameObject);
                            m_climbedObject = null;
                        }
                    }
                }
                //If he is currently overlapping climbable object and not already performing an action
            }else if(m_playerState == ENUM_PlayerStates.climbing)
            {
                m_playerState = ENUM_PlayerStates.none;
                m_climbedObject.Release();
                mB_blocked = false;
                this.transform.position = m_playerStartClimbingPos;
                this.GetComponent<Rigidbody>().useGravity = true;
            }
            if (m_playerState == ENUM_PlayerStates.interacting)//Already dragging an object
            {
                m_playerState = ENUM_PlayerStates.none;
                m_draggedObject.Release();//Drops the object
                if (m_lookAroundRef != null && m_camLookAroundRef != null)//Frees looking axis rotations
                {
                    m_lookAroundRef.FreeAxis(PLAY_LookAround.ENUM_RotationAxis.Y);
                    m_camLookAroundRef.FreeAxis(PLAY_LookAround.ENUM_RotationAxis.X);
                }
                m_draggedObject = null;
            }
        }
    }

    private void UpdateNoiseLevel()
    {
        if(!mB_blocked)
        {
            if (m_currentSpeed > 0.0f /*&& mB_isGrounded*/)
            {
                m_noiseLevel = m_currentSpeed * (m_spdNoiseImpact + ((m_playerState == ENUM_PlayerStates.interacting) ? m_draggedObject.GetNoiseImpact() : 0.0f));   
            }
            else
                m_noiseLevel = 0.0f;
        }else
        {
            if(m_playerState == ENUM_PlayerStates.climbing)
            {
                m_noiseLevel = m_climbedObject.GetNoiseImpact() * Input.GetAxis("Gamepad_Trigger");
            }
        }
        //Keeps m_noiseLevel between 0 and 1
        m_noiseLevel = Mathf.Clamp(m_noiseLevel, 0.0f, 1.0f);
        /*
        if (m_noiseLevel == 1.0f && !mB_deathTrig)
        {
            mB_deathTrig = true;
            m_gmRef.Restart();
        }
        */
        if (mImg_noiseLevelBar != null && m_noiseLvlBcgRef != null)
        {
            mImg_noiseLevelBar.fillAmount = m_noiseLevel;
            mImg_noiseLevelBar.color = new Color(1f, 1 - m_noiseLevel, 1 - m_noiseLevel, 0.8f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Landing
    }

    private bool ObjectOverlapped(GameObject tObject)
    {
        return(m_overlappedActors.Contains(tObject));
    }

    //Returns the transform of the first (if found) object of the specified type in the overlapped actors list.
    private Transform FirstObjInOverlapped(ENUM_Object objectType)
    {
        int i = 0;

        switch(objectType)
        {
            case ENUM_Object.climbable:
                for(i = 0 ; i < m_overlappedActors.Count ; i++)
                {
                    if(m_overlappedActors[i].GetComponent<Climbable>() != null)
                        return m_overlappedActors[i].transform;
                }
                break;
            case ENUM_Object.draggable:
                for(i = 0 ; i < m_overlappedActors.Count ; i++)
                {
                    if(m_overlappedActors[i].GetComponent<Draggable>() != null)
                        return m_overlappedActors[i].transform;
                }
                break;
            default:
                break;
        }
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        m_overlappedActors.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        switch(m_playerState)
        {
            case ENUM_PlayerStates.interacting:
                if( other.gameObject == m_climbedObject && ObjectOverlapped(other.gameObject))
                    m_overlappedActors.Remove(other.gameObject);
            break;
            case ENUM_PlayerStates.climbing:
                if( other.gameObject == m_draggedObject && ObjectOverlapped(other.gameObject))
                    m_overlappedActors.Remove(other.gameObject);
            break;
            default:
                    m_overlappedActors.Remove(other.gameObject);

            break;
        }
    }

    public void Respawn()
    {
        Invoke("Spawn", m_spawnDelay);
    }

    private void Spawn()
    {
        Initialization();
        this.transform.position = m_spawnPosition;
        this.transform.rotation = m_spawnRotation;
    }

    public float InputTriggerIntens()
    {
        return Input.GetAxis("Gamepad_Trigger");
    }
}
