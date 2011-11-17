using UnityEngine;

/// <summary>
/// Attach to a game object to make its rotation always lag behind its parent as the parent rotates.
/// </summary>

[AddComponentMenu("Misc/Lag Rotation")]
public class LagRotation : MonoBehaviour
{
	public int level = 0;
	public float speed = 10f;
	
	Transform mTrans;
	Quaternion mRelative;
	Quaternion mAbsolute;
	
	void Start()
	{
		mTrans = transform;
		mRelative = mTrans.localRotation;
		mAbsolute = mTrans.rotation;
		UpdateManager.AddLateUpdate(level, this, OnLateUpdate);
	}

	bool OnLateUpdate()
	{
		Transform parent = mTrans.parent;
		
		if (parent != null)
		{
			mAbsolute = Quaternion.Slerp(mAbsolute, parent.rotation * mRelative, Time.deltaTime * speed);
			mTrans.rotation = mAbsolute;
		}
		return true;
	}
}