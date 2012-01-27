using UnityEngine;

/// <summary>
/// Simple slider functionality.
/// </summary>

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
	public float initialValue = 1f;
	public GameObject eventReceiver;
	public string functionName = "OnSliderChange";

	float mValue = 1f;
	Vector3 mScale = Vector3.one;
	BoxCollider mCol;
	Transform mTrans;
	Transform mForeTrans;
	UIWidget mWidget;
	UIFilledSprite mSprite;

	/// <summary>
	/// Change the slider's value.
	/// </summary>

	public float sliderValue
	{
		get
		{
			return mValue;
		}
		set
		{
			float val = Mathf.Clamp01(value);

			if (mValue != val)
			{
				mValue = val;
				UpdateSlider();
			}
		}
	}

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
			mScale = foreground.localScale;
		}
		else if (mCol != null)
		{
			mScale = mCol.size;
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
		if (thumb != null && thumb.collider != null)
		{
			UIForwardEvents fe = thumb.gameObject.AddComponent<UIForwardEvents>();
			fe.target = gameObject;
			fe.onPress = true;
			fe.onDrag = true;
		}

		mValue = initialValue;
		UpdateSlider();
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
		sliderValue = (direction == Direction.Horizontal) ? dir.x / mCol.size.x : dir.y / mCol.size.y;
	}

	/// <summary>
	/// Update the visible slider.
	/// </summary>

	public void UpdateSlider ()
	{
		Vector3 scale = mScale;

		if (direction == Direction.Horizontal) scale.x *= mValue;
		else scale.y *= mValue;

		if (mSprite != null)
		{
			mSprite.fillAmount = mValue;
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
					case UIFilledSprite.FillDirection.TowardLeft:		pos.x = mScale.x - scale.x; break;
					case UIFilledSprite.FillDirection.TowardBottom:		pos.y = mScale.y - scale.y; break;
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
			eventReceiver.SendMessage(functionName, mValue, SendMessageOptions.DontRequireReceiver);
		}
	}
}