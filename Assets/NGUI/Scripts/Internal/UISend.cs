using UnityEngine;

/// <summary>
/// Internal abstract script. Has common functionality used by UISend series of scripts.
/// </summary>

public abstract class UISend : MonoBehaviour
{
	public GameObject target;
	public bool includeChildren = false;

	protected void Send (int state)
	{
		Send("OnState", state);
	}

	protected void Send (string funcName, int state)
	{
		GameObject go = (target != null) ? target : gameObject;

		if (includeChildren)
		{
			Transform[] transforms = go.GetComponentsInChildren<Transform>();

			foreach (Transform t in transforms)
			{
				t.gameObject.SendMessage(funcName, state, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			go.SendMessage(funcName, state, SendMessageOptions.DontRequireReceiver);
		}
	}
}