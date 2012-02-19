//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This script should be attached to each camera that's used to draw the objects with
/// UI components on them. This may mean only one camera (main camera or your UI camera),
/// or multiple cameras if you happen to have multiple viewports. Failing to attach this
/// script simply means that objects drawn by this camera won't receive UI notifications:
/// 
/// - OnHover (isOver) is sent when the mouse hovers over a collider or moves away.
/// - OnPress (isDown) is sent when a mouse button gets pressed on the collider.
/// - OnSelect (selected) is sent when a mouse button is released on the same object as it was pressed on.
/// - OnClick is sent with the same conditions as OnSelect, with the added check to see if the mouse has not moved much.
/// - OnDrag (delta) is sent when a mouse or touch gets pressed on a collider and starts dragging it.
/// - OnDrop (gameObject) is sent when the mouse or touch get released on a different collider than the one that was being dragged.
/// - OnInput (text) is sent when typing after selecting a collider by clicking on it.
/// - OnTooltip (show) is sent when the mouse hovers over a collider for some time without moving.
/// - OnScroll (float delta) is sent out when the mouse scroll wheel is moved.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Camera")]
[RequireComponent(typeof(Camera))]
public class UICamera : MonoBehaviour
{
	public class MouseOrTouch
	{
		public Vector3 pos;			// Current position of the mouse or touch event
		public Vector2 delta;		// Delta since last update
		public Vector2 totalDelta;	// Delta since the event started being tracked

		public Camera pressedCam;	// Camera that the OnPress(true) was fired with

		public GameObject current;	// The current game object under the touch or mouse
		public GameObject hover;	// The last game object to receive OnHover
		public GameObject pressed;	// The last game object to receive OnPress

		// Whether the touch is currently being considered for click events
		public bool considerForClick = false;
	}

	/// <summary>
	/// How long of a delay to expect before showing the tooltip.
	/// </summary>

	public float tooltipDelay = 1f;

	/// <summary>
	/// Name of the axis used for scrolling.
	/// </summary>

	public string scrollAxisName = "Mouse ScrollWheel";

	/// <summary>
	/// Last camera active prior to sending out the event. This will always be the camera that actually sent out the event.
	/// </summary>

	static public Camera lastCamera;

	/// <summary>
	/// Last raycast hit prior to sending out the event. This is useful if you want detailed information
	/// about what was actually hit in your OnClick, OnHover, and other event functions.
	/// </summary>
	
	static public RaycastHit lastHit;

	/// <summary>
	/// Last mouse or touch position in screen coordinates prior to sending out the event.
	/// </summary>

	static public Vector3 lastTouchPosition;

	/// <summary>
	/// ID of the touch or mouse operation prior to sending out the event. Mouse ID is '-1'.
	/// </summary>

	static public int lastTouchID = -1;

	/// <summary>
	/// If events don't get handled, they will be forwarded to this game object.
	/// </summary>

	static public GameObject fallThrough;

	// List of all active cameras in the scene
	static List<UICamera> mList = new List<UICamera>();

	// Selected widget (for input)
	static GameObject mSel = null;

	// Mouse event
	MouseOrTouch mMouse = new MouseOrTouch();

	// List of currently active touches
	Dictionary<int, MouseOrTouch> mTouches = new Dictionary<int, MouseOrTouch>();

	// Tooltip widget (mouse only)
	GameObject mTooltip = null;

	// Mouse input is turned off on iOS
	bool mUseMouseInput = true;
	Camera mCam = null;
	LayerMask mLayerMask;
	float mTooltipTime = 0f;

	/// <summary>
	/// Helper function that determines if this script should be handling the events.
	/// </summary>

	bool handlesEvents { get { return eventHandler == this; } }

	/// <summary>
	/// Caching is always preferable for performance.
	/// </summary>

	public Camera cachedCamera { get { if (mCam == null) mCam = camera; return mCam; } }

	/// <summary>
	/// Option to manually set the selected game object.
	/// </summary>

