//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Scroll bar functionality.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Scroll Bar")]
public class UIScrollBar : UIWidgetContainer
{
	public enum Direction
	{
		Horizontal,
		Vertical,
	};

	/// <summary>
	/// Current scroll bar. This value is set prior to the callback function being triggered.
	/// </summary>

	static public UIScrollBar current;

	/// <summary>
	/// Callbacks triggered when the scroll bar's value changes.
	/// </summary>

	public List<EventDelegate> onChange = new List<EventDelegate>();

	/// <summary>
	/// Delegate triggered when the scroll bar stops being dragged.
	/// Useful for things like centering on the closest valid object, for example.
	/// </summary>

	public OnDragFinished onDragFinished;
	public delegate void OnDragFinished ();

	[HideInInspector][SerializeField] UIWidget mBG;
	[HideInInspector][SerializeField] UIWidget mFG;
	[HideInInspector][SerializeField] Direction mDir = Direction.Horizontal;
	[HideInInspector][SerializeField] bool mInverted = false;
	[HideInInspector][SerializeField] float mScroll = 0f;
	[HideInInspector][SerializeField] float mSize = 1f;

	Transform mTrans;
	bool mIsDirty = false;
	Camera mCam;
	Vector2 mScreenPos = Vector2.zero;
	Vector3 mStartingPos = Vector3.zero;
	Vector2 mStartingSize = Vector2.zero;

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

	public UIWidget foreground { get { return mFG; } set { if (mFG != value) { mFG = value; mIsDirty = true; } } }

	/// <summary>
	/// Widget used for the background.
	/// </summary>

	public UIWidget background { get { return mBG; } set { if (mBG != value) { mBG = value; mIsDirty = true; } } }

	/// <summary>
	/// The scroll bar's direction.
	/// </summary>

	public Direction direction
	{
		get
		{
			return mDir;
		}
		set
		{
			if (mDir != value)
			{
				mDir = value;
				mIsDirty = true;
			}
		}
	}

	/// <summary>
	/// Whether the movement direction is flipped.
	/// </summary>

	public bool inverted { get { return mInverted; } set { if (mInverted != value) { mInverted = value; mIsDirty = true; } } }

	/// <summary>
	/// Modifiable value for the scroll bar, 0-1 range.
	/// </summary>

	public float value
	{
		get
		{
			return mScroll;
		}
		set
		{
			float val = Mathf.Clamp01(value);

			if (mScroll != val)
			{
				mScroll = val;
				mIsDirty = true;
				
				if (onChange != null)
				{
					current = this;
					EventDelegate.Execute(onChange);
					current = null;
				}
			}
		}
	}

	[System.Obsolete("Use 'value' instead")]
	public float scrollValue { get { return this.value; } set { this.value = value; } }

	/// <summary>
	/// The size of the foreground bar in percent (0-1 range).
	/// </summary>

