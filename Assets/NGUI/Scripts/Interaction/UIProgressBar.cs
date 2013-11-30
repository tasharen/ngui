//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple progress bar that fills itself based on the specified value.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/NGUI Progress Bar")]
public class UIProgressBar : UIWidgetContainer
{
	public enum FillDirection
	{
		LeftToRight,
		RightToLeft,
		BottomToTop,
		TopToBottom,
	}

	/// <summary>
	/// Current slider. This value is set prior to the callback function being triggered.
	/// </summary>

	static public UIProgressBar current;

	/// <summary>
	/// Delegate triggered when the scroll bar stops being dragged.
	/// Useful for things like centering on the closest valid object, for example.
	/// </summary>

	public OnDragFinished onDragFinished;
	public delegate void OnDragFinished ();

	[HideInInspector][SerializeField] protected UIWidget mBG;
	[HideInInspector][SerializeField] protected UIWidget mFG;
	[HideInInspector][SerializeField] protected float mValue = 1f;
	[HideInInspector][SerializeField] protected FillDirection mFill = FillDirection.LeftToRight;

	protected Transform mTrans;
	protected bool mIsDirty = false;
	protected Camera mCam;
	protected UISprite mSprite;
	protected float mOffset = 0f;

	/// <summary>
	/// Number of steps the slider should be divided into. For example 5 means possible values of 0, 0.25, 0.5, 0.75, and 1.0.
	/// </summary>

	public int numberOfSteps = 0;

	/// <summary>
	/// Callbacks triggered when the scroll bar's value changes.
	/// </summary>

	public List<EventDelegate> onChange = new List<EventDelegate>();

	/// <summary>
	/// Cached for speed.
	/// </summary>

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	/// <summary>
	/// Camera used to draw the scroll bar.
	/// </summary>

	public Camera cachedCamera { get { if (mCam == null) mCam = NGUITools.FindCameraForLayer(gameObject.layer); return mCam; } }

	/// <summary>
	/// Widget used for the foreground.
	/// </summary>

	public UIWidget foregroundWidget { get { return mFG; } set { if (mFG != value) { mFG = value; mIsDirty = true; } } }

	/// <summary>
	/// Widget used for the background.
	/// </summary>

	public UIWidget backgroundWidget { get { return mBG; } set { if (mBG != value) { mBG = value; mIsDirty = true; } } }

	/// <summary>
	/// The scroll bar's direction.
	/// </summary>

	public FillDirection fillDirection
	{
		get
		{
			return mFill;
		}
		set
		{
			if (mFill != value)
			{
				mFill = value;
				mIsDirty = true;
				if (!Application.isPlaying) ForceUpdate();
			}
		}
	}

	/// <summary>
	/// Modifiable value for the scroll bar, 0-1 range.
	/// </summary>

	public float value
	{
		get
		{
			if (numberOfSteps > 1) return Mathf.Round(mValue * (numberOfSteps - 1)) / (numberOfSteps - 1);
			return mValue;
		}
		set
		{
			float val = Mathf.Clamp01(value);

			if (mValue != val)
			{
				mValue = val;
				mIsDirty = true;

				if (EventDelegate.IsValid(onChange))
				{
					current = this;
					EventDelegate.Execute(onChange);
					current = null;
				}
				if (!Application.isPlaying) ForceUpdate();
			}
		}
	}

	/// <summary>
	/// Allows to easily change the scroll bar's alpha, affecting both the foreground and the background sprite at once.
	/// </summary>

	public float alpha
	{
		get
		{
			if (mFG != null) return mFG.alpha;
			if (mBG != null) return mBG.alpha;
			return 1f;
		}
		set
		{
			if (mFG != null)
			{
				mFG.alpha = value;
				if (mFG.collider != null) mFG.collider.enabled = mFG.alpha > 0.001f;
			}

			if (mBG != null)
			{
				mBG.alpha = value;
				if (mBG.collider != null) mBG.collider.enabled = mBG.alpha > 0.001f;
			}
		}
	}

	/// <summary>
	/// Whether the progress bar is horizontal in nature. Convenience function.
	/// </summary>

	protected bool isHorizontal { get { return (mFill == FillDirection.LeftToRight || mFill == FillDirection.RightToLeft); } }

	/// <summary>
	/// Whether the progress bar is inverted in its behaviour. Convenience function.
	/// </summary>

	protected bool isInverted { get { return (mFill == FillDirection.RightToLeft || mFill == FillDirection.TopToBottom); } }

	/// <summary>
	/// Position the scroll bar to be under the current touch.
	/// </summary>

	protected void OnPressBackground (GameObject go, bool isPressed)
	{
		mCam = UICamera.currentCamera;
		value = ScreenToValue(UICamera.lastTouchPosition);
		if (!isPressed && onDragFinished != null) onDragFinished();
	}

	/// <summary>
	/// Position the scroll bar to be under the current touch.
	/// </summary>

	protected void OnDragBackground (GameObject go, Vector2 delta)
	{
		mCam = UICamera.currentCamera;
		value = ScreenToValue(UICamera.lastTouchPosition);
	}

	/// <summary>
	/// Save the position of the foreground on press.
	/// </summary>

