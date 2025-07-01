//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

public interface KeyEventBlocker
{
	public bool isActive { get; }

	public int priority { get; }

	public bool ReactsTo (KeyCode key, UIKeyBinding.Modifier modifier);
}

/// <summary>
/// This class makes it possible to activate or select something by pressing a key (such as space bar for example).
/// </summary>

[AddComponentMenu("NGUI/Interaction/Key Binding")]
#if TNET
public class UIKeyBinding : MonoBehaviour, TNet.IStartable, KeyEventBlocker
#else
public class UIKeyBinding : MonoBehaviour, KeyEventBlocker
#endif
{
	[System.NonSerialized] static public List<KeyEventBlocker> list = new List<KeyEventBlocker>();

	[DoNotObfuscateNGUI] public enum Action
	{
		PressAndClick,
		Select,
		All,
	}

	[DoNotObfuscateNGUI] public enum Modifier
	{
		Any,
		Shift,
		Ctrl,
		Alt,
		None,
	}

	[DoNotObfuscateNGUI] public enum ControlScheme
	{
		Any,
		MouseKeyboard,
		Controller,
	}

	[Tooltip("Key that will trigger the binding.")]
	public KeyCode keyCode = KeyCode.None;

	[Tooltip("Modifier key that must be active in order for the binding to trigger.")]
	public Modifier modifier = Modifier.Any;

	[Tooltip("Action to take with the specified key.")]
	public Action action = Action.PressAndClick;

	[Tooltip("Control scheme that this key binding reacts to.")]
	public ControlScheme controlScheme = ControlScheme.Any;

	[Tooltip("Higher value will block lower priority events. All events set to the same block priority will receive the event.")]
	public int blockPriority = 0;

	public int priority { get { return blockPriority; } }

#if W2
	[Tooltip("Visible virtual controller-based mouse should make all key binds inactive by default, unless specified otherwise.")]
	public bool controllerMouse = false;
#endif

	[System.NonSerialized] protected bool mIgnoreUp = false;
	[System.NonSerialized] protected bool mIsInput = false;
	[System.NonSerialized] protected bool mPress = false;
	[System.NonSerialized] protected float mLastClickTime = 0f;

	/// <summary>
	/// Key binding's descriptive caption.
	/// </summary>

	public virtual string captionText
	{
		get
		{
			if (keyCode == KeyCode.None) return "";
			var s = NGUITools.KeyToCaption(keyCode);
			if (modifier == Modifier.None || modifier == Modifier.Any) return s;
			return modifier + "+" + s;
		}
	}

	/// <summary>
	/// Check to see if the specified key happens to be bound to some element.
	/// </summary>

	static public bool IsBound (KeyCode key)
	{
		for (int i = 0, imax = list.Count; i < imax; ++i)
		{
			var kb = list[i];
			if (kb != null && kb.ReactsTo(key, Modifier.Any)) return true;
		}
		return false;
	}

#if TNET
	protected virtual void Awake () { TNet.TNUpdater.AddStart(this); }
#endif
	protected virtual void OnEnable () { list.Add(this); }
	protected virtual void OnDisable () { list.Remove(this); }

	/// <summary>
	/// If we're bound to an input field, subscribe to its Submit notification.
	/// </summary>

#if TNET
	public virtual void OnStart ()
#else
	protected virtual void Start ()
#endif
	{
		UIInput input;
		mIsInput = TryGetComponent(out input);
		if (mIsInput) EventDelegate.Add(input.onSubmit, OnSubmit);
	}

	/// <summary>
	/// Whether the keybinding should react to key events.
	/// </summary>

	protected virtual bool Poll (out bool down, out bool up)
	{
		down = false;
		up = false;

		if (keyCode != KeyCode.Numlock && UICamera.inputHasFocus) return false;
		if (keyCode == KeyCode.None || !IsModifierActive(modifier)) return false;
		if (UIDragDropItem.IsDragged(gameObject)) return false;
#if WINDWARD && UNITY_ANDROID
		// NVIDIA Shield controller has an odd bug where it can open the on-screen keyboard via a KeyCode.Return binding,
		// and then it can never be closed. I am disabling it here until I can track down the cause.
		if (keyCode == KeyCode.Return && PlayerPrefs.GetInt("Start Chat") == 0) return false;
#endif
		down = UICamera.GetKeyDown(keyCode);
		up = UICamera.GetKeyUp(keyCode);
		return down || up;
	}

	/// <summary>
	/// Whether this key binding reacts to the specified one.
	/// </summary>

	public virtual bool ReactsTo (KeyCode key, Modifier mod) { return keyCode == key && (modifier == Modifier.Any || mod == Modifier.Any || modifier == mod); }

	/// <summary>
	/// Ignore the KeyUp message if the input field "ate" it.
	/// </summary>

	protected virtual void OnSubmit () { if (UICamera.currentKey == keyCode && IsModifierActive(modifier)) mIgnoreUp = true; }

	/// <summary>
	/// Convenience function that checks whether the required modifier key is active.
	/// </summary>

	static public bool IsModifierActive (Modifier modifier)
	{
		if (modifier == Modifier.Any) return true;

		if (modifier == Modifier.Alt)
		{
			if (UICamera.GetKey(KeyCode.LeftAlt) ||
				UICamera.GetKey(KeyCode.RightAlt)) return true;
		}
		else if (modifier == Modifier.Ctrl)
		{
			if (UICamera.GetKey(KeyCode.LeftControl) ||
				UICamera.GetKey(KeyCode.RightControl)) return true;
		}
		else if (modifier == Modifier.Shift)
		{
			if (UICamera.GetKey(KeyCode.LeftShift) ||
				UICamera.GetKey(KeyCode.RightShift))
			{
				return true;
			}
		}
		else if (modifier == Modifier.None)
		{
			if (!UICamera.GetKey(KeyCode.LeftAlt) &&
				!UICamera.GetKey(KeyCode.RightAlt) &&
				!UICamera.GetKey(KeyCode.LeftControl) &&
				!UICamera.GetKey(KeyCode.RightControl) &&
				!UICamera.GetKey(KeyCode.LeftShift) &&
				!UICamera.GetKey(KeyCode.RightShift))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Whether the current control scheme is valid for this keybind.
	/// </summary>

	public virtual bool isActive
	{
		get
		{
			if (controlScheme != ControlScheme.Any)
			{
				if (controlScheme == ControlScheme.Controller) { if (UICamera.currentScheme != UICamera.ControlScheme.Controller) return false; }
				else if (controlScheme == ControlScheme.MouseKeyboard) { if (UICamera.currentScheme != UICamera.ControlScheme.Mouse) return false; }
			}

			if (UICamera.inputHasFocus || (UICamera.disableController && keyCode >= KeyCode.JoystickButton0)) return false;
#if W2
			if (!controllerMouse && UIControllerCursor.isVisible) return false;
#endif
#if TNET
			if (TNet.UIOptionsWindow.current != null) return false;
			if (TNet.UIOptionsWindow.isPerformingKeyBind) return false;
#endif
			return true;
		}
	}

	/// <summary>
	/// Process the key binding.
	/// </summary>

	protected virtual void Update ()
	{
		if (isActive)
		{
			bool keyDown, keyUp;
			if (Poll(out keyDown, out keyUp)) React(keyDown, keyUp);
		}
	}

	/// <summary>
	/// React to key down or key up.
	/// </summary>

	protected virtual void React (bool keyDown, bool keyUp)
	{
		if (blockPriority != 0)
		{
			// Blocked by another higher value blocking event
			foreach (var kb in list)
			{
				if (kb.ReactsTo(keyCode, modifier))
				{
					if (kb.priority > blockPriority && kb.isActive) return;
				}
			}
		}
		else
		{
			// If there is a blocking event, don't proceed
			foreach (var kb in list)
			{
				if (kb.ReactsTo(keyCode, modifier))
				{
					if (kb.priority != 0 && kb.isActive) return;
				}
			}
		}

		if (keyDown) mPress = true;

		if (action == Action.PressAndClick || action == Action.All)
		{
			if (keyDown)
			{
				UICamera.currentTouchID = -1;
				UICamera.currentKey = keyCode;
				OnBindingPress(true);
			}

			if (mPress && keyUp)
			{
				UICamera.currentTouchID = -1;
				UICamera.currentKey = keyCode;
				OnBindingPress(false);
				OnBindingClick();

				if (mLastClickTime != 0f && Time.unscaledTime - mLastClickTime < 0.35f)
				{
					mLastClickTime = 0f;
					OnBindingDoubleClick();
				}
				else mLastClickTime = Time.unscaledTime;
			}
		}

		if (action == Action.Select || action == Action.All)
		{
			if (keyUp)
			{
				if (mIsInput)
				{
					if (!mIgnoreUp && !(keyCode != KeyCode.Numlock && UICamera.inputHasFocus))
					{
						if (mPress) UICamera.selectedObject = gameObject;
					}
					mIgnoreUp = false;
				}
				else if (mPress)
				{
					UICamera.hoveredObject = gameObject;
				}
			}
		}

		if (keyUp) mPress = false;
	}

	protected virtual void OnBindingPress (bool pressed) { UICamera.Notify(gameObject, "OnPress", pressed); }
	protected virtual void OnBindingClick () { UICamera.Notify(gameObject, "OnClick", null); }
	protected virtual void OnBindingDoubleClick () { UICamera.Notify(gameObject, "OnDoubleClick", null); }

	/// <summary>
	/// Convert the key binding to its text format.
	/// </summary>

	public override string ToString () { return GetString(keyCode, modifier); }

	/// <summary>
	/// Convert the key binding to its text format.
	/// </summary>

	static public string GetString (KeyCode keyCode, Modifier modifier)
	{
		if (keyCode == KeyCode.None) return "none";
		return (modifier != Modifier.None) ? modifier + "+" + NGUITools.KeyToCaption(keyCode) : NGUITools.KeyToCaption(keyCode);
	}

	/// <summary>
	/// Given the ToString() text, parse it for key and modifier information.
	/// </summary>

	static public bool GetKeyCode (string text, out KeyCode key, out Modifier modifier)
	{
		key = KeyCode.None;
		modifier = Modifier.None;
		if (string.IsNullOrEmpty(text) || text == "none") return true;

		if (text.Length > 2 && text.Contains("+") && text[text.Length - 1] != '+')
		{
			var parts = text.Split(new char[] { '+' }, 2);
			key = NGUITools.CaptionToKey(parts[1]);
			try { modifier = (Modifier)System.Enum.Parse(typeof(Modifier), parts[0]); }
			catch (System.Exception) { return false; }
		}
		else
		{
			modifier = Modifier.None;
			key = NGUITools.CaptionToKey(text);
		}
		return key != KeyCode.None;
	}

	/// <summary>
	/// Get the currently active key modifier, if any.
	/// </summary>

	static public Modifier GetActiveModifier ()
	{
		var mod = Modifier.None;
		if (UICamera.GetKey(KeyCode.LeftAlt) || UICamera.GetKey(KeyCode.RightAlt)) mod = Modifier.Alt;
		else if (UICamera.GetKey(KeyCode.LeftShift) || UICamera.GetKey(KeyCode.RightShift)) mod = Modifier.Shift;
		else if (UICamera.GetKey(KeyCode.LeftControl) || UICamera.GetKey(KeyCode.RightControl)) mod = Modifier.Ctrl;
		return mod;
	}
}
