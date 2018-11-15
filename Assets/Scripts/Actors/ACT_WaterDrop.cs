using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACT_WaterDrop : MonoBehaviour
{

    [SerializeField] private float m_speed;
    private bool m_dripping;

    // Use this for initialization    

    private void Awake()
    {
        m_dripping = true;
        Invoke("Kill", 5.0f);
    }

    private void Movement()
    {
        this.transform.Translate(new Vector3(0.0f, m_speed * Time.deltaTime, 0.0f) * -1.0f);
    }

    public void Kill()
    {
        Debug.Log("Waterdrop : " + this.gameObject.name + " destroyed.");
        Destroy(this.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("WaterDropper"))
        {
            Movement();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("WaterDropper"))
        {
            m_dripping = false;
            this.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_dripping && !other.CompareTag("Dropped"))
        {
            Kill();
        }
    }
}
