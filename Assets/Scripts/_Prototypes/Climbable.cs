using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbable : Interactable
{
	[SerializeField] private float m_climbDuration;

	public float GetClimbDuration()
	{
		return m_climbDuration;
	}
}
