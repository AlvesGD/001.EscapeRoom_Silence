using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
	private List<FadingSource> m_sources;

	public struct FadingSource
	{
		public AudioSource source;
		public float fadingDuration;
		public float fadingTime;

		public FadingSource(AudioSource src, float duration)
		{
			source = src;
			fadingDuration = duration;
			fadingTime = 0;
		}

		public FadingSource(AudioSource src, float duration, float time)
		{
			source = src;
			fadingDuration = duration;
			fadingTime = time;
		}
	}

	private void Start()
	{
		m_sources = new List<FadingSource>();
	}

	private void Update()
	{
			for(int i = m_sources.Count-1; i >= 0 ; i --)
			{
				if(m_sources[i].fadingTime < m_sources[i].fadingDuration)
				{
					m_sources[i].source.volume = Mathf.Lerp(1, 0, m_sources[i].fadingTime/m_sources[i].fadingDuration);

					//FIX ME : Disgusting (cannot change the value of m_sources[i].fadingTime directly).
					m_sources[i] = new FadingSource(m_sources[i].source, m_sources[i].fadingDuration, m_sources[i].fadingTime + Time.deltaTime);
				}
				else
				{
					m_sources[i].source.Stop();
					m_sources[i].source.volume = 1;
					m_sources.RemoveAt(i);
				}
			}
	}

	public AudioClip RandomSound(List<AudioClip> clips)
	{
		return clips[Random.Range(0, clips.Count - 1)];
	}

	public void TryPlaySound(AudioSource src, AudioClip clip)
	{
		if(!src.isPlaying)	
		{
			if(src.clip != clip)//If the clip assigned to the audio source isn't already the one trying to be played.
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

	public void StartFadeSource(AudioSource src, float time)
	{
		bool temp = false;

		//Checking if the audio source hasn't already been added to the list.
		for(int i = 0 ; i < m_sources.Count ; i ++)
		{
			if(m_sources[i].source == src && !src.isPlaying)
				temp = true;
		}
		if(!temp)
			m_sources.Add(new FadingSource(src, time));
	}

	public void StopSources(List<AudioSource> sources)
	{
		for(int i = 0 ; i < sources.Count ; i++)
		{
			if(sources[i].isPlaying)
				sources[i].Stop();
		}
	}
}
