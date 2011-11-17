using UnityEngine;

/// <summary>
/// Optional scene-wide UI properties used by the entire UI.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Config")]
public class UIConfig : MonoBehaviour
{
	static UIConfig mInstance = null;
	static public UIConfig instance { get { return mInstance; } }

	/// <summary>
	/// Set the instance reference.
	/// </summary>

	void Awake () { mInstance = this; }
}