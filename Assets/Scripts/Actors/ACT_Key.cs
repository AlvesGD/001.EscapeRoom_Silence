using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACT_Key : MonoBehaviour
{
    //References
    [SerializeField] protected ACT_Door m_doorRef;
    //Properties
    [SerializeField] protected int mN_index;
    protected bool mB_isIn;

    protected void IntoDoor()
    {
        if(!mB_isIn)
        {
            m_doorRef.KeyIn(mN_index);
            mB_isIn = true;
        }
    }

    protected void OutFromDoor()
    {
        if(mB_isIn)
        {
            m_doorRef.KeyOut(mN_index);
            mB_isIn = false;
        }
    }

    public int GetIndex()
    {
        return mN_index;
    }
}
