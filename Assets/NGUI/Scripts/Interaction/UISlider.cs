using UnityEngine;

/// <summary>
/// Simple slider functionality.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Slider")]
public class UISlider : MonoBehaviour
{
	public Camera cameraToUse;
	public Transform foreground;

	public float sliderValue = 1f;

	float mScale = 1f;
	Transform mTrans;

	/// <summary>
	/// Ensure that we have a background and a foreground object to work with.
	/// </summary>

	void Start ()
	{
		if (foreground == null)
		{
			Debug.LogWarning("Expected to find an object to scale on " + NGUITools.GetHierarchy(gameObject));
			Destroy(this);
		}
		else
		{
			mTrans = foreground.transform;
			mScale = foreground.localScale.x;
		}
	}

	/// <summary>
	/// Scale the foreground based on the slider's value.
	/// </summary>

	void Update ()
	{
		if (foreground != null)
		{
			Vector3 scale = foreground.localScale;
			scale.x = mScale * sliderValue;
			foreground.localScale = scale;
		}
	}

	/// <summary>
	/// Update the slider's position on press.
	/// </summary>

	void OnPress (bool pressed)
	{
		if (pressed) UpdateSlider();
	}

	/// <summary>
	/// When dragged, figure out where the mouse is and calculate the updated value of the slider.
	/// </summary>

	void OnDrag (Vector2 delta)
	{
		UpdateSlider();
	}

	/// <summary>
	/// Update the slider's position based on the mouse.
	/// </summary>

	void UpdateSlider ()
	{
		if (cameraToUse == null) cameraToUse = UICamera.mainCamera;
		Ray ray = cameraToUse.ScreenPointToRay(Input.mousePosition);

		Plane plane = new Plane(mTrans.rotation * Vector3.back, mTrans.position);

		float dist;
		if (!plane.Raycast(ray, out dist)) return;

		Vector3 intersect = ray.origin + ray.direction * dist;
		Vector3 dir = intersect - mTrans.position;

		Quaternion inv = Quaternion.Inverse(mTrans.rotation);
		dir = inv * dir;

		Vector3 range = inv * mTrans.TransformDirection(Vector3.right);
		sliderValue = Mathf.Clamp01(dir.x / range.x);
	}
}