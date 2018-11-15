using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACT_Door : MonoBehaviour {

    //Properties
    [SerializeField] private bool mB_startOpen;
    [SerializeField] private int mN_nbConditions;
    [SerializeField] private bool mB_order;
    [SerializeField] private int[] mN_keysOrder;
    [SerializeField] private bool mB_punitiveOrder;
    private int mN_correctKeys;
    private bool mB_isOpen;

    // Use this for initialization
    void Awake()
    {
        if (mN_nbConditions > 0 && mB_order)
        {
            mN_correctKeys = 0;
            //mN_keysOrder = new int[mN_nbConditions];
        }
        Debug.Log(mN_keysOrder[0] + " " + mN_keysOrder[1] + " " + mN_keysOrder[2]);
    }

    public void KeyIn(/*Add the key script reference*/int keyIndex)
    {
        if (mN_nbConditions <= 0)//If the door requires more than one condition to be opened (you never know)
        {
            OpenDoor();
        } else
        {
            if (mB_order && !mB_isOpen)
            {
                Debug.Log("Key index : " + keyIndex + ". Awaited index : " + mN_keysOrder[mN_correctKeys]);
                if (keyIndex == mN_keysOrder[mN_correctKeys])//If the index of the calling key is correct (referring to mN_correctKeys) mN_correctKeys is incremented
                {
                    mN_correctKeys++;
                    Debug.Log("Correct index : " + keyIndex + ". Nb keys : " + mN_keysOrder.Length);
                    if(mN_correctKeys == mN_keysOrder.Length)//If each key is placed in the correct order, the door opens
                    {
                        OpenDoor();
                    }
                    
                } else//Otherwise, either mN_correctKeys is set to 0 or the current key is not validated considering the value of mB_punitiveOrder
                {
                    if (mB_punitiveOrder)
                    {
                        mN_correctKeys = 0;
                    }
                }

            }
        }
    }

    public bool KeyOut(int keyIndex)
    {
        for (int i = 0; i < mN_correctKeys; i++)
        {
            if (mN_keysOrder[i] == keyIndex)//If the key currently getting out is part of the correct keys entered, mN_correctKeys is set as the index of the combination key index
            {
                mN_correctKeys = i;
                return true;
            }
        }
        return false;
    }

    protected virtual void OpenDoor()
    {
        mB_isOpen = true;
        this.GetComponent<Animator>().SetBool("isOpen", true);
        Debug.Log("Door opening");
    }

    protected virtual void CloseDoor()
    {
        mB_isOpen = false;
    }

    public bool IsOpen()
    {
        return mB_isOpen;
    }
}
