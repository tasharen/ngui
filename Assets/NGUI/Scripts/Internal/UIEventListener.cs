using UnityEngine;

/// <summary>
/// Event Hook class lets you easily add remote event listener functions to an object.
/// Example usage: UIEventListener.Add(gameObject).onClick += MyClickFunction;
/// </summary>

[AddComponentMenu("NGUI/Internal/Event Listener")]
public class UIEventListener : MonoBehaviour
{
	public delegate void VoidDelegate (GameObject go);
	public delegate void BoolDelegate (GameObject go, bool state);
	public delegate void VectorDelegate (GameObject go, Vector2 delta);
	public delegate void StringDelegate (GameObject go, string text);
	public delegate void ObjectDelegate (GameObject go, GameObject draggedObject);

	public VoidDelegate onClick;
	public BoolDelegate onHover;
	public BoolDelegate onPress;
	public BoolDelegate onSelect;
	public VectorDelegate onDrag;
	public ObjectDelegate onDrop;
	public StringDelegate onInput;

	void OnClick ()					{ if (onClick != null) onClick(gameObject); }
	void OnHover (bool isOver)		{ if (onHover != null) onHover(gameObject, isOver); }
	void OnPress (bool isPressed)	{ if (onPress != null) onPress(gameObject, isPressed); }
	void OnSelect (bool selected)	{ if (onSelect != null) onSelect(gameObject, selected); }
	void OnDrag (Vector2 delta)		{ if (onDrag != null) onDrag(gameObject, delta); }
	void OnDrop (GameObject go)		{ if (onDrop != null) onDrop(gameObject, go); }
	void OnInput (string text)		{ if (onInput != null) onInput(gameObject, text); }

	/// <summary>
	/// Add an event listener to the specified game object.
	/// </summary>

	static public UIEventListener Add (GameObject go)
	{
		UIEventListener listener = go.GetComponent<UIEventListener>();
		if (listener == null) listener = go.AddComponent<UIEventListener>();
		return listener;
	}
}