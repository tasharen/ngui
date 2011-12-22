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

		public RaycastHit current;	// The current game object under the touch or mouse
		public RaycastHit hover;	// The last game object to receive OnHover
		public RaycastHit pressed;	// The last game object to receive OnPress

		// Whether the touch is currently being considered for click events
		public bool considerForClick = false;
	}

	public float tooltipDelay = 1f;

	// List of all active cameras in the scene
	static List<UICamera> mList = new List<UICamera>();

	// Empty raycast hit used to avoid 'new' calls all the time.
	static RaycastHit mNullHit = new RaycastHit();

	// Mouse event
	MouseOrTouch mMouse = new MouseOrTouch();

	// List of currently active touches
	Dictionary<int, MouseOrTouch> mTouches = new Dictionary<int, MouseOrTouch>();

	// Selected widget (for input)
	GameObject mSel = null;

	// Tooltip widget (mouse only)
	GameObject mTooltip = null;

	bool mUseTouchInput = false;
	Camera mCam = null;
	LayerMask mLayerMask;
	float mTooltipTime = 0f;

	/// <summary>
	/// Caching is always preferable for performance.
	/// </summary>

	public Camera cachedCamera { get { if (mCam == null) mCam = camera; return mCam; } }

	/// <summary>
	/// Helper function that determines if this script should be handling the events.
	/// </summary>

	bool handlesEvents { get { return eventHandler == this; } }

	/// <summary>
	/// Current mouse touch.
	/// </summary>

	public MouseOrTouch mouse { get { return mMouse; } }

	/// <summary>
	/// Access to current touch events if additional info is required (such as HitInfo).
	/// </summary>

	public Dictionary<int, MouseOrTouch> touches { get { return mTouches; } }

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

	static public UICamera eventHandler
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

	static bool GetHitInfo (Vector3 inPos, ref RaycastHit hit)
	{
		foreach (UICamera mouse in mList)
		{
			// Skip inactive scripts
			if (!mouse.enabled || !mouse.gameObject.active) continue;

			// Convert to view space
			Camera cam = mouse.cachedCamera;
			Vector3 pos = cam.ScreenToViewportPoint(inPos);

			// If it's outside the camera's viewport, do nothing
			if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f) continue;

			// Cast a ray into the screen
			Ray ray = cam.ScreenPointToRay(inPos);

			// Raycast into the screen
			return Physics.Raycast(ray, out hit, cam.farClipPlane - cam.nearClipPlane, mouse.cachedCamera.cullingMask);
		}
		return false;
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

	void Start ()
	{
		// We should be using touch-based input on Android and iOS-based devices.
		mUseTouchInput = Application.platform == RuntimePlatform.Android ||
						 Application.platform == RuntimePlatform.IPhonePlayer;

		if (!mUseTouchInput) mMouse.pos = Input.mousePosition;

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
		if (Application.isPlaying && !mUseTouchInput && handlesEvents)
		{
			GetHitInfo(Input.mousePosition, ref mMouse.current);
		}
	}

	/// <summary>
	/// Check the input and send out appropriate events.
	/// </summary>

	void Update ()
	{
		// Only the first UI layer should be processing events
		if (!Application.isPlaying || !handlesEvents) return;

		if (mUseTouchInput)
		{
			if (Input.touchCount > 0)
			{
				foreach (Touch input in Input.touches)
				{
					MouseOrTouch touch = GetTouch(input.fingerId);

					bool pressed = (input.phase == TouchPhase.Began);
					bool unpressed = (input.phase == TouchPhase.Canceled) || (input.phase == TouchPhase.Ended);

					touch.pos = input.position;
					touch.delta = input.deltaPosition;

					// Update the object under this touch
					if (pressed || unpressed) GetHitInfo(input.position, ref touch.current);

					// Process the events from this touch
					ProcessTouch(touch, pressed, unpressed);

					// If the touch has ended, remove it from the list
					if (unpressed) RemoveTouch(input.fingerId);
				}
			}
		}
		else
		{
			bool pressed = Input.GetMouseButtonDown(0);
			bool unpressed = Input.GetMouseButtonUp(0);

			Vector3 pos = Input.mousePosition;
			mMouse.delta = pos - mMouse.pos;

			if (mMouse.pos != pos)
			{
				if (mTooltipTime != 0f) mTooltipTime = Time.time + tooltipDelay;
				else if (mTooltip != null) ShowTooltip(false);
				mMouse.pos = pos;
			}

			// Process the mouse events
			ProcessTouch(mMouse, pressed, unpressed);
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
				mSel.collider.SendMessage("OnInput", input, SendMessageOptions.DontRequireReceiver);
			}
		}

		// If it's time to show a tooltip, inform the object we're hovering over
		if (!mUseTouchInput && mMouse.hover.collider != null && mTooltipTime != 0f && mTooltipTime < Time.time)
		{
			mTooltip = mMouse.hover.collider.gameObject;
			ShowTooltip(true);
		}
	}

	/// <summary>
	/// Process the events of the specified touch.
	/// </summary>

	void ProcessTouch (MouseOrTouch touch, bool pressed, bool unpressed)
	{
		// If we're using the mouse for input, we should send out a hover(false) message first
		if (!mUseTouchInput &&
			(touch.pressed.collider == null) &&
			(touch.hover.collider != null) &&
			(touch.hover.collider != touch.current.collider))
		{
			if (mTooltip != null) ShowTooltip(false);
			touch.hover.collider.SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
		}

		// Send the drag notification, intentionally before the pressed object gets changed
		if (touch.pressed.collider != null && touch.delta.magnitude != 0f)
		{
			if (mTooltip != null) ShowTooltip(false);
			touch.totalDelta += touch.delta;
			touch.pressed.collider.SendMessage("OnDrag", touch.delta, SendMessageOptions.DontRequireReceiver);

			float threshold = mUseTouchInput ? 30f : 5f;
			if (touch.totalDelta.magnitude > threshold) touch.considerForClick = false;
		}

		// Send out the press message
		if (pressed)
		{
			if (mTooltip != null) ShowTooltip(false);
			touch.pressed = touch.current;
			touch.considerForClick = true;
			touch.totalDelta = Vector3.zero;
			if (touch.pressed.collider != null) touch.pressed.collider.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
		}

		// Clear the selection
		if ((mSel != null) && (pressed || Input.GetKeyDown(KeyCode.Escape)))
		{
			if (mTooltip != null) ShowTooltip(false);
			mSel.collider.SendMessage("OnSelect", false, SendMessageOptions.DontRequireReceiver);
			mSel = null;
		}

		// Send out the unpress message
		if (unpressed)
		{
			if (mTooltip != null) ShowTooltip(false);

			if (touch.pressed.collider != null)
			{
				touch.pressed.collider.SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);

				// If the button/touch was released on the same object, consider it a click and select it
				if (touch.pressed.collider == touch.current.collider)
				{
					mSel = touch.pressed.collider.gameObject;
					touch.pressed.collider.SendMessage("OnSelect", true, SendMessageOptions.DontRequireReceiver);
					if (touch.considerForClick) touch.pressed.collider.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
				}
				else // The button/touch was released on a different object
				{
					// Send a drop notification (for drag & drop)
					if (touch.current.collider != null) touch.current.collider.SendMessage("OnDrop", touch.pressed.collider.gameObject, SendMessageOptions.DontRequireReceiver);

					// If we're using mouse-based input, send a hover notification
					if (!mUseTouchInput) touch.pressed.collider.SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
				}
			}
			touch.pressed = mNullHit;
			touch.hover = mNullHit;
		}

		// Send out a hover(true) message last
		if (!mUseTouchInput && (touch.pressed.collider == null) && (touch.hover.collider != touch.current.collider))
		{
			mTooltipTime = Time.time + tooltipDelay;
			touch.hover = touch.current;
			if (touch.hover.collider != null) touch.hover.collider.SendMessage("OnHover", true, SendMessageOptions.DontRequireReceiver);
		}
	}

	/// <summary>
	/// Show or hide the tooltip.
	/// </summary>

	void ShowTooltip (bool val)
	{
		mTooltipTime = 0f;
		mTooltip.collider.SendMessage("OnTooltip", val, SendMessageOptions.DontRequireReceiver);
		if (!val) mTooltip = null;
	}
}