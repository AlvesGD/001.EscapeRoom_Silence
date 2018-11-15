using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPT_Spawner : MonoBehaviour
{
    [SerializeField] private GameObject m_objectToSpawn;
    [SerializeField] private float m_rate;
    [SerializeField] private bool m_enabled;
    [SerializeField] private Vector3 m_objectScale;

    private float m_coolDown = 0.0f;

    // Use this for initialization
    void Start()
    {
        if (m_rate < 0.01f)
            m_rate = 0.01f;
        m_coolDown = m_rate;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_enabled)
        {
            if(m_coolDown <= 0.0f)
                SpawnObject(m_objectToSpawn);
            else
                m_coolDown -= Time.deltaTime;
        }
    }

    public void SpawnObject(GameObject objectToSpawn)
    {
        m_coolDown = m_rate;
        Instantiate(objectToSpawn, this.transform.position, this.transform.rotation, null).transform.localScale = m_objectScale;
    }

    //MUTATORS

    public void SetEnabled(bool value)
    {
        m_enabled = value;
    }

    public void ReverseEnabled()
    {
        m_enabled = !m_enabled;
    }

}
