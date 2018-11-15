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

public class Player_Proto_001 : MonoBehaviour
{
    //Constants
    //private const float JUMP_NOISEIMPACT_DURATION

    //References
    //[SerializeField] private GM_Manager m_gmRef;
    [SerializeField] private Image mImg_noiseLevelBar;
    [SerializeField] private Transform m_hintParentRef;
    [SerializeField] private Transform m_noiseLvlBcgRef;
    private PLAY_LookAround m_lookAroundRef;    //Look around scripts references
    [SerializeField] private PLAY_LookAround m_camLookAroundRef;
    private List<ACT_Draggable_Proto_001> m_draggableObjects;
    private ACT_Draggable_Proto_001 m_draggedObject;
    private List<ACT_Climbable_Proto_001> m_climbableObjects;
    private ACT_Climbable_Proto_001 m_climbedObject;
    private Transform m_aimedObject;
    private AudioSource m_audioActionRef;
    //Properties
    //Convert these SerializeFields to constants
    [SerializeField] private float m_spawnDelay;
    [SerializeField] private float m_speed;
    [SerializeField] private float m_spdNoiseImpact;
    /*
    [SerializeField] private float m_jumpForce;
    [SerializeField] private float m_jumpNoiseImpact;
    [SerializeField] private float m_jmpNoiseImpDuration;
    [SerializeField] private float m_timeToLand;
    [SerializeField] private float m_landNoiseImpact;
    */
    [SerializeField] private AudioClip[] m_audioSamples = new AudioClip[(int)ENUM_PlayerActions.NB_ENTRIES];
    private List<GameObject> m_overlappedActors;

    private Vector3 m_spawnPosition;
    private Quaternion m_spawnRotation;

    //Triggers
    //private bool mB_jumpTrig;
    private bool mB_deathTrig;