	public float barSize
	{
		get
		{
			return mSize;
		}
		set
		{
			float val = Mathf.Clamp01(value);

			if (mSize != val)
			{
				mSize = val;
				mIsDirty = true;

				if (onChange != null)
				{
					current = this;
					EventDelegate.Execute(onChange);
					current = null;
				}
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
	/// Move the scroll bar to be centered on the specified position.
	/// </summary>

	void CenterOnPos (Vector2 localPos)
	{
		if (mFG == null) return;

		if (mDir == Direction.Horizontal)
		{
			float range = (mStartingSize.x - mFG.width);
			float min = mStartingPos.x - range * 0.5f;
			float val = (localPos.x - min) / range;
			value = Mathf.Clamp01((mInverted ? 1f - val : val));
		}
		else
		{
			float range = (mStartingSize.y - mFG.height);
			float min = mStartingPos.y - range * 0.5f;
			float val = (localPos.y - min) / range;
			value = Mathf.Clamp01((mInverted ? 1f - val : val));
		}
	}

	/// <summary>
	/// Drag the scroll bar by the specified on-screen amount.
	/// </summary>

	void Reposition (Vector2 screenPos)
	{
		// Create a plane
		Transform trans = cachedTransform;
		Plane plane = new Plane(trans.rotation * Vector3.back, trans.position);

		// If the ray doesn't hit the plane, do nothing
		float dist;
		Ray ray = cachedCamera.ScreenPointToRay(screenPos);
		if (!plane.Raycast(ray, out dist)) return;

		// Transform the point from world space to local space
		CenterOnPos(trans.InverseTransformPoint(ray.GetPoint(dist)));
	}

	/// <summary>
	/// Position the scroll bar to be under the current touch.
	/// </summary>

	void OnPressBackground (GameObject go, bool isPressed)
	{
		mCam = UICamera.currentCamera;
		Reposition(UICamera.lastTouchPosition);
		if (!isPressed && onDragFinished != null) onDragFinished();
	}

	/// <summary>
	/// Position the scroll bar to be under the current touch.
	/// </summary>

	void OnDragBackground (GameObject go, Vector2 delta)
	{
		mCam = UICamera.currentCamera;
		Reposition(UICamera.lastTouchPosition);
	}

	/// <summary>
	/// Save the position of the foreground on press.
	/// </summary>

	void OnPressForeground (GameObject go, bool isPressed)
	{
		if (isPressed)
		{
			mCam = UICamera.currentCamera;
			Bounds b = NGUIMath.CalculateAbsoluteWidgetBounds(mFG.cachedTransform);
			mScreenPos = mCam.WorldToScreenPoint(b.center);
		}
		else if (onDragFinished != null) onDragFinished();
	}

	/// <summary>
	/// Drag the scroll bar in the specified direction.
	/// </summary>

	void OnDragForeground (GameObject go, Vector2 delta)
	{
		mCam = UICamera.currentCamera;
		Reposition(mScreenPos + UICamera.currentTouch.totalDelta);
	}

	/// <summary>
	/// Register the event listeners.
	/// </summary>

	void Start ()
	{
		if (mFG != null)
		{
			mStartingSize = new Vector2(mFG.width, mFG.height);
			mFG.pivot = UIWidget.Pivot.Center;
			mStartingPos = mFG.cachedTransform.localPosition;
		}
		else
		{
			Debug.LogWarning("Scroll bar needs a foreground widget to work with", this);
			enabled = false;
			return;
		}

		GameObject bg = (mBG != null && mBG.collider != null) ? mBG.gameObject : gameObject;
		UIEventListener bgl = UIEventListener.Get(bg);
		bgl.onPress += OnPressBackground;
		bgl.onDrag += OnDragBackground;
		mBG.autoResizeBoxCollider = true;

		if (mFG.collider != null && mFG.gameObject != gameObject)
		{
			UIEventListener fgl = UIEventListener.Get(mFG.gameObject);
			fgl.onPress += OnPressForeground;
			fgl.onDrag += OnDragForeground;
			mFG.autoResizeBoxCollider = true;
		}

		if (onChange != null)
		{
			current = this;
			EventDelegate.Execute(onChange);
			current = null;
		}
		ForceUpdate();
	}

	/// <summary>
	/// Update the value of the scroll bar if necessary.
	/// </summary>

	void Update() { if (mIsDirty) ForceUpdate(); }

	/// <summary>
	/// Invalidate the scroll bar.
	/// </summary>

	void OnValidate () { mIsDirty = true; }

	/// <summary>
	/// Update the value of the scroll bar.
	/// </summary>

	public void ForceUpdate ()
	{
		mIsDirty = false;

		if (mFG != null)
		{
			mSize = Mathf.Clamp01(mSize);
			mScroll = Mathf.Clamp01(mScroll);

			float val = mInverted ? 1f - mScroll : mScroll;
			Vector3 pos = mStartingPos;

			if (mDir == Direction.Horizontal)
			{
				int size = Mathf.RoundToInt(mStartingSize.x * mSize);
				mFG.width = ((size & 1) == 1) ? size + 1 : size;
				float diff = (mStartingSize.x - mFG.width) * 0.5f;
				pos.x = Mathf.Round(Mathf.Lerp(pos.x - diff, pos.x + diff, val));
			}
			else
			{
				int size = Mathf.RoundToInt(mStartingSize.y * mSize);
				mFG.height = ((size & 1) == 1) ? size + 1 : size;
				float diff = (mStartingSize.y - mFG.height) * 0.5f;
				pos.y = Mathf.Round(Mathf.Lerp(pos.y - diff, pos.y + diff, val));
			}
			mFG.cachedTransform.localPosition = pos;
		}
	}
}
