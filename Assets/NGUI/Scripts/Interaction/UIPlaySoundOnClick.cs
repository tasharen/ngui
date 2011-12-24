using UnityEngine;

/// <summary>
/// Plays the specified sound on click.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Play Sound (OnClick)")]
public class UIPlaySoundOnClick : MonoBehaviour
{
	public AudioClip clip;
	public float volume = 1f;

	void OnClick ()
	{
		NGUITools.PlaySound(clip, volume);
	}
}