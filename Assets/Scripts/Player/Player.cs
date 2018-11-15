using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public enum ENUM_PlayerActions
    {
        jump, land, hit, NB_ENTRIES
    }
    //Constants
    //private const float JUMP_NOISEIMPACT_DURATION

    //References
    [SerializeField] private GM_Manager m_gmRef;
    [SerializeField] private Image mImg_noiseLevelBar;
    private PLAY_LookAround m_lookAroundRef;    //Look around scripts references
    [SerializeField] private PLAY_LookAround m_camLookAroundRef;
    private ACT_Draggable m_draggableObject;
    private AudioSource m_audioActionRef;
    //Properties
    //Convert these SerializeFields to constants
    [SerializeField] private float m_spawnDelay;
    [SerializeField] private float m_speed;
    [SerializeField] private float m_spdNoiseImpact;
    [SerializeField] private float m_jumpForce;
    [SerializeField] private float m_jumpNoiseImpact;
    [SerializeField] private float m_jmpNoiseImpDuration;
    [SerializeField] private float m_timeToLand;
    [SerializeField] private float m_landNoiseImpact;
    [SerializeField] private AudioClip[] m_audioSamples = new AudioClip[(int)ENUM_PlayerActions.NB_ENTRIES];

    private Vector3 m_spawnPosition;
    private Quaternion m_spawnRotation;

    //Triggers
    private bool mB_jumpTrig;
    private bool mB_deathTrig;

    private bool mB_isGrounded;
    private bool mB_blocked;
    private bool mB_interacting;
    private float mFlt_jumpCooling;
    private float mFlt_landCooling;
    private float m_noiseLevel;
    private float m_currentSpeed;

    private void Awake()
    {
        m_spawnPosition = this.transform.position;
        m_spawnRotation = this.transform.rotation;
        mB_jumpTrig = false;
        mB_deathTrig = false;
        mB_blocked = false;
        mB_isGrounded = true;
        m_audioActionRef = this.GetComponent<AudioSource>();
        m_currentSpeed = 0.0f;
        m_lookAroundRef = this.GetComponent<PLAY_LookAround>();
        Cursor.visible = false;
    }
    // Update is called once per frame
    void Update ()
    {
        Action();
        Movement();
        UpdateNoiseLevel();
	}

    private void Movement()
    {
        Vector3 movementForce = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

        if(!mB_blocked)
        {
            //Adds the speed impact of the dragged object
            if (mB_interacting)
            {
                if (m_draggableObject.X_Blocked())
                    movementForce.x = 0f;
                if (m_draggableObject.Z_Blocked())
                    movementForce.z = 0f;
                movementForce *= Time.deltaTime * (m_speed / m_draggableObject.GetSpeedImpact());
            }
            movementForce *= Time.deltaTime * m_speed;
            this.transform.Translate(movementForce);
            UpdateCurrentSpeed();
        }
    }

    private void Action()
    {
        if(Input.GetButtonDown("Fire1") && mB_isGrounded)//Player pressing left button and is not jumping
        {
            if (m_draggableObject != null)//If he is currently overlapping a draggable object
            {
                if(mB_interacting)//Already dragging an object
                {
                    mB_interacting = false;
                    m_draggableObject.Drop();//Drops the object
                    if (m_lookAroundRef != null && m_camLookAroundRef != null)//Frees looking axis rotations
                    {
                        m_lookAroundRef.FreeAxis(PLAY_LookAround.ENUM_RotationAxis.X);
                        m_camLookAroundRef.FreeAxis(PLAY_LookAround.ENUM_RotationAxis.Y);
                    }
                }
                else//Empty handed
                {
                    mB_interacting = true;
                    m_draggableObject.Drag(this.GetComponent<Player>());//Drags the object
                    if(m_lookAroundRef != null && m_camLookAroundRef != null) //Blocks the looking axis rotations
                    {
                        m_lookAroundRef.BlockAxis(PLAY_LookAround.ENUM_RotationAxis.X);
                        m_camLookAroundRef.BlockAxis(PLAY_LookAround.ENUM_RotationAxis.Y);
                    }
                }
            }
        }
        if(Input.GetKeyDown("space") && mB_isGrounded && (!mB_interacting || !m_draggableObject.Y_Blocked()))
        {
            this.GetComponent<Rigidbody>().AddForce(Vector3.up * m_jumpForce);
            mB_isGrounded = false;
            mB_jumpTrig = false;//A generic boolean used to detect the exact moment of the jump (trigger) later in "UpdateNoiseLevel()"
            mFlt_jumpCooling = m_jmpNoiseImpDuration;
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
            if (m_currentSpeed > 0.0f && mB_isGrounded)
                m_noiseLevel = m_currentSpeed * (m_spdNoiseImpact + ((mB_interacting) ? m_draggableObject.GetNoiseImpact() : 0.0f));
            else
                m_noiseLevel = 0.0f;
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
        }else if(mFlt_landCooling > 0.0f)
        {
            mFlt_landCooling -= Time.deltaTime;
            m_noiseLevel = m_landNoiseImpact;
        }else
        {
            mB_blocked = false;
        }
        //Keeps m_noiseLevel between 0 and 1
        m_noiseLevel = Mathf.Clamp(m_noiseLevel, 0.0f, 1.0f);
        if(m_noiseLevel == 1.0f && !mB_deathTrig)
        {
            mB_deathTrig = true;
            m_gmRef.Restart();
        }

        if (mB_interacting)
        {
            if(m_draggableObject != null)
            {
                if (m_currentSpeed > 0.0f)
                {
                    m_draggableObject.GetComponent<AudioSource>().volume = m_noiseLevel;
                    if (!m_draggableObject.GetComponent<AudioSource>().isPlaying)
                    {
                        m_draggableObject.GetComponent<AudioSource>().Play();
                    }
                    if (m_currentSpeed > 0.5f && !m_draggableObject.GetComponents<AudioSource>()[1].isPlaying)
                    {
                        m_draggableObject.GetComponents<AudioSource>()[1].Play();
                    }
                }
                else
                {
                    if (m_draggableObject.GetComponent<AudioSource>().isPlaying)
                    {
                        m_draggableObject.GetComponent<AudioSource>().Stop();
                    }
                    if (m_draggableObject.GetComponents<AudioSource>()[1].isPlaying)
                    {
                        m_draggableObject.GetComponents<AudioSource>()[1].Stop();
                    }

                }

            }
        }else
        {
            if(m_draggableObject != null)
            {
                if (m_draggableObject.GetComponent<AudioSource>().isPlaying)
                {
                    m_draggableObject.GetComponent<AudioSource>().Stop();
                }
                if (m_draggableObject.GetComponents<AudioSource>()[1].isPlaying)
                {
                    m_draggableObject.GetComponents<AudioSource>()[1].Stop();
                }
            }
        }
        if (mImg_noiseLevelBar != null)
        {
            mImg_noiseLevelBar.fillAmount = m_noiseLevel;
            mImg_noiseLevelBar.color = new Color(m_noiseLevel, 0f, 0f, 0.5f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Walkable"))
        {
            mB_isGrounded = true;
            Debug.Log("Collided with : " + collision.collider.name);
            if(!m_audioActionRef.isPlaying && !mB_blocked)
            {
                m_audioActionRef.clip = m_audioSamples[(int)ENUM_PlayerActions.land];
                m_audioActionRef.Play();
                mB_blocked = true;
                mFlt_landCooling = m_timeToLand;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!mB_interacting)
        {
            if (other.GetComponent<ACT_Draggable>() != null)
            {
                m_draggableObject = other.GetComponent<ACT_Draggable>();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!mB_interacting && m_draggableObject != null && m_draggableObject.gameObject == other.gameObject)
        {
            m_draggableObject = null;
        }
    }

    public void Respawn()
    {
        Invoke("Spawn", m_spawnDelay);
    }

    private void Spawn()
    {
        if (mB_interacting && m_draggableObject != null)
            m_draggableObject.transform.SetParent(null);
        mB_deathTrig = false;
        mB_blocked = false;
        mB_interacting = false;
        m_draggableObject = null;
        m_lookAroundRef.FreeAllAxis();
        m_camLookAroundRef.FreeAllAxis();
        this.transform.position = m_spawnPosition;
        this.transform.rotation = m_spawnRotation;
    }
}
