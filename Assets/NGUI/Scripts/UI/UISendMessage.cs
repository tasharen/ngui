using UnityEngine;

/// <summary>
/// When clicked, call the specified function on the target's attached scripts.
/// If no target was specified, it will use the game object this script is attached to.
/// </summary>

[AddComponentMenu("UI/Send Message")]
public class UISendMessage : MonoBehaviour
{
	public GameObject target;
	public string functionName = "OnSendMessage";
	public bool includeChildren = false;

	void OnClick ()
	{
		GameObject go = (target != null) ? target : gameObject;

		if (includeChildren)
		{
			Transform[] transforms = go.GetComponentsInChildren<Transform>();

			foreach (Transform t in transforms)
			{
				t.gameObject.SendMessage(functionName, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			go.SendMessage(functionName, SendMessageOptions.DontRequireReceiver);
		}
	}
}