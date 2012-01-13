using UnityEngine;

/// <summary>
/// When clicked, call the specified function on the target's attached scripts.
/// If no target was specified, it will use the game object this script is attached to.
/// DEPRECATED: This script has been deprecated as of version 1.30.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Deprecated/Send Message (OnClick)")]
public class UISendMessageOnClick : MonoBehaviour
{
	public GameObject target;
	public bool includeChildren = false;
	public string functionName = "OnSendMessage";

	void Awake ()
	{
		Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " uses a deprecated script: " + GetType() +
			"\nUpgrading to UIButtonMessage.", this);

		UIButtonMessage bm = gameObject.AddComponent<UIButtonMessage>();
		bm.functionName = functionName;
		bm.target = target;
		bm.trigger = UIButtonMessage.Trigger.OnClick;
		bm.includeChildren = includeChildren;

		DestroyImmediate(this);
	}
}