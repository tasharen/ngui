//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Simple slider functionality.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Slider")]
public class UISlider : IgnoreTimeScale
{
	public enum Direction
	{
		Horizontal,
		Vertical,
	}

	/// <summary>
	/// Current slider. This value is set prior to the callback function being triggered.
	/// </summary>

	static public UISlider current;

	/// <summary>
	/// Object used for the foreground.
	/// </summary>

	public Transform foreground;

	/// <summary>
	/// Object that acts as a thumb.
	/// </summary>

	public Transform thumb;

	/// <summary>
	/// Direction the slider will expand in.
	/// </summary>

	public Direction direction = Direction.Horizontal;

	/// <summary>
	/// When at 100%, this will be the size of the foreground object.
	/// </summary>

	public Vector2 fullSize = Vector2.zero;

	/// <summary>
	/// Event receiver that will be notified of the value changes.
	/// </summary>

	public GameObject eventReceiver;

	/// <summary>
	/// Function on the event receiver that will receive the value changes.
	/// </summary>

	public string functionName = "OnSliderChange";

	/// <summary>
	/// Number of steps the slider should be divided into. For example 5 means possible values of 0, 0.25, 0.5, 0.75, and 1.0.
	/// </summary>

	public int numberOfSteps = 0;

	// Used to be public prior to 1.87
	[SerializeField] float rawValue = 1f;

	float mStepValue = 1f;
	BoxCollider mCol;
	Transform mTrans;
	Transform mForeTrans;
	UIWidget mWidget;
	UIFilledSprite mSprite;

	/// <summary>
	/// Value of the slider.
	/// </summary>

	public float sliderValue { get { return mStepValue; } set { Set(value); } }

	/// <summary>
	/// Ensure that we have a background and a foreground object to work with.
	/// </summary>

	void Awake ()
	{
		mTrans = transform;
		mCol = collider as BoxCollider;
	}

	/// <summary>
	/// We want to receive drag events from the thumb.
	/// </summary>

	void Start ()
	{
		if (foreground != null)
		{
			mWidget = foreground.GetComponent<UIWidget>();
			mSprite = (mWidget != null) ? mWidget as UIFilledSprite : null;
			mForeTrans = foreground.transform;
			if (fullSize == Vector2.zero) fullSize = foreground.localScale;
		}
		else if (mCol != null)
		{
			if (fullSize == Vector2.zero) fullSize = mCol.size;
		}
		else
		{
			Debug.LogWarning("UISlider expected to find a foreground object or a box collider to work with", this);
		}

		if (Application.isPlaying && thumb != null && thumb.collider != null)
		{
			UIEventListener listener = UIEventListener.Add(thumb.gameObject);
			listener.onPress += OnPressThumb;
			listener.onDrag += OnDragThumb;
		}
		Set(rawValue);
	}

	/// <summary>
	/// Update the slider's position on press.
	/// </summary>

	void OnPress (bool pressed) { if (pressed) UpdateDrag(); }

	/// <summary>
	/// When dragged, figure out where the mouse is and calculate the updated value of the slider.
	/// </summary>

	void OnDrag (Vector2 delta) { UpdateDrag(); }

	/// <summary>
	/// Callback from the thumb.
	/// </summary>

	void OnPressThumb (GameObject go, bool pressed) { if (pressed) UpdateDrag(); }

	/// <summary>
	/// Callback from the thumb.
	/// </summary>

	void OnDragThumb (GameObject go, Vector2 delta) { UpdateDrag(); }

	/// <summary>
	/// Watch for key events and adjust the value accordingly.
	/// </summary>

	void OnKey (KeyCode key)
	{
		float step = (numberOfSteps > 1f) ? 1f / (numberOfSteps - 1) : 0.125f;

		if (direction == Direction.Horizontal)
		{
			if		(key == KeyCode.LeftArrow)	Set(rawValue - step);
			else if (key == KeyCode.RightArrow) Set(rawValue + step);
		}
		else
		{
			if		(key == KeyCode.DownArrow)	Set(rawValue - step);
			else if (key == KeyCode.UpArrow)	Set(rawValue + step);
		}
	}

	/// <summary>
	/// Update the slider's position based on the mouse.
	/// </summary>

	void UpdateDrag ()
	{
		// Create a plane for the slider
		if (mCol == null || UICamera.currentCamera == null || UICamera.currentTouch == null) return;

		// Don't consider the slider for click events
		UICamera.currentTouch.clickNotification = UICamera.ClickNotification.None;

		// Create a ray and a plane
		Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);
		Plane plane = new Plane(mTrans.rotation * Vector3.back, mTrans.position);

		// If the ray doesn't hit the plane, do nothing
		float dist;
		if (!plane.Raycast(ray, out dist)) return;

		// Collider's bottom-left corner in local space
		Vector3 localOrigin = mTrans.localPosition + mCol.center - mCol.extents;
		Vector3 localOffset = mTrans.localPosition - localOrigin;

		// Direction to the point on the plane in scaled local space
		Vector3 localCursor = mTrans.InverseTransformPoint(ray.GetPoint(dist));
		Vector3 dir = localCursor + localOffset;

		// Update the slider
		Set( (direction == Direction.Horizontal) ? dir.x / mCol.size.x : dir.y / mCol.size.y );
	}

#if UNITY_EDITOR
	void Update () { Set(rawValue); }
#endif

	/// <summary>
	/// Update the visible slider.
	/// </summary>

	void Set (float input)
	{
		// Clamp the input
		float val = Mathf.Clamp01(input);
		if (val < 0.001f) val = 0f;

		// Save the raw value
		rawValue = val;

		// Take steps into consideration
		if (numberOfSteps > 1) val = Mathf.Round(val * (numberOfSteps - 1)) / (numberOfSteps - 1); ;

		// If the stepped value doesn't match the last one, it's time to update
		if (mStepValue != val)
		{
			mStepValue = val;
			Vector3 scale = fullSize;

			if (direction == Direction.Horizontal) scale.x *= mStepValue;
			else scale.y *= mStepValue;

			if (mSprite != null)
			{
				mSprite.fillAmount = mStepValue;
			}
			else if (mForeTrans != null)
			{
				mForeTrans.localScale = scale;
				if (mWidget != null) mWidget.MarkAsChanged();
			}

			if (thumb != null)
			{
				Vector3 pos = thumb.localPosition;

				if (mSprite != null)
				{
					switch (mSprite.fillDirection)
					{
						case UIFilledSprite.FillDirection.TowardRight:		pos.x = scale.x; break;
						case UIFilledSprite.FillDirection.TowardTop:		pos.y = scale.y; break;
						case UIFilledSprite.FillDirection.TowardLeft:		pos.x = fullSize.x - scale.x; break;
						case UIFilledSprite.FillDirection.TowardBottom:		pos.y = fullSize.y - scale.y; break;
					}
				}
				else if (direction == Direction.Horizontal)
				{
					pos.x = scale.x;
				}
				else
				{
					pos.y = scale.y;
				}
				thumb.localPosition = pos;
			}

			if (eventReceiver != null && !string.IsNullOrEmpty(functionName) && Application.isPlaying)
			{
				current = this;
				eventReceiver.SendMessage(functionName, mStepValue, SendMessageOptions.DontRequireReceiver);
				current = null;
			}
		}
	}
}