using UnityEngine;

/// <summary>
/// Very simple sprite animation. Attach to a sprite and specify a bunch of sprite names and it will cycle through them.
/// </summary>

[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/UI/Sprite Animation")]
public class UISpriteAnimation : MonoBehaviour
{
	public float frameDuration = 1f / 30f;
	public string[] frames;

	UISprite mSprite;
	float mDelay = 0f;
	int mIndex = 0;

	void Start ()
	{
		mSprite = GetComponent<UISprite>();
	}

	void Update ()
	{
		if (frames.Length > 1)
		{
			mDelay += Time.deltaTime;

			if (frameDuration < mDelay)
			{
				if (frameDuration > 0f)
				{
					mDelay -= frameDuration;
				}
				else
				{
					mDelay = 0f;
				}

				if (++mIndex >= frames.Length) mIndex = 0;
				mSprite.spriteName = frames[mIndex];
				mSprite.MakePixelPerfect();
			}
		}
	}
}