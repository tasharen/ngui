using UnityEngine;

/// <summary>
/// Plays the specified sound on click.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Play Sound (OnClick)")]
public class UIPlaySoundOnClick : MonoBehaviour
{
	public AudioClip clip;
	public float volume = 1f;

	AudioListener mListener;

	void OnClick ()
	{
		if (clip != null)
		{
			if (mListener == null)
			{
				mListener = FindObjectOfType(typeof(AudioListener)) as AudioListener;
				if (mListener == null) mListener = Camera.main.gameObject.AddComponent<AudioListener>();
			}

			AudioSource source = mListener.audio;
			if (source == null) source = mListener.gameObject.AddComponent<AudioSource>();

			source.PlayOneShot(clip, volume);
		}
	}
}