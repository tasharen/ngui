//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Simple slider functionality.
/// </summary>

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider))]
[AddComponentMenu("NGUI/Interaction/Slider")]
public class UISlider : MonoBehaviour
{
	public enum Direction
	{
		Horizontal,
		Vertical,
	}

	public Transform foreground;
	public Transform thumb;

	public Direction direction = Direction.Horizontal;
	public float rawValue = 1f;
	public Vector2 fullSize = Vector2.zero;
	public GameObject eventReceiver;
	public string functionName = "OnSliderChange";
	public int numberOfSteps = 0;

	float mStepValue = 1f;
	BoxCollider mCol;
	Transform mTrans;
	Transform mForeTrans;
	UIWidget mWidget;
	UIFilledSprite mSprite;

	/// <summary>
	/// Value of the slider. Will match 'rawValue' unless the slider has steps.
	/// </summary>

	public float sliderValue { get { return mStepValue; } set { Set(value); } }

	/// <summary>
	/// Ensure that we have a background and a foreground object to work with.
	/// </summary>

	void Awake ()
	{
		mTrans = transform;
		mCol = collider as BoxCollider;

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
	}

	/// <summary>
	/// We want to receive drag events from the thumb.
	/// </summary>

	void Start ()
	{
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
	/// Watch for slider value changes and adjust the visual sprite accordingly.
	/// </summary>

	void Update () { Set(rawValue); }

	/// <summary>
	/// Update the slider's position based on the mouse.
	/// </summary>

	void UpdateDrag ()
	{
		// Create a plane for the slider
		if (mCol == null) return;

		// Create a ray and a plane
		Ray ray = UICamera.lastCamera.ScreenPointToRay(UICamera.lastTouchPosition);
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

	/// <summary>
	/// Update the visible slider.
	/// </summary>

	void Set (float input)
	{
		// Clamp the input
		float val = Mathf.Clamp01(input);

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

			if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
			{
				eventReceiver.SendMessage(functionName, mStepValue, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}