	protected void OnPressForeground (GameObject go, bool isPressed)
	{
		if (isPressed)
		{
			mOffset = (mFG == null) ? 0f :
				value - ScreenToValue(UICamera.lastTouchPosition);
		}
		else if (onDragFinished != null) onDragFinished();
	}

	/// <summary>
	/// Drag the scroll bar in the specified direction.
	/// </summary>

	protected void OnDragForeground (GameObject go, Vector2 delta)
	{
		mCam = UICamera.currentCamera;
		value = mOffset + ScreenToValue(UICamera.lastTouchPosition);
	}

	/// <summary>
	/// Watch for key events and adjust the value accordingly.
	/// </summary>

	protected void OnKey (KeyCode key)
	{
		if (enabled)
		{
			float step = (numberOfSteps > 1f) ? 1f / (numberOfSteps - 1) : 0.125f;

			if (fillDirection == FillDirection.LeftToRight || fillDirection == FillDirection.RightToLeft)
			{
				if (key == KeyCode.LeftArrow) value = mValue - step;
				else if (key == KeyCode.RightArrow) value = mValue + step;
			}
			else
			{
				if (key == KeyCode.DownArrow) value = mValue - step;
				else if (key == KeyCode.UpArrow) value = mValue + step;
			}
		}
	}

	/// <summary>
	/// Register the event listeners.
	/// </summary>

	protected void Start ()
	{
		Upgrade();

		mSprite = mFG as UISprite;

		if (Application.isPlaying)
		{
			if (mFG == null)
			{
				Debug.LogWarning("Progress bar needs a foreground widget to work with", this);
				enabled = false;
				return;
			}

			GameObject bg = (mBG != null && mBG.collider != null) ? mBG.gameObject : gameObject;
			UIEventListener bgl = UIEventListener.Get(bg);
			bgl.onPress += OnPressBackground;
			bgl.onDrag += OnDragBackground;
			if (mBG != null) mBG.autoResizeBoxCollider = true;

			OnStart();

			if (onChange != null)
			{
				current = this;
				EventDelegate.Execute(onChange);
				current = null;
			}
		}
		ForceUpdate();
	}

	/// <summary>
	/// Functionality for derived classes.
	/// </summary>

	protected virtual void OnStart() { }

	/// <summary>
	/// Update the value of the scroll bar if necessary.
	/// </summary>

	protected void Update ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		if (mIsDirty) ForceUpdate();
	}

	/// <summary>
	/// Invalidate the scroll bar.
	/// </summary>

	protected void OnValidate ()
	{
		Upgrade();
		mIsDirty = true;
		mSprite = mFG as UISprite;
		float val = Mathf.Clamp01(mValue);
		if (mValue != val) mValue = val;
		if (numberOfSteps < 0) numberOfSteps = 0;
		else if (numberOfSteps > 20) numberOfSteps = 20;
		ForceUpdate();
	}

	/// <summary>
	/// Used to upgrade from legacy functionality.
	/// </summary>

	protected virtual void Upgrade () { }

	/// <summary>
	/// Drag the scroll bar by the specified on-screen amount.
	/// </summary>

	protected float ScreenToValue (Vector2 screenPos)
	{
		// Create a plane
		Transform trans = cachedTransform;
		Plane plane = new Plane(trans.rotation * Vector3.back, trans.position);

		// If the ray doesn't hit the plane, do nothing
		float dist;
		Ray ray = cachedCamera.ScreenPointToRay(screenPos);
		if (!plane.Raycast(ray, out dist)) return value;

		// Transform the point from world space to local space
		return LocalToValue(trans.InverseTransformPoint(ray.GetPoint(dist)));
	}

	/// <summary>
	/// Calculate the value of the progress bar given the specified local position.
	/// </summary>

	protected virtual float LocalToValue (Vector2 localPos)
	{
		if (mFG != null)
		{
			Vector3[] corners = mFG.localCorners;
			Vector3 size = (corners[2] - corners[0]);

			if (isHorizontal)
			{
				float diff = (localPos.x - corners[0].x) / size.x;
				return Mathf.Clamp01(isInverted ? 1f - diff : diff);
			}
			else
			{
				float diff = (localPos.y - corners[0].y) / size.y;
				return Mathf.Clamp01(isInverted ? 1f - diff : diff);
			}
		}
		return value;
	}

	/// <summary>
	/// Update the value of the scroll bar.
	/// </summary>

	public virtual void ForceUpdate ()
	{
		mIsDirty = false;

		if (mFG != null)
		{
			if (isHorizontal)
			{
				if (mSprite != null && mSprite.type == UISprite.Type.Filled)
				{
					mSprite.fillDirection = UISprite.FillDirection.Horizontal;
					mSprite.invert = isInverted;
					mSprite.fillAmount = value;
				}
				else
				{
					mFG.drawRegion = isInverted ?
						new Vector4(1f - value, 0f, 1f, 1f) :
						new Vector4(0f, 0f, value, 1f);
				}
			}
			else if (mSprite != null && mSprite.type == UISprite.Type.Filled)
			{
				mSprite.fillDirection = UISprite.FillDirection.Vertical;
				mSprite.invert = isInverted;
				mSprite.fillAmount = value;
			}
			else
			{
				mFG.drawRegion = isInverted ?
					new Vector4(0f, 1f - value, 1f, 1f) :
					new Vector4(0f, 0f, 1f, value);
			}
		}
	}
}
