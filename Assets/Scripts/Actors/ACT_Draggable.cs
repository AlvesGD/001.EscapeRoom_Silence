using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACT_Draggable : MonoBehaviour
{

    //References
    [SerializeField] private Player m_playerRef;
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
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void Drag(Player player)
    {
        mB_dragged = true;
        this.GetComponent<BoxCollider>().enabled = false;
        this.transform.SetParent(player.transform);
        //this.transform.position = mV_savedPosition;
    }

    public void Drop()
    {
        mB_dragged = false;
        this.transform.parent = null;
        this.GetComponent<BoxCollider>().enabled = true;
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
