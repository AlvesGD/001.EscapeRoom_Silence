using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACT_Climbable_Proto_001 : MonoBehaviour
{

    [SerializeField] private float mFlt_climbDuration;
    [SerializeField] private float mFlt_noiseImpact;
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public float GetTotalClimbDuration()
    {
        return mFlt_climbDuration;
    }

    public float GetNoiseImpact()
    {
        return mFlt_noiseImpact;
    }
}