	static public GameObject selectedObject
	{
		get
		{
			return mSel;
		}
		set
		{
			if (mSel != value)
			{
				if (mSel != null)
				{
					UICamera uicam = FindCameraForLayer(mSel.layer);
					
					if (uicam != null)
					{
						lastCamera = uicam.mCam;
						mSel.SendMessage("OnSelect", false, SendMessageOptions.DontRequireReceiver);
					}
				}

				mSel = value;

				if (mSel != null)
				{
					UICamera uicam = FindCameraForLayer(mSel.layer);

					if (uicam != null)
					{
						lastCamera = uicam.mCam;
						mSel.SendMessage("OnSelect", true, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
	}

	/// <summary>
	/// Convenience function that returns the main HUD camera.
	/// </summary>

	static public Camera mainCamera
	{
		get
		{
			UICamera mouse = eventHandler;
			return (mouse != null) ? mouse.cachedCamera : null;
		}
	}

	/// <summary>
	/// Event handler for all types of events.
	/// </summary>

	static UICamera eventHandler
	{
		get
		{
			foreach (UICamera mouse in mList)
			{
				// Invalid or inactive entry -- keep going
				if (mouse == null || !mouse.enabled || !mouse.gameObject.active) continue;
				return mouse;
			}
			return null;
		}
	}

	/// <summary>
	/// Static comparison function used for sorting.
	/// </summary>

	static int CompareFunc (UICamera a, UICamera b)
	{
		if (a.cachedCamera.depth < b.cachedCamera.depth) return 1;
		if (a.cachedCamera.depth > b.cachedCamera.depth) return -1;
		return 0;
	}

	/// <summary>
	/// Returns the object under the specified position.
	/// </summary>

	static bool Raycast (Vector3 inPos, ref RaycastHit hit)
	{
		foreach (UICamera cam in mList)
		{
			// Skip inactive scripts
			if (!cam.enabled || !cam.gameObject.active) continue;

			// Convert to view space
			lastCamera = cam.cachedCamera;
			Vector3 pos = lastCamera.ScreenToViewportPoint(inPos);

			// If it's outside the camera's viewport, do nothing
			if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f) continue;

			// Cast a ray into the screen
			Ray ray = lastCamera.ScreenPointToRay(inPos);

			// Raycast into the screen
			if (Physics.Raycast(ray, out hit, lastCamera.farClipPlane - lastCamera.nearClipPlane, lastCamera.cullingMask)) return true;
		}
		return false;
	}

	/// <summary>
	/// Find the camera responsible for handling events on objects of the specified layer.
	/// </summary>

	static public UICamera FindCameraForLayer (int layer)
	{
		int layerMask = 1 << layer;

		foreach (UICamera cam in mList)
		{
			Camera uc = cam.cachedCamera;
			if ((uc != null) && (uc.cullingMask & layerMask) != 0) return cam;
		}
		return null;
	}

	/// <summary>
	/// Get or create a touch event.
	/// </summary>

	MouseOrTouch GetTouch (int id)
	{
		MouseOrTouch touch;

		if (!mTouches.TryGetValue(id, out touch))
		{
			touch = new MouseOrTouch();
			mTouches.Add(id, touch);
		}
		return touch;
	}

	/// <summary>
	/// Remove a touch event from the list.
	/// </summary>

	void RemoveTouch (int id)
	{
		mTouches.Remove(id);
	}

	/// <summary>
	/// Add this camera to the list.
	/// </summary>

	void Awake ()
	{
		// We should be using only touch-based input on Android and iOS-based devices.
		mUseMouseInput = Application.platform != RuntimePlatform.Android &&
						 Application.platform != RuntimePlatform.IPhonePlayer;

		if (mUseMouseInput) mMouse.pos = Input.mousePosition;

		// Add this camera to the list
		mList.Add(this);
		mList.Sort(CompareFunc);
	}

	/// <summary>
	/// Remove this camera from the list.
	/// </summary>

	void OnDestroy ()
	{
		mList.Remove(this);
	}

	/// <summary>
	/// Update the object under the mouse if we're not using touch-based input.
	/// </summary>

	void FixedUpdate ()
	{
		if (Application.isPlaying && mUseMouseInput && handlesEvents)
		{
			mMouse.current = Raycast(Input.mousePosition, ref lastHit) ? lastHit.collider.gameObject : fallThrough;
		}
	}

	/// <summary>
	/// Check the input and send out appropriate events.
	/// </summary>

	void Update ()
	{
		// Only the first UI layer should be processing events
		if (!Application.isPlaying || !handlesEvents) return;

		if (mUseMouseInput)
		{
			bool pressed = Input.GetMouseButtonDown(0);
			bool unpressed = Input.GetMouseButtonUp(0);

			lastTouchID = -1;
			lastTouchPosition = Input.mousePosition;
			mMouse.delta = lastTouchPosition - mMouse.pos;

			// We still want to update what's under the mouse even if the game is paused
			if (pressed || unpressed || Time.timeScale == 0f)
			{
				mMouse.current = Raycast(lastTouchPosition, ref lastHit) ? lastHit.collider.gameObject : fallThrough;
			}

			// We don't want to update the last camera while there is a touch happening
			if (pressed) mMouse.pressedCam = lastCamera;
			else if (mMouse.pressed != null) lastCamera = mMouse.pressedCam;

			if (mMouse.pos != lastTouchPosition)
			{
				if (mTooltipTime != 0f) mTooltipTime = Time.realtimeSinceStartup + tooltipDelay;
				else if (mTooltip != null) ShowTooltip(false);
				mMouse.pos = lastTouchPosition;
			}

			// Process the mouse events
			ProcessTouch(mMouse, pressed, unpressed);
		}

		// Process touch input
		if (Input.touchCount > 0)
		{
			foreach (Touch input in Input.touches)
			{
				lastTouchID = input.fingerId;
				MouseOrTouch touch = GetTouch(lastTouchID);

				bool pressed = (input.phase == TouchPhase.Began);
				bool unpressed = (input.phase == TouchPhase.Canceled) || (input.phase == TouchPhase.Ended);

				touch.pos = input.position;
				touch.delta = input.deltaPosition;
				lastTouchPosition = touch.pos;

				// Update the object under this touch
				if (pressed || unpressed)
				{
					touch.current = Raycast(input.position, ref lastHit) ? lastHit.collider.gameObject : fallThrough;
				}

				// We don't want to update the last camera while there is a touch happening
				if (pressed) touch.pressedCam = lastCamera;
				else if (touch.pressed != null) lastCamera = touch.pressedCam;

				// Process the events from this touch
				ProcessTouch(touch, pressed, unpressed);

				// If the touch has ended, remove it from the list
				if (unpressed) RemoveTouch(lastTouchID);
			}
		}

		// Forward the input to the selected object
		if (mSel != null)
		{
			string input = Input.inputString;

			// Adding support for some macs only having the "Delete" key instead of "Backspace"
			if (Input.GetKeyDown(KeyCode.Delete)) input += "\b";

			if (input.Length > 0)
			{
				if (mTooltip != null) ShowTooltip(false);
				mSel.SendMessage("OnInput", input, SendMessageOptions.DontRequireReceiver);
			}
		}

		// If it's time to show a tooltip, inform the object we're hovering over
		if (mUseMouseInput && mMouse.hover != null)
		{
			float scroll = Input.GetAxis(scrollAxisName);
			if (scroll != 0f) mMouse.hover.SendMessage("OnScroll", scroll, SendMessageOptions.DontRequireReceiver);

			if (mTooltipTime != 0f && mTooltipTime < Time.realtimeSinceStartup)
			{
				mTooltip = mMouse.hover;
				ShowTooltip(true);
			}
		}
	}

	/// <summary>
	/// Process the events of the specified touch.
	/// </summary>

	void ProcessTouch (MouseOrTouch touch, bool pressed, bool unpressed)
	{
		// If we're using the mouse for input, we should send out a hover(false) message first
		if (mUseMouseInput && touch.pressed == null && touch.hover != touch.current && touch.hover != null)
		{
			if (mTooltip != null) ShowTooltip(false);
			touch.hover.SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
		}

		// Send the drag notification, intentionally before the pressed object gets changed
		if (touch.pressed != null && touch.delta.magnitude != 0f)
		{
			if (mTooltip != null) ShowTooltip(false);
			touch.totalDelta += touch.delta;
			touch.pressed.SendMessage("OnDrag", touch.delta, SendMessageOptions.DontRequireReceiver);

			float threshold = (touch == mMouse) ? 5f : 30f;
			if (touch.totalDelta.magnitude > threshold) touch.considerForClick = false;
		}

		// Send out the press message
		if (pressed)
		{
			if (mTooltip != null) ShowTooltip(false);
			touch.pressed = touch.current;
			touch.considerForClick = true;
			touch.totalDelta = Vector2.zero;
			if (touch.pressed != null) touch.pressed.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
		}

		// Clear the selection
		if ((pressed && touch.pressed != mSel) || Input.GetKeyDown(KeyCode.Escape))
		{
			if (mTooltip != null) ShowTooltip(false);
			selectedObject = null;
		}

		// Send out the unpress message
		if (unpressed)
		{
			if (mTooltip != null) ShowTooltip(false);

			if (touch.pressed != null)
			{
				touch.pressed.SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);

				// If the button/touch was released on the same object, consider it a click and select it
				if (touch.pressed == touch.current)
				{
					if (touch.pressed != mSel)
					{
						mSel = touch.pressed;
						touch.pressed.SendMessage("OnSelect", true, SendMessageOptions.DontRequireReceiver);
					}
					else
					{
						mSel = touch.pressed;
					}
					if (touch.considerForClick) touch.pressed.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
				}
				else // The button/touch was released on a different object
				{
					// Send a drop notification (for drag & drop)
					if (touch.current != null) touch.current.SendMessage("OnDrop", touch.pressed, SendMessageOptions.DontRequireReceiver);

					// If we're using mouse-based input, send a hover notification
					if (mUseMouseInput) touch.pressed.SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
				}
			}
			touch.pressed = null;
			touch.hover = null;
		}

		// Send out a hover(true) message last
		if (mUseMouseInput && touch.pressed == null && touch.hover != touch.current)
		{
			mTooltipTime = Time.realtimeSinceStartup + tooltipDelay;
			touch.hover = touch.current;
			if (touch.hover != null) touch.hover.SendMessage("OnHover", true, SendMessageOptions.DontRequireReceiver);
		}
	}

	/// <summary>
	/// Show or hide the tooltip.
	/// </summary>

	void ShowTooltip (bool val)
	{
		mTooltipTime = 0f;
		mTooltip.SendMessage("OnTooltip", val, SendMessageOptions.DontRequireReceiver);
		if (!val) mTooltip = null;
	}
}