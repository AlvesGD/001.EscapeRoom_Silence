using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACT_Vase : ACT_Key
{
    [SerializeField] private Transform m_dripRef;
    [SerializeField] private float mFlt_marginDelay;
    private AudioSource m_audioSourceRef;

	// Use this for initialization
	void Start ()
    {
        m_audioSourceRef = this.GetComponents<AudioSource>()[2];	
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnParticleCollision(GameObject other)
    {
        if(other.CompareTag("Dropped"))
        {
            if(!m_audioSourceRef.isPlaying)
                m_audioSourceRef.Play();
            if(other.GetComponent<ACT_Drip>() != null)
            {
                if(other.GetComponent<ACT_Drip>() == m_dripRef.GetComponent<ACT_Drip>())//If the vase is currently being hit by the right linked particle
                {
                    if (!m_doorRef.IsOpen())
                    {
                        IntoDoor();
                        //Invoke("OutFromDoor", mFlt_marginDelay);
                    }
                }
            }
        }
    }
}
