using UnityEngine;

/// <summary>
/// Same as UISendMessage, but passes the specified integer component when calling the target function.
/// </summary>

[AddComponentMenu("UI/Send Message (int)")]
public class UISendMessageInt : MonoBehaviour
{
	public GameObject target;
	public string functionName = "OnSendMessage";
	public int valueToSend = 0;
	public bool includeChildren = false;

	void OnClick ()
	{
		GameObject go = (target != null) ? target : gameObject;

		if (includeChildren)
		{
			Transform[] transforms = go.GetComponentsInChildren<Transform>();

			foreach (Transform t in transforms)
			{
				t.gameObject.SendMessage(functionName, valueToSend, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			go.SendMessage(functionName, valueToSend, SendMessageOptions.DontRequireReceiver);
		}
	}
}