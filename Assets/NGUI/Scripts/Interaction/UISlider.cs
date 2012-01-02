using UnityEngine;

/// <summary>
/// Simple slider functionality.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Slider")]
public class UISlider : MonoBehaviour
{
	public Transform foreground;

	public float initialValue = 1f;

	float mValue = 1f;
	Vector3 mScale = Vector3.one;
	Transform mForeTrans;

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
			mForeTrans = foreground.transform;
			mScale = foreground.localScale;
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

		// Reset the scale so TransformPoint calculations are correct
		mForeTrans.localScale = mScale;
		mValue = 1f;

		// Direction to the point on the plane in scaled local space
		Vector3 dir = mForeTrans.InverseTransformDirection(ray.GetPoint(dist) - mForeTrans.position);

		// Direction of the full slider in scaled local space
		Vector3 fullDir = mForeTrans.InverseTransformDirection(mForeTrans.TransformPoint(Vector3.right) - mForeTrans.TransformPoint(Vector3.zero));

		// Update the value of the slider
		sliderValue = dir.x / fullDir.x;
	}

	/// <summary>
	/// Update the visible slider.
	/// </summary>

	void UpdateSlider ()
	{
		Vector3 scale = mScale;
		scale.x *= mValue;
		mForeTrans.localScale = scale;
	}
}