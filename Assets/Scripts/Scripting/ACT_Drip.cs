using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACT_Drip : MonoBehaviour
{
    //References
    [SerializeField] private ACT_Vase m_vaseRef;

    public int GetIndex()
    {
        return m_vaseRef.GetIndex();
    }

    public ACT_Vase GetVaseRef()
    {
        return m_vaseRef;
    }
}
