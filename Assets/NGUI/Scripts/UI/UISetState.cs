using UnityEngine;

/// <summary>
/// Calls "OnState" function on all of the scripts attached to the specified target (or this game object
/// if none was specified). Use it in conjunction with scripts like UIStateColor, UIStatePosition,
/// and UIStateRotation to set up click-triggered transitions, such as showing/hiding windows, moving
/// game objects or UI components around, changing color, and more.
/// </summary>

[AddComponentMenu("NGUI/Send State")]
public class UISendState : MonoBehaviour
{
	public GameObject target;
	public int state = 0;
	public bool includeChildren = false;

	void OnClick ()
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