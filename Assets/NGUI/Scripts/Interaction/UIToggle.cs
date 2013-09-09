//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using AnimationOrTween;

/// <summary>
/// Simple toggle functionality.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Toggle")]
public class UIToggle : MonoBehaviour
{
	/// <summary>
	/// List of all the active toggles currently in the scene.
	/// </summary>

	static public BetterList<UIToggle> list = new BetterList<UIToggle>();

	/// <summary>
	/// Current toggle that sent a state change notification.
	/// </summary>

	static public UIToggle current;
	public delegate void OnStateChange (bool state);

	/// <summary>
	/// If set to anything other than '0', all active toggles in this group will behave as radio buttons.
	/// </summary>

	public int group = 0;

	/// <summary>
	/// Sprite that's visible when the 'isChecked' status is 'true'.
	/// </summary>

	public UISprite activeSprite;

	/// <summary>
	/// Animation to play on the checkmark sprite, if any.
	/// </summary>

	public Animation activeAnimation;

	/// <summary>
	/// Whether the toggle starts checked.
	/// </summary>

	public bool startsActive = false;

	/// <summary>
	/// If checked, tween-based transition will be instant instead.
	/// </summary>

	public bool instantTween = false;

	/// <summary>
	/// Can the radio button option be 'none'?
	/// </summary>

	public bool optionCanBeNone = false;

	/// <summary>
	/// Generic event receiver that will be notified when the state changes.
	/// </summary>

	public GameObject eventReceiver;

	/// <summary>
	/// Function that will be called on the event receiver when the state changes.
	/// </summary>

	public string functionName = "OnActivate";

	/// <summary>
	/// Delegate that will be called when the toggle's state changes. Faster than using 'eventReceiver'.
	/// </summary>

	public OnStateChange onStateChange;

	/// <summary>
	/// Deprecated functionality. Use the 'group' option instead.
	/// </summary>

	[HideInInspector][SerializeField] Transform radioButtonRoot;
	[HideInInspector][SerializeField] bool startsChecked;
	[HideInInspector][SerializeField] UISprite checkSprite;
	[HideInInspector][SerializeField] Animation checkAnimation;

	bool mChecked = true;
	bool mStarted = false;

	/// <summary>
	/// Whether the toggle is checked.
	/// </summary>

	public bool isChecked
	{
		get { return mChecked; }
		set { if (group == 0 || value || optionCanBeNone || !mStarted) Set(value); }
	}

	/// <summary>
	/// Legacy functionality support -- set the radio button root if the 'option' value was 'true'.
	/// </summary>

	void Awake ()
	{
		// Auto-upgrade
		if (startsChecked)
		{
			startsChecked = false;
			startsActive = true;
		}

		if (checkSprite != null && activeSprite == null)
		{
			activeSprite = checkSprite;
			checkSprite = null;
		}

		if (checkAnimation != null && activeAnimation == null)
		{
			activeAnimation = checkAnimation;
			checkAnimation = null;
		}

		if (Application.isPlaying && activeSprite != null)
			activeSprite.alpha = startsActive ? 1f : 0f;
	}

	void OnEnable ()  { list.Add(this); }
	void OnDisable () { list.Remove(this); }

	/// <summary>
	/// Activate the initial state.
	/// </summary>

	void Start ()
	{
		if (radioButtonRoot != null && group == 0)
		{
			Debug.LogWarning(NGUITools.GetHierarchy(gameObject) +
				" uses a 'Radio Button Root'. You need to change it to use a 'group' instead.", this);
		}

		if (Application.isPlaying)
		{
			if (eventReceiver == null) eventReceiver = gameObject;
			mChecked = !startsActive;
			mStarted = true;
			Set(startsActive);
		}
	}

	/// <summary>
	/// Check or uncheck on click.
	/// </summary>

	void OnClick () { if (enabled) isChecked = !isChecked; }

	/// <summary>
	/// Fade out or fade in the checkmark and notify the target of OnChecked event.
	/// </summary>

	void Set (bool state)
	{
		if (!mStarted)
		{
			mChecked = state;
			startsActive = state;
			if (activeSprite != null) activeSprite.alpha = state ? 1f : 0f;
		}
		else if (mChecked != state)
		{
			// Uncheck all other togglees
			if (group != 0 && state)
			{
				for (int i = 0, imax = list.size; i < imax; ++i)
				{
					UIToggle cb = list[i];
					if (cb != this && cb.group == group) cb.Set(false);
				}
			}

			// Remember the state
			mChecked = state;

			// Tween the color of the checkmark
			if (activeSprite != null)
			{
				if (instantTween)
				{
					activeSprite.alpha = mChecked ? 1f : 0f;
				}
				else
				{
					TweenAlpha.Begin(activeSprite.gameObject, 0.15f, mChecked ? 1f : 0f);
				}
			}

			current = this;

			// Notify the delegate
			if (onStateChange != null) onStateChange(mChecked);

			// Send out the event notification
			if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
			{
				eventReceiver.SendMessage(functionName, mChecked, SendMessageOptions.DontRequireReceiver);
			}
			current = null;

			// Play the checkmark animation
			if (activeAnimation != null)
			{
				ActiveAnimation.Play(activeAnimation, state ? Direction.Forward : Direction.Reverse);
			}
		}
	}
}
