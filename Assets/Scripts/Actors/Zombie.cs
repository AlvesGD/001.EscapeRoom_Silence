using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{

    private bool mB_stop;

    private void Start()
    {
        mB_stop = false;    
    }

    private void Update()
    {
        if(mB_stop)
        {
            if (this.GetComponents<AudioSource>()[0].volume > 0.0f)
            {
                this.GetComponents<AudioSource>()[0].volume -= (1 * Time.deltaTime);
            }else
            {
                this.GetComponents<AudioSource>()[0].Stop();
                mB_stop = false;
                this.GetComponents<AudioSource>()[0].volume = 0.41f;
            }
        }
    }

    public void StopCollision()
    {
        mB_stop = true;
    }

    public void PlayCollision()
    {
        if (!this.GetComponents<AudioSource>()[1].isPlaying)
        {
            this.GetComponents<AudioSource>()[1].Play();
        }
    }
}
