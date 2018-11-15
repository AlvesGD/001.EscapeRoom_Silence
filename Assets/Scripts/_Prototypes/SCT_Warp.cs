using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SCT_Warp : MonoBehaviour {

	[SerializeField]private string m_sceneWarp;

	// Use this for initialization

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            SceneManager.LoadSceneAsync(m_sceneWarp, LoadSceneMode.Single);
        }
    }
}
