using UnityEngine;

/// <summary>
/// Simply spins the transform over time.
/// </summary>

[AddComponentMenu("UI/Spin")]
public class UISpin : MonoBehaviour
{
	public Vector3 rotationsPerSecond = new Vector3(0f, 0f, 0.1f);

	Transform mTrans;

	void Start () { mTrans = transform; }

	void Update ()
	{
		float delta = Time.deltaTime * Mathf.Rad2Deg * Mathf.PI * 2f;
		Quaternion offset = Quaternion.Euler(rotationsPerSecond * delta);
		mTrans.rotation = mTrans.rotation * offset;
	}
}