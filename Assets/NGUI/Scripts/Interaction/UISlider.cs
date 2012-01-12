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

	public Direction direction = Direction.Horizontal;
	public float initialValue = 1f;

	float mValue = 1f;
	Vector3 mScale = Vector3.one;
	BoxCollider mCol;
	Transform mTrans;
	Transform mForeTrans;
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
		if (foreground == null)
		{
			Debug.LogWarning("Expected to find an object to scale on " + NGUITools.GetHierarchy(gameObject));
			Destroy(this);
		}
		else
		{
			mSprite = foreground.GetComponent<UIFilledSprite>();
			mForeTrans = foreground.transform;
			mScale = foreground.localScale;
			mCol = collider as BoxCollider;
			mTrans = transform;
		}
	}

	/// <summary>
	/// Make the slider show the specified initial value.
	/// </summary>

	void Start ()
	{
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
		if (mForeTrans == null) return;
		Ray ray = UICamera.lastCamera.ScreenPointToRay(UICamera.lastTouchPosition);
		Plane plane = new Plane(mForeTrans.rotation * Vector3.back, mForeTrans.position);

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

	void UpdateSlider ()
	{
		if (mSprite != null)
		{
			mSprite.fillAmount = mValue;
		}
		else
		{
			Vector3 scale = mScale;
			if (direction == Direction.Horizontal) scale.x *= mValue;
			else scale.y *= mValue;
			mForeTrans.localScale = scale;
		}
	}
}