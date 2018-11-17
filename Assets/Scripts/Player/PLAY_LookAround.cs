using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PLAY_LookAround : MonoBehaviour
{

    public enum ENUM_RotationAxis
    {
        X, Y, Z, NB_AXIS
    }

    [SerializeField] private ENUM_RotationAxis m_rotAxis;
    [SerializeField] private float m_Xclamp;
    [SerializeField] private float m_sensitivity;
    [SerializeField] private bool m_smooth;
    private bool[] m_blockedAxis = new bool[(int)ENUM_RotationAxis.NB_AXIS]; //Should an axis be blocked ?
    private bool m_gamepad;

    private void Start()
    {
        if (Input.GetJoystickNames().Length == 0)
            m_gamepad = false;
        else
            m_gamepad = true;

        for (int i = 0; i < (int)ENUM_RotationAxis.NB_AXIS; i++)
        {
            m_blockedAxis[i] = false;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        Look();
    }

    private void Look()
    {
        switch(m_rotAxis)
        {
            case ENUM_RotationAxis.X:
                if(Mathf.Abs(Input.GetAxis("Y")) > 0.1f)//Precision (Point mort)
                {
                    if (!m_blockedAxis[(int)ENUM_RotationAxis.X])    //If the axis is not being blocked
                        this.transform.Rotate(Input.GetAxis("Y") * m_sensitivity * (-1.0f), 0.0f, 0.0f);
                    
                    if(m_Xclamp != 0.0f)
                    {
                        if (this.transform.eulerAngles.x > 45f && this.transform.eulerAngles.x < 180f)
                        {
                            this.transform.eulerAngles = new Vector3(45f, this.transform.eulerAngles.y, this.transform.eulerAngles.z);
                        }
                        if(this.transform.eulerAngles.x < 315f && this.transform.eulerAngles.x > 180f)
                        {
                            this.transform.eulerAngles = new Vector3(315f, this.transform.eulerAngles.y, this.transform.eulerAngles.z);
                        }
                    }
                }
                break;
            case ENUM_RotationAxis.Y:
                if (Mathf.Abs(Input.GetAxis("X")) > 0.05f)//Precision (Point mort)
                {
                    if (!m_blockedAxis[(int)ENUM_RotationAxis.Y])
                        this.transform.Rotate(0.0f, Input.GetAxis("X") * m_sensitivity, 0.0f);
                }
                break;
            case ENUM_RotationAxis.Z:
                if (Mathf.Abs(Input.GetAxis("Y")) > 0.05f)//Precision (Point mort)
                {
                    if (!m_blockedAxis[(int)ENUM_RotationAxis.Z])
                        this.transform.Rotate(0.0f, 0.0f, Input.GetAxis("Y") * m_sensitivity);
                }
                break;
            default:
                break;
        }
    }

    //MUTATORS

    public void BlockAllAxis()
    {
        for(int i = 0; i < (int)ENUM_RotationAxis.NB_AXIS; i++)
        {
            m_blockedAxis[i] = true;
        }
    }

    public void FreeAllAxis()
    {
        for (int i = 0; i < (int)ENUM_RotationAxis.NB_AXIS; i++)
        {
            m_blockedAxis[i] = false;
        }
    }

    public void FreeAxis(ENUM_RotationAxis axis)
    {
        m_blockedAxis[(int)axis] = false;
    }

    public void BlockAxis(ENUM_RotationAxis axis)
    {
        m_blockedAxis[(int)axis] = true;
    }
}
