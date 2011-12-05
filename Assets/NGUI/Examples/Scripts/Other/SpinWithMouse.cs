using UnityEngine;

[AddComponentMenu("NGUI/Examples/Spin With Mouse")]
public class SpinWithMouse : MonoBehaviour
{
	Transform mTrans;

	void Start ()
	{
		mTrans = transform;
	}

	void OnDrag (Vector2 delta)
	{
		mTrans.localRotation = Quaternion.Euler(0f, -0.5f * delta.x, 0f) * mTrans.localRotation;
	}
}