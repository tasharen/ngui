using UnityEngine;

/// <summary>
/// Will send out an OnState message to the specified target when OnTrigger event is received.
/// </summary>

[RequireComponent(typeof(Collider))]
[AddComponentMenu("NGUI/Examples/Trigger Set State")]
public class TriggerSetState : MonoBehaviour
{
	public GameObject target;
	public LayerMask mask = -1;
	public bool includeChildren = false;

	Collider mCol;
	int mCollisions = 0;

	void Awake ()
	{
		mCol = collider;
		mCol.isTrigger = true;
	}

	void OnTriggerEnter (Collider col)
	{
		int goMask = 1 << col.gameObject.layer;
		if ((mask & goMask) != 0 && ++mCollisions == 1) SetState(1);
	}

	void OnTriggerExit (Collider col)
	{
		int goMask = 1 << col.gameObject.layer;
		if ((mask & goMask) != 0 && --mCollisions == 0) SetState(0);
	}

	public void SetState (int state)
	{
		GameObject go = (target != null) ? target : gameObject;

		if (includeChildren)
		{
			Transform[] transforms = go.GetComponentsInChildren<Transform>();

			foreach (Transform t in transforms)
			{
				t.gameObject.SendMessage("OnState", state, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			go.SendMessage("OnState", state, SendMessageOptions.DontRequireReceiver);
		}
	}
}