using UnityEngine;

/// <summary>
/// When clicked, call the specified function on the target's attached scripts.
/// If no target was specified, it will use the game object this script is attached to.
/// DEPRECATED: This script has been deprecated as of version 1.30.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Send Message (OnClick)")]
public class UISendMessageOnClick : MonoBehaviour
{
	public GameObject target;
	public bool includeChildren = false;
	public string functionName = "OnSendMessage";

	void Start ()
	{
		Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " uses a deprecated script: " + GetType() +
			"\nConsider switching to UIButtonTween instead.");
	}

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