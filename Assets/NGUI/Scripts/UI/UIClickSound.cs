using UnityEngine;

/// <summary>
/// Plays the specified sound on click.
/// </summary>

[AddComponentMenu("UI/Click Sound")]
public class UIClickSound : MonoBehaviour
{
	public AudioClip clip;
	public float volume = 1f;

	void OnClick ()
	{
		if (clip != null)
		{
			Camera cam = Camera.main;

			if (cam != null)
			{
				AudioListener listener = cam.GetComponent<AudioListener>();
				if (listener == null) listener = cam.gameObject.AddComponent<AudioListener>();

				AudioSource source = cam.audio;
				if (source == null) source = cam.gameObject.AddComponent<AudioSource>();

				source.PlayOneShot(clip, volume);
			}
		}
	}
}