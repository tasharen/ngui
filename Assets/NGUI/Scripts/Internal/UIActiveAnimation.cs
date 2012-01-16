using UnityEngine;

/// <summary>
/// Internal script used by  This script is attached to the animated object,
/// and is used to store additional information about the animation process.
/// </summary>

[RequireComponent(typeof(Animation))]
[AddComponentMenu("NGUI/Internal/Active Animation")]
public class UIActiveAnimation : MonoBehaviour
{
	/// <summary>
	/// Function to call when the animation finishes playing.
	/// </summary>

	public string callWhenFinished;

	Animation mAnim;
	int mLastDirection = 0;
	bool mNotify = false;
	int mDisableDirection;

	/// <summary>
	/// Notify the target when the animation finishes playing.
	/// </summary>

	void Update ()
	{
		if (mAnim != null)
		{
			if (mAnim.isPlaying) return;

			if (mNotify)
			{
				mNotify = false;

				// Notify listeners
				if (!string.IsNullOrEmpty(callWhenFinished)) SendMessage(callWhenFinished, SendMessageOptions.DontRequireReceiver);

				if (mDisableDirection != 0 && mLastDirection == mDisableDirection)
				{
					gameObject.SetActiveRecursively(false);
				}
			}
		}
		enabled = false;
	}

	/// <summary>
	/// Play the specified animation.
	/// </summary>

	void Play (string clipName, int playDirection)
	{
		if (mAnim != null)
		{
			// Determine the play direction
			if (playDirection == 0) playDirection = (mLastDirection != 1) ? 1 : -1;

			// Don't play the animation from start if we haven't returned it back to origin
			if (mLastDirection != playDirection)
			{
				bool noName = string.IsNullOrEmpty(clipName);

				// If this animation is not already playing, play it
				if ((noName && !mAnim.isPlaying) || (!noName && !mAnim.IsPlaying(clipName)))
				{
					if (noName) mAnim.Play();
					else mAnim.CrossFade(clipName);
				}

				// Update the animation speed based on direction -- forward or back
				foreach (AnimationState state in mAnim)
				{
					if (string.IsNullOrEmpty(clipName) || state.name == clipName)
					{
						float speed = Mathf.Abs(state.speed);
						state.speed = speed * playDirection;

						// Automatically start the animation from the end if it's playing in reverse
						if (playDirection == -1 && state.time == 0f) state.time = state.length;
						else if (playDirection == 1 && state.time == state.length) state.time = 0f;
					}
				}

				// Remember the direction for disable checks in Update() below
				mLastDirection = playDirection;
				mNotify = true;
			}
		}
	}

	/// <summary>
	/// Play the specified animation on the specified object.
	/// </summary>

	static public UIActiveAnimation Play (Animation anim, string clipName, int playDirection, bool enableBeforePlay, int disableDirection)
	{
		if (!anim.gameObject.active)
		{
			// If the object is disabled, don't do anything
			if (!enableBeforePlay) return null;

			// Enable the game object before animating it
			anim.gameObject.SetActiveRecursively(true);
		}

		UIActiveAnimation aa = anim.GetComponent<UIActiveAnimation>();
		if (aa != null) aa.enabled = true;
		else aa = anim.gameObject.AddComponent<UIActiveAnimation>();
		aa.mAnim = anim;
		aa.mDisableDirection = disableDirection;
		aa.Play(clipName, playDirection);
		return aa;
	}
}