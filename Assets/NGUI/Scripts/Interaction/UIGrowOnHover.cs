using UnityEngine;

/// <summary>
/// Simply grows the widget by the specified amount when the mouse is hovering over it.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Grow On Hover")]
public class UIGrowOnHover : MonoBehaviour
{
	public Vector3 amount = new Vector3(1.1f, 1.1f, 1.1f);
	public float animationSpeed = 8f;

	Transform mTrans;
	Vector3 mBaseScale;
	bool mMouseOver = false;

	void OnHover (bool isOver)
	{
		mMouseOver = isOver;
	}

	void Start ()
	{
		mTrans = transform;
		mBaseScale = mTrans.localScale;
	}

	void Update ()
	{
		Vector3 target = mBaseScale;
		
		if (mMouseOver)
		{
			target.x *= amount.x;
			target.y *= amount.y;
			target.z *= amount.z;
		}

		mTrans.localScale = Vector3.Lerp(mTrans.localScale, target, Time.deltaTime * animationSpeed);
	}
}