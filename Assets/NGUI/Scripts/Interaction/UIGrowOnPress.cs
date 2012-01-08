using UnityEngine;

/// <summary>
/// Simply grows the widget by the specified amount when the mouse is pressed on it.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Grow On Press")]
public class UIGrowOnPress : MonoBehaviour
{
	public Vector3 amount = new Vector3(1.1f, 1.1f, 1.1f);
	public float animationSpeed = 8f;

	Transform mTrans;
	Vector3 mBaseScale;
	bool mPressed = false;

	void OnPress (bool isOver)
	{
		mPressed = isOver;
	}

	void OnDrag (Vector2 delta)
	{
		mPressed = (UICamera.lastHit.collider == collider);
	}

	void Start ()
	{
		mTrans = transform;
		mBaseScale = mTrans.localScale;
	}

	void Update ()
	{
		Vector3 target = mBaseScale;

		if (mPressed)
		{
			target.x *= amount.x;
			target.y *= amount.y;
			target.z *= amount.z;
		}

		mTrans.localScale = Vector3.Lerp(mTrans.localScale, target, Time.deltaTime * animationSpeed);
	}
}