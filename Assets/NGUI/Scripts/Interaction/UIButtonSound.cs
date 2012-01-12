using UnityEngine;

/// <summary>
/// Plays the specified sound.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Sound")]
public class UIButtonSound : MonoBehaviour
{
	public AudioClip onHoverOver;
	public AudioClip onHoverOut;
	public AudioClip onPressed;
	public AudioClip onUnpressed;
	public AudioClip onClick;
	public float volume = 1f;

	void OnHover (bool isOver)
	{
		NGUITools.PlaySound(isOver ? onHoverOver : onHoverOut, volume);
	}

	void OnPress (bool isPressed)
	{
		NGUITools.PlaySound(isPressed ? onPressed : onUnpressed, volume);
	}

	void OnClick ()
	{
		NGUITools.PlaySound(onClick, volume);
	}
}