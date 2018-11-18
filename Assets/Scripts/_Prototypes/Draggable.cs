using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : Interactable
{
    [SerializeField] private float mFlt_speedImpact;

    public float GetSpeedImpact()
    {
        return mFlt_speedImpact;
    }

	public override void Interact()
	{
		base.Interact();
        this.GetComponent<BoxCollider>().enabled = false;
        this.transform.SetParent(m_playerRef.transform);
	}

	public override void Release()
	{
		base.Release();
        this.transform.parent = null;
        this.GetComponent<BoxCollider>().enabled = true;
	}
}