    //private bool mB_isGrounded;
    private ENUM_PlayerStates m_playerState;
    private Vector3 m_playerStartClimbingPos;
    private float mFlt_climbPercent;
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
        m_overlappedActors = new List<GameObject>();
        m_audioActionRef = this.GetComponent<AudioSource>();
        m_lookAroundRef = this.GetComponent<PLAY_LookAround>();
        m_climbableObjects = new List<ACT_Climbable_Proto_001>();
        m_draggableObjects = new List<ACT_Draggable_Proto_001>();
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
                //Debug.DrawRay(m_camLookAroundRef.transform.position, m_camLookAroundRef.transform.forward * hit.distance, Color.yellow);
                    //If the collider is a draggable object and already included in the overlapping objects list
                    /*/
                    if (hit.collider.GetComponent<ACT_Draggable_Proto_001>() != null &&  m_draggableObjects.Contains(hit.collider.GetComponent<ACT_Draggable_Proto_001>()))
                    {
                        m_aimedObject = hit.collider.transform;//Referencing the transform of the object if it is contained by the overlapping objects list
                        m_hintParentRef.gameObject.SetActive(true);
                        m_hintParentRef.GetChild(0).GetChild(0).gameObject.SetActive(true);
                    }else
                    {
                        m_hintParentRef.GetChild(0).GetChild(0).gameObject.SetActive(false);
                    }
                    if(hit.collider.GetComponent<ACT_Climbable_Proto_001>() != null &&  m_climbableObjects.Contains(hit.collider.GetComponent<ACT_Climbable_Proto_001>()))
                    {
                        m_aimedObject = hit.collider.transform;
                        m_hintParentRef.gameObject.SetActive(true);
                        m_hintParentRef.GetChild(0).GetChild(1).gameObject.SetActive(true);
                        //Otherwise it is set to invisible
                    }else
                    {
                        m_hintParentRef.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    }
                    */
                    m_aimedObject = hit.collider.gameObject.transform;
                    //DEGUEULASSE
            }else
            {
                m_aimedObject = null;
                //Debug.DrawRay(m_camLookAroundRef.transform.position, m_camLookAroundRef.transform.forward * 3000, Color.white);
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

    private void Action()
    {

        if(Input.GetAxis("Gamepad_Trigger") < 0f)//Player pressing right trigger
        {
            if(m_aimedObject != null)//If the player is aiming at a draggable object
            {
                if(m_aimedObject.GetComponent<ACT_Draggable_Proto_001>() != null)
                {
                    m_draggedObject = m_aimedObject.GetComponent<ACT_Draggable_Proto_001>();
                }
            }
            if (m_draggedObject != null && m_playerState == ENUM_PlayerStates.none)
            {
                m_playerState = ENUM_PlayerStates.interacting;
                m_draggedObject.Drag(this.GetComponent<Player_Proto_001>());//Drags the object
                if(m_lookAroundRef != null && m_camLookAroundRef != null) //Blocks the looking axis rotations
                {
                    m_lookAroundRef.BlockAxis(PLAY_LookAround.ENUM_RotationAxis.Y);
                    m_camLookAroundRef.BlockAxis(PLAY_LookAround.ENUM_RotationAxis.X);
                }
            }
        }else//Player not pressing the right trigger
        {
            if(Input.GetAxis("Gamepad_Trigger") > 0f)//Player pressing the left trigger
            {
                if(m_aimedObject != null)
                {
                    if(m_aimedObject.GetComponent<ACT_Climbable_Proto_001>() != null)//If the player is aiming at a climbable object
                    {
                        m_climbedObject = m_aimedObject.GetComponent<ACT_Climbable_Proto_001>();
                    }
                }
                if(m_climbedObject != null)
                {
                    if (m_playerState == ENUM_PlayerStates.none && !m_climbedObject.IsClimbed())
                    {
                        m_playerState = ENUM_PlayerStates.climbing;
                        mB_blocked = true;
                        
                        this.GetComponent<Rigidbody>().useGravity = false;
                        m_playerStartClimbingPos = this.transform.position;
                        mFlt_climbPercent = m_climbedObject.GetTotalClimbDuration();
                    }
                    if(this.transform.position.y > m_climbedObject.transform.GetChild(0).position.y + this.GetComponent<CapsuleCollider>().height / 2)//Player has climbed the object
                    {
                        m_playerState = ENUM_PlayerStates.none;
                        mB_blocked = false;
                        this.GetComponent<Rigidbody>().useGravity = true;
                        if(m_climbedObject != null)
                            m_climbedObject.SetIsClimbed(true);
                        m_climbedObject = null;
                    }//IF the object has not just been already climbed, it avoids to chain the climbs between different climbable objects
                    else if(m_climbedObject != null && !m_climbedObject.IsClimbed())
                    {
                        float input = Input.GetAxis("Gamepad_Trigger");
                        float speed = Vector3.Distance(m_playerStartClimbingPos, m_climbedObject.transform.GetChild(0).position) / m_climbedObject.GetTotalClimbDuration();
                    
                        this.transform.position += new Vector3(0, input*speed, 0);
                    }
                }
                //If he is currently overlapping climbable object and not already performing an action
                
            }else if(m_playerState == ENUM_PlayerStates.climbing)
            {
                m_playerState = ENUM_PlayerStates.none;
                mB_blocked = false;
                this.transform.position = m_playerStartClimbingPos;
                this.GetComponent<Rigidbody>().useGravity = true;
                m_climbedObject.SetIsClimbed(false);
            }

            if (m_playerState == ENUM_PlayerStates.interacting)//Already dragging an object
            {
                m_playerState = ENUM_PlayerStates.none;
                m_draggedObject.Drop(this);//Drops the object
                if (m_lookAroundRef != null && m_camLookAroundRef != null)//Frees looking axis rotations
                {
                    m_lookAroundRef.FreeAxis(PLAY_LookAround.ENUM_RotationAxis.Y);
                    m_camLookAroundRef.FreeAxis(PLAY_LookAround.ENUM_RotationAxis.X);
                }
                m_draggedObject = null;
            }
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

    private void UpdateNoiseLevel()
    {
        if(!mB_blocked)
        {
            if (m_currentSpeed > 0.0f /*&& mB_isGrounded*/)
                m_noiseLevel = m_currentSpeed * (m_spdNoiseImpact + ((m_playerState == ENUM_PlayerStates.interacting) ? m_draggedObject.GetNoiseImpact() : 0.0f));
            else
                m_noiseLevel = 0.0f;
            /*
            if (mFlt_jumpCooling > 0.0f && !mB_isGrounded)
            {
                //Plays jump sounds here
                if (!m_audioActionRef.isPlaying && !mB_jumpTrig)
                {
                    mB_jumpTrig = true;
                    m_audioActionRef.clip = m_audioSamples[(int)ENUM_PlayerActions.jump];
                    m_audioActionRef.Play();
                }
                //Instantly disable the jump trigger boolean
                mFlt_jumpCooling -= Time.deltaTime;
                m_noiseLevel += m_jumpNoiseImpact;
            }
            */
        }/*else if(mFlt_landCooling > 0.0f)
        {
            mFlt_landCooling -= Time.deltaTime;
            m_noiseLevel = m_landNoiseImpact;
        }else
        {
            mB_blocked = false;
        }*/
        //Keeps m_noiseLevel between 0 and 1
        m_noiseLevel = Mathf.Clamp(m_noiseLevel, 0.0f, 1.0f);
        /*
        if (m_noiseLevel == 1.0f && !mB_deathTrig)
        {
            mB_deathTrig = true;
            m_gmRef.Restart();
        }
        */

        if (m_playerState == ENUM_PlayerStates.interacting)
        {
            if(m_draggedObject != null)
            {
                if (m_currentSpeed > 0.0f)
                {
                    m_draggedObject.GetComponent<AudioSource>().volume = m_noiseLevel;
                    if (!m_draggedObject.GetComponent<AudioSource>().isPlaying)
                    {
                        m_draggedObject.GetComponent<AudioSource>().Play();
                    }
                    if (m_currentSpeed > 0.5f)
                    {
                        if (!m_draggedObject.GetComponents<AudioSource>()[1].isPlaying)
                        {
                            m_draggedObject.GetComponents<AudioSource>()[1].Play();
                        }
                    }else
                    {
                        m_draggedObject.GetComponents<AudioSource>()[1].Pause();
                    }
                }
                else
                {
                    if (m_draggedObject.GetComponent<AudioSource>().isPlaying)
                    {
                        m_draggedObject.GetComponent<AudioSource>().Stop();
                    }
                    if (m_draggedObject.GetComponents<AudioSource>()[1].isPlaying)
                    {
                        m_draggedObject.GetComponents<AudioSource>()[1].Stop();
                    }

                }

            }
        }else
        {
            if(m_playerState == ENUM_PlayerStates.climbing)
            {
                if(m_climbedObject != null)
                {
                    m_noiseLevel = Input.GetAxis("Gamepad_Trigger") * m_climbedObject.GetNoiseImpact();
                }
            }
            if (m_draggedObject != null)
            {
                if (m_draggedObject.GetComponent<AudioSource>().isPlaying)
                {
                    m_draggedObject.GetComponent<AudioSource>().Stop();
                }
                if (m_draggedObject.GetComponents<AudioSource>()[1].isPlaying)
                {
                    m_draggedObject.GetComponents<AudioSource>()[1].Stop();
                }
            }
        }
        if (mImg_noiseLevelBar != null && m_noiseLvlBcgRef != null)
        {
            mImg_noiseLevelBar.fillAmount = m_noiseLevel;
            mImg_noiseLevelBar.color = new Color(1f, 1 - m_noiseLevel, 1 - m_noiseLevel, 0.8f);
            /*
            m_noiseLvlBcgRef.GetComponent<Image>().color = new Color(m_noiseLevel, 0f, 0f);
            */
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
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
                    if(m_overlappedActors[i].GetComponent<ACT_Climbable_Proto_001>() != null)
                        return m_overlappedActors[i].transform;
                }
                break;
            case ENUM_Object.draggable:
                for(i = 0 ; i < m_overlappedActors.Count ; i++)
                {
                    if(m_overlappedActors[i].GetComponent<ACT_Draggable_Proto_001>() != null)
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
        m_overlappedActors.Remove(other.gameObject);
    }

    public void Respawn()
    {
        Invoke("Spawn", m_spawnDelay);
    }

    private void Spawn()
    {
        if ((m_playerState == ENUM_PlayerStates.interacting) && m_draggableObjects[0] != null)
            m_draggableObjects[0].transform.SetParent(null);
        Initialization();
        this.transform.position = m_spawnPosition;
        this.transform.rotation = m_spawnRotation;
    }
}
