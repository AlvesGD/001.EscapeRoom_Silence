using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{

    //References
    [SerializeField] private Player_Proto_001 m_playerRef;
    //Properties
    private bool mB_beingUsed;
    [SerializeField] private float mFlt_noiseImpact;
    [SerializeField] private float mFlt_speedImpact;
    [SerializeField] private bool mB_blockZMovement;
    [SerializeField] private bool mB_blockYMovement;
    [SerializeField] private bool mB_blockXMovement;
    private Vector3 mV_savedPosition;

	// Use this for initialization
	void Start ()
    {
        mV_savedPosition = this.transform.position;
    }

    public void Interact(Player_Proto_001 player)
    {
        mB_beingUsed = true;
        this.GetComponent<BoxCollider>().enabled = false;
        this.transform.SetParent(player.transform);
        // this.transform.position = mV_savedPosition;
        //this.GetComponent<FixedJoint>().connectedBody = player.GetComponent<Rigidbody>();
    }

    public void Release(Player_Proto_001 player)
    {
        mB_beingUsed = false;
        this.transform.parent = null;
        this.GetComponent<BoxCollider>().enabled = true;
        //this.GetComponent<FixedJoint>().connectedBody = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        /*/
        if (other.CompareTag("Player"))
        {
            MeshRenderer rend = this.GetComponent<MeshRenderer>();

            rend.material.SetColor("_Color", Color.red);
        }
        */
    }

    private void OnTriggerExit(Collider other)
    {
        /*
        if (other.CompareTag("Player"))
        {
            MeshRenderer rend = this.GetComponent<MeshRenderer>();

            rend.material.SetColor("_Color", Color.white);
        }
        */
    }

    //ACCESSORS
    

    public float GetNoiseImpact()
    {
        return mFlt_noiseImpact;
    }

    public float GetSpeedImpact()
    {
        return mFlt_speedImpact;
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
}
