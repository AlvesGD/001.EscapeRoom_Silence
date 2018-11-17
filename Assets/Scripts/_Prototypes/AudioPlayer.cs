using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{

	public AudioClip RandomSound(List<AudioClip> clips)
	{
		return clips[Random.Range(0, clips.Count - 1)];
	}

	public void TryPlaySound(AudioSource src, AudioClip clip)
	{
		if(!src.isPlaying)	
		{
			src.clip = clip;
			src.Play();
		}
	}

	public AudioSource FreeSource(List<AudioSource> sources)
	{
		for(int i = 0 ; i < sources.Count ; i++)
		{
			if(!sources[i].isPlaying)
				return sources[i];
		}
		return null;
	}

	public void TryPlayRndSound(AudioSource src, List<AudioClip> clips)
	{
		TryPlaySound(src, RandomSound(clips));
	}

	public void TryPlayRndSound(List<AudioSource> sources, List<AudioClip> clips)
	{
		TryPlayRndSound(FreeSource(sources), clips);
	}

	public void TryPlayInRndSource(List<AudioSource> sources, AudioClip clip)
	{
		TryPlaySound(FreeSource(sources), clip);
	}
}
