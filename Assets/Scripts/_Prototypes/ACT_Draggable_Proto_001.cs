using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACT_Draggable_Proto_001 : MonoBehaviour
{

    //References
    [SerializeField] private Player_Proto_001 m_playerRef;
    //Properties
    private bool mB_dragged;
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

    public void Drag(Player_Proto_001 player)
    {
        mB_dragged = true;
        this.GetComponent<BoxCollider>().enabled = false;
        this.transform.SetParent(player.transform);
        // this.transform.position = mV_savedPosition;
        //this.GetComponent<FixedJoint>().connectedBody = player.GetComponent<Rigidbody>();
    }

    public void Drop(Player_Proto_001 player)
    {
        mB_dragged = false;
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
        return mB_dragged;
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
