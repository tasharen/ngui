//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This script should be attached to each camera that's used to draw the objects with
/// UI components on them. This may mean only one camera (main camera or your UI camera),
/// or multiple cameras if you happen to have multiple viewports. Failing to attach this
/// script simply means that objects drawn by this camera won't receive UI notifications:
///
/// * OnHover (isOver) is sent when the mouse hovers over a collider or moves away.
/// * OnPress (isDown) is sent when a mouse button gets pressed on the collider.
/// * OnSelect (selected) is sent when a mouse button is first pressed on an object. Repeated presses won't result in an OnSelect(true).
/// * OnClick () is sent when a mouse is pressed and released on the same object.
///   UICamera.currentTouchID tells you which button was clicked.
/// * OnDoubleClick () is sent when the click happens twice within a fourth of a second.
///   UICamera.currentTouchID tells you which button was clicked.
///
/// * OnDragStart () is sent to a game object under the touch just before the OnDrag() notifications begin.
/// * OnDrag (delta) is sent to an object that's being dragged.
/// * OnDragOver (draggedObject) is sent to a game object when another object is dragged over its area.
/// * OnDragOut (draggedObject) is sent to a game object when another object is dragged out of its area.
/// * OnDragEnd () is sent to a dragged object when the drag event finishes.
///
/// * OnTooltip (show) is sent when the mouse hovers over a collider for some time without moving.
/// * OnScroll (float delta) is sent out when the mouse scroll wheel is moved.
/// * OnNavigate (KeyCode key) is sent when horizontal or vertical navigation axes are moved.
/// * OnPan (Vector2 delta) is sent when when horizontal or vertical panning axes are moved.
/// * OnKey (KeyCode key) is sent when keyboard or controller input is used.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Event System (UICamera)")]
[RequireComponent(typeof(Camera))]
public class UICamera : MonoBehaviour
{
	[DoNotObfuscateNGUI] public enum ControlScheme
	{
		Mouse,				// Mouse and keyboard input
		Touch,				// Touchscreen-based input
		Controller,			// Joystick or another similar controller type
	}

	/// <summary>
	/// Whether the touch event will be sending out the OnClick notification at the end.
	/// </summary>

	[DoNotObfuscateNGUI] public enum ClickNotification
	{
		None,
		Always,
		BasedOnDelta,
	}

	/// <summary>
	/// Ambiguous mouse, touch, or controller event.
	/// </summary>

	public class MouseOrTouch
	{
		public KeyCode key = KeyCode.None;
		public Vector2 pos;				// Current position of the mouse or touch event
		public Vector2 lastPos;			// Previous position of the mouse or touch event
		public Vector2 delta;			// Delta since last update
		public Vector2 totalDelta;		// Delta since the event started being tracked

		public Camera pressedCam;		// Camera that the OnPress(true) was fired with

		public GameObject last;			// Last object under the touch or mouse
		public GameObject current;		// Current game object under the touch or mouse
		public GameObject pressed;		// Last game object to receive OnPress
		public GameObject dragged;		// Game object that's being dragged
		public GameObject lastClickGO;	// Last game object that received a click event (used for double click)

		public float pressTime = 0f;	// When the touch event started
		public float clickTime = 0f;    // The last time a click event was sent out
		public float rayTime = 0f;      // Used to track the time of the last performed raycast

		public ClickNotification clickNotification = ClickNotification.Always;
		public bool touchBegan = true;
		public bool pressStarted = false;
		public bool dragStarted = false;
		public bool longPress = false;
		public int ignoreDelta = 0;

		/// <summary>
		/// Delta time since the touch operation started.
		/// </summary>

		public float deltaTime { get { return RealTime.time - pressTime; } }

		/// <summary>
		/// Returns whether this touch is currently over a UI element.
		/// </summary>

		public bool isOverUI
		{
			get
			{
				return current != null && current != fallThrough && NGUITools.FindInParents<UIRoot>(current) != null;
			}
		}
	}

	/// <summary>
	/// Camera type controls how raycasts are handled by the UICamera.
	/// </summary>

	[DoNotObfuscateNGUI] public enum EventType : int
	{
		World_3D,	// Perform a Physics.Raycast and sort by distance to the point that was hit.
		UI_3D,		// Perform a Physics.Raycast and sort by widget depth.
		World_2D,	// Perform a Physics2D.OverlapPoint
		UI_2D,		// Physics2D.OverlapPoint then sort by widget depth
	}

	/// <summary>
	/// List of all active cameras in the scene.
	/// </summary>

	static public BetterList<UICamera> list = new BetterList<UICamera>();

	public delegate bool GetKeyStateFunc (KeyCode key);
	public delegate float GetAxisFunc (string name);
	public delegate bool GetAnyKeyFunc ();
	public delegate MouseOrTouch GetMouseDelegate (int button);
	public delegate MouseOrTouch GetTouchDelegate (int id, bool createIfMissing);
	public delegate void RemoveTouchDelegate (int id);

	/// <summary>
	/// GetKeyDown function -- return whether the specified key was pressed this Update().
	/// </summary>

	static public GetKeyStateFunc GetKeyDown = delegate(KeyCode key)
	{
		if (key >= KeyCode.JoystickButton0 && ignoreControllerInput) return false;
		return Input.GetKeyDown(key);
	};

	/// <summary>
	/// GetKeyDown function -- return whether the specified key was released this Update().
	/// </summary>

	static public GetKeyStateFunc GetKeyUp = delegate(KeyCode key)
	{
		if (key >= KeyCode.JoystickButton0 && ignoreControllerInput) return false;
		return Input.GetKeyUp(key);
	};

	/// <summary>
	/// GetKey function -- return whether the specified key is currently held.
	/// </summary>

	static public GetKeyStateFunc GetKey = delegate(KeyCode key)
	{
		if (key >= KeyCode.JoystickButton0 && ignoreControllerInput) return false;
		return Input.GetKey(key);
	};

	/// <summary>
	/// GetAxis function -- return the state of the specified axis.
	/// </summary>

	static public GetAxisFunc GetAxis = delegate(string axis)
	{
		if (ignoreControllerInput) return 0f;
		return Input.GetAxis(axis);
	};

	/// <summary>
	/// Convenience function that returns a vec2 for a pair of axes and makes sure the result is normalized if it's above 1.
	/// </summary>

	static public Vector2 GetAxes (string x, string y)
	{
		var v = new Vector2(GetAxis(x), GetAxis(y));
		var mag = v.sqrMagnitude;
		if (mag > 1f) v /= Mathf.Sqrt(mag);
		return v;
	}

	/// <summary>
	/// User-settable Input.anyKeyDown
	/// </summary>

	static public GetAnyKeyFunc GetAnyKeyDown = delegate () { return Input.anyKeyDown; };

	/// <summary>
	/// Get the details of the specified mouse button.
	/// </summary>

	static public GetMouseDelegate GetMouse = delegate(int button) { return mMouse[button]; };

	/// <summary>
	/// Get or create a touch event. If you are trying to iterate through a list of active touches, use activeTouches instead.
	/// </summary>

	static public GetTouchDelegate GetTouch = delegate(int id, bool createIfMissing)
	{
		if (id < 0) return GetMouse(-id - 1);

		for (int i = 0, imax = mTouchIDs.Count; i < imax; ++i)
			if (mTouchIDs[i] == id) return activeTouches[i];

		if (createIfMissing)
		{
			var touch = new MouseOrTouch();
			touch.pressTime = RealTime.time;
			touch.touchBegan = true;
			activeTouches.Add(touch);
			mTouchIDs.Add(id);
			return touch;
		}
		return null;
	};

	/// <summary>
	/// Remove a touch event from the list.
	/// </summary>

	static public RemoveTouchDelegate RemoveTouch = delegate(int id)
	{
		for (int i = 0, imax = mTouchIDs.Count; i < imax; ++i)
		{
			if (mTouchIDs[i] == id)
			{
				mTouchIDs.RemoveAt(i);
				activeTouches.RemoveAt(i);
				return;
			}
		}
	};

	/// <summary>
	/// Delegate triggered when the screen size changes for any reason.
	/// Subscribe to it if you don't want to compare Screen.width and Screen.height each frame.
	/// </summary>

	static public OnScreenResize onScreenResize;
	public delegate void OnScreenResize ();

	/// <summary>
	/// Event type -- use "UI" for your user interfaces, and "World" for your game camera.
	/// This setting changes how raycasts are handled. Raycasts have to be more complicated for UI cameras.
	/// </summary>

	public EventType eventType = EventType.UI_3D;

	/// <summary>
	/// By default, events will go to rigidbodies when the Event Type is not UI.
	/// You can change this behaviour back to how it was pre-3.7.0 using this flag.
	/// </summary>

	public bool eventsGoToColliders = false;

	/// <summary>
	/// Which layers will receive events.
	/// </summary>

	public LayerMask eventReceiverMask = -1;

	[DoNotObfuscateNGUI] public enum ProcessEventsIn
	{
		Update,
		LateUpdate,
	}

	/// <summary>
	/// When events will be processed.
	/// </summary>

	public ProcessEventsIn processEventsIn = ProcessEventsIn.Update;

	/// <summary>
	/// If 'true', currently hovered object will be shown in the top left corner.
	/// </summary>

	public bool debug = false;

	/// <summary>
	/// Whether the mouse input is used.
	/// </summary>

	public bool useMouse = true;

	/// <summary>
	/// Whether the touch-based input is used.
	/// </summary>

	public bool useTouch = true;

	/// <summary>
	/// Whether multi-touch is allowed.
	/// </summary>

	public bool allowMultiTouch = true;

	/// <summary>
	/// Whether the keyboard events will be processed.
	/// </summary>

	public bool useKeyboard = true;

	/// <summary>
	/// Whether the joystick and controller events will be processed.
	/// </summary>

	public bool useController = true;

	[System.Obsolete("Use new OnDragStart / OnDragOver / OnDragOut / OnDragEnd events instead")]
	public bool stickyPress { get { return true; } }

	/// <summary>
	/// Whether the tooltip will disappear as soon as the mouse moves (false) or only if the mouse moves outside of the widget's area (true).
	/// </summary>

	public bool stickyTooltip = true;

	[Tooltip("How long a key has to be held in order to qualify for an OnLongPress() event.")]
	public float longPressDelay = 0.5f;

	[Tooltip("If enabled, a tooltip will be shown at the same time as the OnLongPress() notification.")]
	public bool longPressTooltip = false;

	[Tooltip("How long of a delay to expect before showing the tooltip. Only valid for mouse events.")]
	public float tooltipDelay = 1f;

	/// <summary>
	/// How much the mouse has to be moved after pressing a button before it starts to send out drag events.
	/// </summary>

	public float mouseDragThreshold = 4f;

	/// <summary>
	/// How far the mouse is allowed to move in pixels before it's no longer considered for click events, if the click notification is based on delta.
	/// </summary>

	public float mouseClickThreshold = 10f;

	/// <summary>
	/// How much the mouse has to be moved after pressing a button before it starts to send out drag events.
	/// </summary>

	public float touchDragThreshold = 40f;

	/// <summary>
	/// How far the touch is allowed to move in pixels before it's no longer considered for click events, if the click notification is based on delta.
	/// </summary>

	public float touchClickThreshold = 40f;

	/// <summary>
	/// Raycast range distance. By default it's as far as the camera can see.
	/// </summary>

	public float rangeDistance = -1f;

	/// <summary>
	/// Name of the axis used to send left and right key events.
	/// </summary>

	public string horizontalAxisName = "Horizontal";

	/// <summary>
	/// Name of the axis used to send up and down key events.
	/// </summary>

	public string verticalAxisName = "Vertical";

	/// <summary>
	/// Name of the horizontal axis used to move scroll views and sliders around.
	/// </summary>

	public string horizontalPanAxisName = null;

	/// <summary>
	/// Name of the vertical axis used to move scroll views and sliders around.
	/// </summary>

	public string verticalPanAxisName = null;

	/// <summary>
	/// Name of the axis used for scrolling.
	/// </summary>

	public string scrollAxisName = "Mouse ScrollWheel";

	/// <summary>
	/// Simulate a right-click on OSX when the Command key is held and a left-click is used (for trackpad).
	/// </summary>

	[Tooltip("If enabled, command-click will result in a right-click event on OSX")]
	public bool commandClick = true;

	/// <summary>
	/// Various keys used by the camera.
	/// </summary>

	public KeyCode submitKey0 = KeyCode.Return;
	public KeyCode submitKey1 = KeyCode.JoystickButton0;
	public KeyCode cancelKey0 = KeyCode.Escape;
	public KeyCode cancelKey1 = KeyCode.JoystickButton1;

	/// <summary>
	/// Whether NGUI will automatically hide the mouse cursor when controller or touch input is detected.
	/// </summary>

	public bool autoHideCursor = true;

	public delegate void OnCustomInput ();

	/// <summary>
	/// Custom input processing logic, if desired. For example: WP7 touches.
	/// Use UICamera.current to get the current camera.
	/// </summary>

	static public OnCustomInput onCustomInput;

	/// <summary>
	/// Custom input processing for the controller that replaces the built-in one, if desired. For example: adding support for a virtual mouse.
	/// The boolean value represents whether the controller events should be processed or not.
	/// </summary>

	static public System.Action<UICamera, bool> onControllerInput;

	/// <summary>
	/// Whether tooltips will be shown or not.
	/// </summary>

	static public bool showTooltips = true;

	/// <summary>
	/// Whether controller input will be temporarily disabled or not.
	/// It's useful to be able to turn off controller interaction and only turn it on when the UI is actually visible.
	/// </summary>

	static public bool disableController
	{
		get
		{
			return mDisableController && !UIPopupList.isOpen;
		}
		set
		{
			mDisableController = value;
		}
	}

	/// <summary>
	/// If set to 'true', all events will be ignored until set to 'true'.
	/// </summary>

	static public bool ignoreAllEvents = false;

	/// <summary>
	/// If set to 'true', controller input will be flat-out ignored. Permanently, for all cameras.
	/// </summary>

	static public bool ignoreControllerInput = false;

	static bool mDisableController = false;
	static Vector2 mLastPos = Vector2.zero;

	/// <summary>
	/// Position of the last touch (or mouse) event.
	/// </summary>

	[System.Obsolete("Use lastEventPosition instead. It handles controller input properly.")]
	static public Vector2 lastTouchPosition { get { return mLastPos; } set { mLastPos = value; } }

	/// <summary>
	/// Position of the last touch (or mouse) event.
	/// </summary>

	static public Vector2 lastEventPosition
	{
		get
		{
			if (currentScheme == ControlScheme.Controller && onControllerInput == null)
			{
				var go = hoveredObject;

				if (go != null)
				{
					var b = NGUIMath.CalculateAbsoluteWidgetBounds(go.transform);
					var cam = NGUITools.FindCameraForLayer(go.layer);
					return cam.WorldToScreenPoint(b.center);
				}
			}
			return mLastPos;
		}
		set { mLastPos = value; }
	}

	/// <summary>
	/// Position of the last touch (or mouse) event in the world.
	/// </summary>

	static public Vector3 lastWorldPosition = Vector3.zero;

	/// <summary>
	/// Last raycast into the world space.
	/// </summary>

	static public Ray lastWorldRay = new Ray();

	/// <summary>
	/// Last raycast hit prior to sending out the event. This is useful if you want detailed information
	/// about what was actually hit in your OnClick, OnHover, and other event functions.
	/// Note that this is not going to be valid if you're using 2D colliders.
	/// </summary>

	static public RaycastHit lastHit;

	/// <summary>
	/// UICamera that sent out the event.
	/// </summary>

	static public UICamera current = null;

	/// <summary>
	/// NGUI event system that will be handling all events.
	/// </summary>

	static public UICamera first
	{
		get
		{
			if (list == null || list.size == 0) return null;
			return list.buffer[0];
		}
	}

	/// <summary>
	/// Last camera active prior to sending out the event. This will always be the camera that actually sent out the event.
	/// </summary>

	static public Camera currentCamera = null;

	public delegate void OnSchemeChange ();

	/// <summary>
	/// Delegate called when the control scheme changes.
	/// </summary>

	static public OnSchemeChange onSchemeChange;
	static ControlScheme mLastScheme = ControlScheme.Mouse;

	/// <summary>
	/// Current control scheme. Derived from the last event to arrive.
	/// </summary>

	static public ControlScheme currentScheme
	{
		get
		{
			if (mCurrentKey == KeyCode.None) return ControlScheme.Touch;
			if (mCurrentKey >= KeyCode.JoystickButton0) return ControlScheme.Controller;

			var c = current;

			if (c != null)
			{
				if (mLastScheme == ControlScheme.Controller && (mCurrentKey == c.submitKey0 || mCurrentKey == c.submitKey1))
					return ControlScheme.Controller;

				if (c.useMouse) return ControlScheme.Mouse;
				if (c.useTouch) return ControlScheme.Touch;
				return ControlScheme.Controller;
			}
			return ControlScheme.Mouse;
		}
		set
		{
			if (mLastScheme != value)
			{
				if (value == ControlScheme.Mouse)
				{
					currentKey = KeyCode.Mouse0;
				}
				else if (value == ControlScheme.Controller)
				{
					currentKey = KeyCode.JoystickButton0;
				}
				else if (value == ControlScheme.Touch)
				{
					currentKey = KeyCode.None;
				}
				else currentKey = KeyCode.Alpha0;

				mLastScheme = value;
			}
		}
	}

	/// <summary>
	/// ID of the touch or mouse operation prior to sending out the event.
	/// Mouse ID is '-1' for left, '-2' for right mouse button, '-3' for middle.
	/// </summary>

	static public int currentTouchID = -100;

	static KeyCode mCurrentKey = KeyCode.Alpha0;

	/// <summary>
	/// Key that triggered the event, if any.
	/// </summary>

	static public KeyCode currentKey
	{
		get
		{
			return mCurrentKey;
		}
		set
		{
			if (mCurrentKey != value)
			{
				var before = mLastScheme;
				mCurrentKey = value;
				mLastScheme = currentScheme;

				if (before != mLastScheme)
				{
					if (mLastScheme == ControlScheme.Controller || currentScheme == ControlScheme.Controller) usedController = true;

					HideTooltip();

					if (mLastScheme == ControlScheme.Mouse)
					{
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
						Screen.lockCursor = false;
						Screen.showCursor = true;
#else
						Cursor.lockState = CursorLockMode.None;
						Cursor.visible = true;
#endif
					}
#if UNITY_EDITOR
					else if (mLastScheme == ControlScheme.Controller)
#else
					else
#endif
					{
						if (current != null && current.autoHideCursor)
						{
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
							Screen.showCursor = false;
							Screen.lockCursor = true;
#else
							Cursor.visible = false;
							Cursor.lockState = CursorLockMode.Locked;
#endif
							// Skip the next 2 frames worth of mouse movement
							mMouse[0].ignoreDelta = 2;
						}
					}

					if (onSchemeChange != null) onSchemeChange();
					UIRoot.Broadcast("OnLocalize");
				}
			}
		}
	}

	/// <summary>
	/// Ray projected into the screen underneath the current touch.
	/// </summary>

	static public Ray currentRay
	{
		get
		{
			return (currentCamera != null && currentTouch != null) ?
				currentCamera.ScreenPointToRay(currentTouch.pos) : new Ray();
		}
	}

	/// <summary>
	/// Current touch, set before any event function gets called.
	/// </summary>

	[System.NonSerialized] static public MouseOrTouch currentTouch = null;

	[System.NonSerialized] static bool mInputFocus = false;

	/// <summary>
	/// Whether an input field currently has focus.
	/// </summary>

	static public bool inputHasFocus
	{
		get
		{
			if (mInputFocus && mSelected && mSelected.activeInHierarchy) return true;
			return false;
		}
	}

	// Obsolete, kept for backwards compatibility.
	[System.NonSerialized] static GameObject mGenericHandler;

	/// <summary>
	/// If set, this game object will receive all events regardless of whether they were handled or not.
	/// </summary>

	[System.Obsolete("Use delegates instead such as UICamera.onClick, UICamera.onHover, etc.")]
	static public GameObject genericEventHandler { get { return mGenericHandler; } set { mGenericHandler = value; } }

	/// <summary>
	/// If events don't get handled, they will be forwarded to this game object.
	/// </summary>

	[System.NonSerialized] static public GameObject fallThrough;

	public delegate void MoveDelegate (Vector2 delta);
	public delegate void VoidDelegate (GameObject go);
	public delegate void BoolDelegate (GameObject go, bool state);
	public delegate void FloatDelegate (GameObject go, float delta);
	public delegate void VectorDelegate (GameObject go, Vector2 delta);
	public delegate void ObjectDelegate (GameObject go, GameObject obj);
	public delegate void KeyCodeDelegate (GameObject go, KeyCode key);

	/// <summary>
	/// These notifications are sent out prior to the actual event going out.
	/// </summary>

	[System.NonSerialized] static public VoidDelegate onClick;
	[System.NonSerialized] static public VoidDelegate onDoubleClick;
	[System.NonSerialized] static public BoolDelegate onHover;
	[System.NonSerialized] static public BoolDelegate onPress;
	[System.NonSerialized] static public BoolDelegate onSelect;
	[System.NonSerialized] static public FloatDelegate onScroll;
	[System.NonSerialized] static public VectorDelegate onDrag;
	[System.NonSerialized] static public VoidDelegate onDragStart;
	[System.NonSerialized] static public ObjectDelegate onDragOver;
	[System.NonSerialized] static public ObjectDelegate onDragOut;
	[System.NonSerialized] static public VoidDelegate onDragEnd;
	[System.NonSerialized] static public ObjectDelegate onDrop;
	[System.NonSerialized] static public KeyCodeDelegate onKey;
	[System.NonSerialized] static public KeyCodeDelegate onNavigate;
	[System.NonSerialized] static public VectorDelegate onPan;
	[System.NonSerialized] static public BoolDelegate onTooltip;
	[System.NonSerialized] static public MoveDelegate onMouseMove;

	// Mouse events
	static MouseOrTouch[] mMouse = new MouseOrTouch[] { new MouseOrTouch(), new MouseOrTouch(), new MouseOrTouch() };

	/// <summary>
	/// Access to the mouse-related data. This is intended to be read-only.
	/// </summary>

	static public MouseOrTouch mouse0 { get { return mMouse[0]; } }
	static public MouseOrTouch mouse1 { get { return mMouse[1]; } }
	static public MouseOrTouch mouse2 { get { return mMouse[2]; } }

	// Joystick/controller/keyboard event
	[System.NonSerialized] static public MouseOrTouch controller = new MouseOrTouch();

	/// <summary>
	/// This will be set to 'true' once the control scheme switches to using a controller at any point, and will remain 'true' for the rest of the session.
	/// </summary>

	static public bool usedController
	{
		get
		{
			return mUsedController;
		}
		set
		{
			if (mUsedController != value)
			{
				mUsedController = value;
				if (value && onActivateController != null) onActivateController();
			}
		}
	}

	[System.NonSerialized] static public System.Action onActivateController;
	[System.NonSerialized] static bool mUsedController = false;

	/// <summary>
	/// List of all the active touches.
	/// </summary>

	[System.NonSerialized] static public List<MouseOrTouch> activeTouches = new List<MouseOrTouch>();

	// Used internally to store IDs of active touches
	[System.NonSerialized] static List<int> mTouchIDs = new List<int>();

	// Used to detect screen dimension changes
	[System.NonSerialized] static int mWidth = 0;
	[System.NonSerialized] static int mHeight = 0;

	static public int screenWidth { get { return mWidth; } }

	static public int screenHeight { get { return mHeight; } }

	// Tooltip widget (mouse only)
	[System.NonSerialized] static GameObject mTooltip = null;

	// Mouse input is turned off on iOS
	[System.NonSerialized] Camera mCam = null;
	[System.NonSerialized] static float mTooltipTime = 0f;
	[System.NonSerialized] float mNextRaycast = 0f;

	/// <summary>
	/// Helper function that determines if this script should be handling the events.
	/// </summary>

	bool handlesEvents { get { return eventHandler == this; } }

	/// <summary>
	/// Caching is always preferable for performance.
	/// </summary>

	public Camera cachedCamera { get { if (mCam == null) mCam = this.FastGetComponent<Camera>(); return mCam; } }

	/// <summary>
	/// Set to 'true' just before OnDrag-related events are sent. No longer needed, but kept for backwards compatibility.
	/// </summary>

	[System.NonSerialized] static public bool isDragging = false;

	/// <summary>
	/// Object that should be showing the tooltip.
	/// </summary>

	static public GameObject tooltipObject { get { return mTooltip; } set { ShowTooltip(value); } }

#if !W2
	/// <summary>
	/// Whether this object is a part of the UI or not.
	/// </summary>

	static public bool IsPartOfUI (GameObject go)
	{
		if (go == null || go == fallThrough) return false;
		return NGUITools.FindInParents<UIRoot>(go) != null;
	}
#else
	// This is a simplified path I use in the Sightseer project. You are welcome to do the same if your UI is only on one layer.
	static public bool IsPartOfUI (GameObject go)
	{
		if (go == null || go == fallThrough) return false;
		if (mUILayer == -1) mUILayer = LayerMask.NameToLayer("UI");
		return go.layer == mUILayer;
	}
	static int mUILayer = -1;
#endif

	/// <summary>
	/// Whether the last raycast was over the UI.
	/// </summary>

	static public bool isOverUI
	{
		get
		{
			var frame = Time.frameCount;

			if (mLastOverCheck != frame)
			{
				mLastOverCheck = frame;
				mLastOverResult = false;

				if (currentTouch != null)
				{
					if (currentTouch.pressed != null)
					{
						mLastOverResult = IsPartOfUI(currentTouch.pressed);
						return mLastOverResult;
					}

					mLastOverResult = IsPartOfUI(currentTouch.current);
					return mLastOverResult;
				}

				if (currentScheme == ControlScheme.Touch)
				{
					for (int i = 0, imax = activeTouches.Count; i < imax; ++i)
					{
						var touch = activeTouches[i];

						if (IsPartOfUI(touch.pressed))
						{
							mLastOverResult = true;
							break;
						}
					}
				}
				else if (currentScheme == ControlScheme.Mouse)
				{
					for (int i = 0; i < 3; ++i)
					{
						var m = mMouse[i];

						if (IsPartOfUI(m.pressed != null ? m.pressed : (i == 0 ? m.current : null)))
						{
							mLastOverResult = true;
							break;
						}
					}
				}
				else if (currentScheme == ControlScheme.Controller)
				{
					if (onControllerInput != null)
					{
						if (IsPartOfUI(controller.pressed != null ? controller.pressed : controller.current))
						{
							mLastOverResult = true;
						}
					}
					else mLastOverResult = IsPartOfUI(controller.pressed);
				}
			}
			return mLastOverResult;
		}
	}

	/// <summary>
	/// Much like 'isOverUI', but also returns 'true' if there is currently an active mouse press on a UI element, or if a UI input has focus.
	/// </summary>

	static public bool uiHasFocus
	{
		get
		{
			var frame = Time.frameCount;

			if (mLastFocusCheck != frame)
			{
				mLastFocusCheck = frame;

				if (inputHasFocus)
				{
					mLastFocusResult = true;
					return mLastFocusResult;
				}

				if (currentTouch != null)
				{
					mLastFocusResult = currentTouch.isOverUI;
					return mLastFocusResult;
				}

				for (int i = 0, imax = activeTouches.Count; i < imax; ++i)
				{
					var touch = activeTouches[i];

					if (IsPartOfUI(touch.pressed))
					{
						mLastFocusResult = true;
						return mLastFocusResult;
					}
				}

				var m = mMouse[0];

				if (IsPartOfUI(m.pressed) || IsPartOfUI(m.current))
				{
					mLastFocusResult = true;
					return mLastFocusResult;
				}

				for (int i = 1; i < 3; ++i)
				{
					m = mMouse[i];

					if (IsPartOfUI(m.pressed))
					{
						mLastFocusResult = true;
						return mLastFocusResult;
					}
				}

				mLastFocusResult = IsPartOfUI(controller.pressed);
			}
			return mLastFocusResult;
		}
	}

	/// <summary>
	/// Whether there is a active current focus on the UI -- either input, or an active touch.
	/// </summary>

	static public bool interactingWithUI
	{
		get
		{
			var frame = Time.frameCount;

			if (mLastInteractionCheck != frame)
			{
				mLastInteractionCheck = frame;

				if (inputHasFocus)
				{
					mLastInteractionResult = true;
					return mLastInteractionResult;
				}

				for (int i = 0, imax = activeTouches.Count; i < imax; ++i)
				{
					var touch = activeTouches[i];

					if (IsPartOfUI(touch.pressed))
					{
						mLastInteractionResult = true;
						return mLastInteractionResult;
					}
				}

				for (int i = 0; i < 3; ++i)
				{
					var m = mMouse[i];

					if (IsPartOfUI(m.pressed))
					{
						mLastInteractionResult = true;
						return mLastInteractionResult;
					}
				}

				mLastInteractionResult = IsPartOfUI(controller.pressed);
			}
			return mLastInteractionResult;
		}
	}

	static int mLastInteractionCheck = -1;
	static bool mLastInteractionResult = false;
	static int mLastFocusCheck = -1;
	static bool mLastFocusResult = false;
	static int mLastOverCheck = -1;
	static bool mLastOverResult = false;

	static GameObject mRayHitObject;
	static GameObject mHover;
	static GameObject mSelected;

	/// <summary>
	/// The object over which the mouse is hovering over, or the object currently selected by the controller input.
	/// Mouse and controller input share the same hovered object, while touches have no hovered object at all.
	/// Checking this value from within a touch-based event will simply return the current touched object.
	/// </summary>

	static public GameObject hoveredObject
	{
		get
		{
			if (currentTouch != null && (currentScheme != ControlScheme.Mouse || currentTouch.dragStarted)) return currentTouch.current;
			if (mHover && mHover.activeInHierarchy) return mHover;
			mHover = null;
			return null;
		}
		set
		{
			// We already have this object highlighted
			if (mHover == value) return;

			bool statesDiffer = false;
			UICamera prevCamera = current;

			if (currentTouch == null)
			{
				statesDiffer = true;
				currentTouchID = -100;
				currentTouch = controller;
			}

			// Hide the tooltip
			ShowTooltip(null);

			// Remove the selection
			//if (mSelected && currentScheme == ControlScheme.Controller)
			//{
			//	Notify(mSelected, "OnSelect", false);
			//	if (onSelect != null) onSelect(mSelected, false);
			//	mSelected = null;
			//}

			// Remove the previous hover state
			if (mHover)
			{
				Notify(mHover, "OnHover", false);
				if (onHover != null) onHover(mHover, false);
			}

			mHover = value;
			currentTouch.clickNotification = ClickNotification.None;

			if (mHover)
			{
				if (mHover != controller.current)
				{
					if (mHover.FastGetComponent<UIKeyNavigation>() != null) controller.current = mHover;
				}

				// Locate the appropriate camera for the new object
				if (statesDiffer)
				{
					var cam = (mHover != null) ? FindCameraForLayer(mHover.layer) : list.buffer[0];

					if (cam != null)
					{
						current = cam;
						currentCamera = cam.cachedCamera;
					}
				}

				if (onHover != null) onHover(mHover, true);
				Notify(mHover, "OnHover", true);
			}

			// Restore previous states
			if (statesDiffer)
			{
				current = prevCamera;
				currentCamera = (prevCamera != null) ? prevCamera.cachedCamera : null;
				currentTouch = null;
				currentTouchID = -100;
			}
		}
	}

	/// <summary>
	/// Currently chosen object for controller-based navigation.
	/// </summary>

	static public GameObject controllerNavigationObject
	{
		get
		{
			if (controller.current && controller.current.activeInHierarchy) return controller.current;

			// Automatically update the object chosen by the controller
			if (currentScheme == ControlScheme.Controller && current != null && (current.useController && !ignoreControllerInput) && UIKeyNavigation.list.size > 0)
			{
				for (int i = 0; i < UIKeyNavigation.list.size; ++i)
				{
					var nav = UIKeyNavigation.list.buffer[i];

					if (nav && nav.constraint != UIKeyNavigation.Constraint.Explicit && nav.startsSelected)
					{
						hoveredObject = nav.gameObject;
						controller.current = mHover;
						return mHover;
					}
				}

				if (mHover == null)
				{
					for (int i = 0; i < UIKeyNavigation.list.size; ++i)
					{
						var nav = UIKeyNavigation.list.buffer[i];

						if (nav && nav.constraint != UIKeyNavigation.Constraint.Explicit)
						{
							hoveredObject = nav.gameObject;
							controller.current = mHover;
							return mHover;
						}
					}
				}
			}

			controller.current = null;
			return null;
		}
		set
		{
			if (controller.current != value && controller.current)
			{
				Notify(controller.current, "OnHover", false);
				if (onHover != null) onHover(controller.current, false);
				controller.current = null;
			}

			hoveredObject = value;
		}
	}

	/// <summary>
	/// Selected object receives exclusive focus. An input field requires exclusive focus in order to type,
	/// for example. Any object is capable of grabbing the selection just by clicking on that object,
	/// but only one object can be selected at a time.
	/// </summary>

	static public GameObject selectedObject
	{
		get
		{
			if (mSelected && mSelected.activeInHierarchy) return mSelected;
			mSelected = null;
			return null;
		}
		set
		{
			if (mSelected == value)
			{
				hoveredObject = value;
				controller.current = value;
				return;
			}

			// Hide the tooltip
			ShowTooltip(null);

			bool statesDiffer = false;
			UICamera prevCamera = current;
			//ControlScheme scheme = currentScheme;

			if (currentTouch == null)
			{
				statesDiffer = true;
				currentTouchID = -100;
				currentTouch = controller;
			}

			// Input no longer has selection, even if it did
			mInputFocus = false;

			// Remove the selection
			if (mSelected)
			{
				Notify(mSelected, "OnSelect", false);
				if (onSelect != null) onSelect(mSelected, false);
			}

			// Remove the hovered state
			//if (mHover && scheme < ControlScheme.Controller)
			//{
			//    Notify(mHover, "OnHover", false);
			//    if (onHover != null) onHover(mHover, false);
			//    mHover = null;
			//}

			// Change the selection and hover
			mSelected = value;
			//if (scheme >= ControlScheme.Controller) mHover = value;
			currentTouch.clickNotification = ClickNotification.None;

			if (value != null)
			{
				var nav = value.FastGetComponent<UIKeyNavigation>();

				if (nav != null)
				{
					controller.current = value;
				}
				else
				{
					// This makes it possible for UIInput to always respond to Escape, even if key navigation is not used
					var input = value.FastGetComponent<UIInput>();
					if (input != null) controller.current = value;
				}
			}

			// Set the camera for events
			if (mSelected && statesDiffer)
			{
				var cam = (mSelected != null) ? FindCameraForLayer(mSelected.layer) : list.buffer[0];

				if (cam != null)
				{
					current = cam;
					currentCamera = cam.cachedCamera;
				}
			}

			// Set the hovered state first
			//if (mHover && currentScheme >= ControlScheme.Controller)
			//{
			//    if (onHover != null) onHover(mHover, true);
			//    Notify(mHover, "OnHover", true);
			//}

			// Set the selection
			if (mSelected)
			{
				mInputFocus = (mSelected.activeInHierarchy && mSelected.FastGetComponent<UIInput>() != null);
				if (onSelect != null) onSelect(mSelected, true);
				Notify(mSelected, "OnSelect", true);
			}

			// Restore the states
			if (statesDiffer)
			{
				current = prevCamera;
				currentCamera = (prevCamera != null) ? prevCamera.cachedCamera : null;
				currentTouch = null;
				currentTouchID = -100;
			}
		}
	}

	/// <summary>
	/// Returns 'true' if any of the active touch, mouse or controller is currently holding the specified object.
	/// </summary>

	static public bool IsPressed (GameObject go)
	{
		for (int i = 0; i < 3; ++i) if (mMouse[i].pressed == go) return true;
		for (int i = 0, imax = activeTouches.Count; i < imax; ++i) if (activeTouches[i].pressed == go) return true;
		if (controller.pressed == go) return true;
		return false;
	}

	[System.Obsolete("Use either 'CountInputSources()' or 'activeTouches.Count'")]
	static public int touchCount { get { return CountInputSources(); } }

	/// <summary>
	/// Number of active touches from all sources.
	/// Note that this will include the sum of touch, mouse and controller events.
	/// If you want only touch events, use activeTouches.Count.
	/// </summary>

	static public int CountInputSources ()
	{
		int count = 0;
		for (int i = 0, imax = activeTouches.Count; i < imax; ++i) if (activeTouches[i].pressed != null) ++count;
		for (int i = 0; i < mMouse.Length; ++i) if (mMouse[i].pressed != null) ++count;
		if (controller.pressed != null) ++count;
		return count;
	}

	/// <summary>
	/// Number of active drag events from all sources.
	/// </summary>

	static public int dragCount
	{
		get
		{
			int count = 0;
			for (int i = 0, imax = activeTouches.Count; i < imax; ++i) if (activeTouches[i].dragged != null) ++count;
			for (int i = 0; i < mMouse.Length; ++i) if (mMouse[i].dragged != null) ++count;
			if (controller.dragged != null) ++count;
			return count;
		}
	}

	/// <summary>
	/// Convenience function that returns the main HUD camera.
	/// </summary>

	static public Camera mainCamera
	{
		get
		{
			var mouse = eventHandler;
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
			for (int i = 0; i < list.size; ++i)
			{
				// Invalid or inactive entry -- keep going
				var cam = list.buffer[i];
				if (cam == null || !cam.enabled || !NGUITools.GetActive(cam.gameObject)) continue;
				return cam;
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

	struct DepthEntry
	{
		public int depth;
		public RaycastHit hit;
		public Vector3 point;
		public GameObject go;
	}

	static DepthEntry mHit = new DepthEntry();
	static BetterList<DepthEntry> mHits = new BetterList<DepthEntry>();

	/// <summary>
	/// Find the rigidbody on the parent, but return 'null' if a UIPanel is found instead.
	/// The idea is: send events to the rigidbody in the world, but to colliders in the UI.
	/// </summary>

	static Rigidbody FindRootRigidbody (Transform trans)
	{
		Rigidbody rb;
		UIPanel p;

		while (trans != null)
		{
			if (trans.TryGetComponent(out p)) break;
			if (trans.TryGetComponent(out rb)) return rb;
			trans = trans.parent;
		}
		return null;
	}

	/// <summary>
	/// Find the 2D rigidbody on the parent, but return 'null' if a UIPanel is found instead.
	/// </summary>

	static Rigidbody2D FindRootRigidbody2D (Transform trans)
	{
		Rigidbody2D rb;
		UIPanel p;

		while (trans != null)
		{
			if (trans.TryGetComponent(out p)) break;
			if (trans.TryGetComponent(out rb)) return rb;
			trans = trans.parent;
		}
		return null;
	}

	/// <summary>
	/// Raycast into the screen underneath the touch and update its 'current' value.
	/// </summary>

	static public void Raycast (MouseOrTouch touch)
	{
		if (!Raycast(touch.pos)) mRayHitObject = fallThrough;
		if (mRayHitObject == null) mRayHitObject = mGenericHandler;
		touch.last = touch.current;
		touch.current = mRayHitObject;
		mLastPos = touch.pos;
	}

	static RaycastHit[] mRayHits;
	static Collider2D[] mOverlap;

	/// <summary>
	/// Returns the object under the specified position.
	/// </summary>

	static public bool Raycast (Vector3 inPos)
	{
		for (int i = 0; i < list.size; ++i)
		{
			var cam = list.buffer[i];

			// Skip inactive scripts
			if (!cam.enabled || !NGUITools.GetActive(cam.gameObject)) continue;

			// Convert to view space
			currentCamera = cam.cachedCamera;
			if (!currentCamera.enabled || currentCamera.targetDisplay != 0) continue;

			var pos = currentCamera.ScreenToViewportPoint(inPos);
			if (float.IsNaN(pos.x) || float.IsNaN(pos.y)) continue;

			// If it's outside the camera's viewport, do nothing
			if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f) continue;

			// Cast a ray into the screen
			var ray = currentCamera.ScreenPointToRay(inPos);

			// Raycast into the screen
			var mask = currentCamera.cullingMask & (int)cam.eventReceiverMask;
			var dist = (cam.rangeDistance > 0f) ? cam.rangeDistance : currentCamera.farClipPlane - currentCamera.nearClipPlane;

			if (cam.eventType == EventType.World_3D)
			{
				lastWorldRay = ray;
				
				if (Physics.Raycast(ray, out lastHit, dist, mask, QueryTriggerInteraction.Ignore))
				{
					lastWorldPosition = lastHit.point;
					mRayHitObject = lastHit.collider.gameObject;

					if (!cam.eventsGoToColliders)
					{
						var rb = mRayHitObject.gameObject.GetComponentInParent<Rigidbody>();
						if (rb != null) mRayHitObject = rb.gameObject;
					}
					return true;
				}
				continue;
			}
			else if (cam.eventType == EventType.UI_3D)
			{
				if (mRayHits == null) mRayHits = new RaycastHit[50];
				var hitCount = Physics.RaycastNonAlloc(ray, mRayHits, dist, mask, QueryTriggerInteraction.Collide);

				if (hitCount > 1)
				{
					for (int b = 0; b < hitCount; ++b)
					{
						var go = mRayHits[b].collider.gameObject;
						var w = go.FastGetComponent<UIWidget>();

						if (w != null)
						{
							if (!w.isVisible) continue;

							if (w is UISpriteCollection)
							{
								var sc = w as UISpriteCollection;
								if (sc.GetCurrentSprite() == null) continue;
							}

							if (w.hitCheck != null && !w.hitCheck(mRayHits[b].point)) continue;
						}
						else
						{
							var rect = NGUITools.FindInParents<UIRect>(go);
							if (rect != null && rect.finalAlpha < 0.001f) continue;
						}

						mHit.depth = NGUITools.CalculateRaycastDepth(go);

						if (mHit.depth != int.MaxValue)
						{
							mHit.hit = mRayHits[b];
							mHit.point = mRayHits[b].point;
							mHit.go = mRayHits[b].collider.gameObject;
							mHits.Add(mHit);
						}
					}

					mHits.Sort(delegate(DepthEntry r1, DepthEntry r2) { return r2.depth.CompareTo(r1.depth); });

					for (int b = 0; b < mHits.size; ++b)
					{
						if (IsVisible(ref mHits.buffer[b]))
						{
							lastHit = mHits.buffer[b].hit;
							mRayHitObject = mHits.buffer[b].go;
							lastWorldRay = ray;
							lastWorldPosition = mHits.buffer[b].point;
							mHits.Clear();
							return true;
						}
					}
					mHits.Clear();
				}
				else if (hitCount == 1)
				{
					var go = mRayHits[0].collider.gameObject;
					var w = go.FastGetComponent<UIWidget>();

					if (w != null)
					{
						if (!w.isVisible) continue;
						if (w.hitCheck != null && !w.hitCheck(mRayHits[0].point)) continue;
					}
					else
					{
						UIRect rect = NGUITools.FindInParents<UIRect>(go);
						if (rect != null && rect.finalAlpha < 0.001f) continue;
					}

					if (IsVisible(mRayHits[0].point, mRayHits[0].collider.gameObject))
					{
						lastHit = mRayHits[0];
						lastWorldRay = ray;
						lastWorldPosition = mRayHits[0].point;
						mRayHitObject = lastHit.collider.gameObject;
						return true;
					}
				}
				continue;
			}
			else if (cam.eventType == EventType.World_2D)
			{
				if (m2DPlane.Raycast(ray, out dist))
				{
					var point = ray.GetPoint(dist);
					var c2d = Physics2D.OverlapPoint(point, mask);

					if (c2d)
					{
						lastWorldPosition = point;
						mRayHitObject = c2d.gameObject;

						if (!cam.eventsGoToColliders)
						{
							Rigidbody2D rb = FindRootRigidbody2D(mRayHitObject.transform);
							if (rb != null) mRayHitObject = rb.gameObject;
						}
						return true;
					}
				}
				continue;
			}
			else if (cam.eventType == EventType.UI_2D)
			{
				if (m2DPlane.Raycast(ray, out dist))
				{
					lastWorldPosition = ray.GetPoint(dist);
					
					if (mOverlap == null) mOverlap = new Collider2D[50];
					var hitCount = Physics2D.OverlapPointNonAlloc(lastWorldPosition, mOverlap, mask);

					if (hitCount > 1)
					{
						for (int b = 0; b < hitCount; ++b)
						{
							var go = mOverlap[b].gameObject;
							var w = go.FastGetComponent<UIWidget>();

							if (w != null)
							{
								if (!w.isVisible) continue;
								if (w.hitCheck != null && !w.hitCheck(lastWorldPosition)) continue;
							}
							else
							{
								UIRect rect = NGUITools.FindInParents<UIRect>(go);
								if (rect != null && rect.finalAlpha < 0.001f) continue;
							}

							mHit.depth = NGUITools.CalculateRaycastDepth(go);

							if (mHit.depth != int.MaxValue)
							{
								mHit.go = go;
								mHit.point = lastWorldPosition;
								mHits.Add(mHit);
							}
						}

						mHits.Sort(delegate(DepthEntry r1, DepthEntry r2) { return r2.depth.CompareTo(r1.depth); });

						for (int b = 0; b < mHits.size; ++b)
						{
							if (IsVisible(ref mHits.buffer[b]))
							{
								mRayHitObject = mHits.buffer[b].go;
								mHits.Clear();
								return true;
							}
						}
						mHits.Clear();
					}
					else if (hitCount == 1)
					{
						var go = mOverlap[0].gameObject;
						var w = go.FastGetComponent<UIWidget>();

						if (w != null)
						{
							if (!w.isVisible) continue;
							if (w.hitCheck != null && !w.hitCheck(lastWorldPosition)) continue;
						}
						else
						{
							var rect = NGUITools.FindInParents<UIRect>(go);
							if (rect != null && rect.finalAlpha < 0.001f) continue;
						}

						if (IsVisible(lastWorldPosition, go))
						{
							mRayHitObject = go;
							return true;
						}
					}
				}
				continue;
			}
		}
		return false;
	}

	static Plane m2DPlane = new Plane(Vector3.back, 0f);

	/// <summary>
	/// Helper function to check if the specified hit is visible by the panel.
	/// </summary>

	static bool IsVisible (Vector3 worldPoint, GameObject go)
	{
		UIPanel panel = NGUITools.FindInParents<UIPanel>(go);

		while (panel != null)
		{
			if (!panel.IsVisible(worldPoint)) return false;
			panel = panel.parentPanel;
		}
		return true;
	}

	/// <summary>
	/// Helper function to check if the specified hit is visible by the panel.
	/// </summary>

	static bool IsVisible (ref DepthEntry de)
	{
		UIPanel panel = NGUITools.FindInParents<UIPanel>(de.go);

		while (panel != null)
		{
			if (!panel.IsVisible(de.point)) return false;
			panel = panel.parentPanel;
		}
		return true;
	}

	/// <summary>
	/// Whether the specified object should be highlighted.
	/// </summary>

	static public bool IsHighlighted (GameObject go) { return (UICamera.hoveredObject == go); }

	/// <summary>
	/// Find the camera responsible for handling events on objects of the specified layer.
	/// </summary>

	static public UICamera FindCameraForLayer (int layer)
	{
		int layerMask = 1 << layer;

		for (int i = 0; i < list.size; ++i)
		{
			UICamera cam = list.buffer[i];
			Camera uc = cam.cachedCamera;
			if ((uc != null) && (uc.cullingMask & layerMask) != 0) return cam;
		}
		return null;
	}

	/// <summary>
	/// Using the keyboard will result in 1 or -1, depending on whether up or down keys have been pressed.
	/// </summary>

	static int GetDirection (KeyCode up, KeyCode down)
	{
		if (GetKeyDown(up)) { currentKey = up; return 1; }
		if (GetKeyDown(down)) { currentKey = down; return -1; }
		return 0;
	}

	/// <summary>
	/// Using the keyboard will result in 1 or -1, depending on whether up or down keys have been pressed.
	/// </summary>

	static int GetDirection (KeyCode up0, KeyCode up1, KeyCode down0, KeyCode down1)
	{
		if (GetKeyDown(up0)) { currentKey = up0; return 1; }
		if (GetKeyDown(up1)) { currentKey = up1; return 1; }
		if (GetKeyDown(down0)) { currentKey = down0; return -1; }
		if (GetKeyDown(down1)) { currentKey = down1; return -1; }
		return 0;
	}

	// Used to ensure that joystick-based controls don't trigger that often
	static float mNextEvent = 0f;

	/// <summary>
	/// Using the joystick to move the UI results in 1 or -1 if the threshold has been passed, mimicking up/down keys.
	/// </summary>

	static int GetDirection (string axis)
	{
		float time = RealTime.time;

		if (mNextEvent < time && !string.IsNullOrEmpty(axis))
		{
			float val = GetAxis(axis);

			if (val > 0.75f)
			{
				currentKey = KeyCode.JoystickButton0;
				mNextEvent = time + 0.25f;
				return 1;
			}

			if (val < -0.75f)
			{
				currentKey = KeyCode.JoystickButton0;
				mNextEvent = time + 0.25f;
				return -1;
			}
		}
		return 0;
	}

	static int mNotifying = 0;

	/// <summary>
	/// Generic notification function. Used in place of SendMessage to shorten the code and allow for more than one receiver.
	/// </summary>

	static public void Notify (GameObject go, string funcName, object obj = null)
	{
		if (mNotifying > 10) return;

		// Automatically forward events to the currently open popup list
		if (currentScheme == ControlScheme.Controller && UIPopupList.isOpen && UIPopupList.current.source == go && UIPopupList.isOpen)
			go = UIPopupList.current.gameObject;

		if (go && go.activeInHierarchy)
		{
			++mNotifying;
			go.SendMessage(funcName, obj, SendMessageOptions.DontRequireReceiver);
			if (mGenericHandler != null && mGenericHandler != go)
				mGenericHandler.SendMessage(funcName, obj, SendMessageOptions.DontRequireReceiver);
			--mNotifying;
		}
	}

	/// <summary>
	/// Add this camera to the list.
	/// </summary>

	void Awake ()
	{
		mWidth = Screen.width;
		mHeight = Screen.height;

#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_WP_8_1 || UNITY_BLACKBERRY)
		currentScheme = ControlScheme.Touch;
#else
#if !UNITY_5_5_OR_NEWER
		if (Application.platform == RuntimePlatform.PS3 || Application.platform == RuntimePlatform.XBOX360)
#else
		if (Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.XboxOne)
#endif
		{
			currentScheme = ControlScheme.Controller;
		}
#endif

		// Save the starting mouse position
		mMouse[0].pos = Input.mousePosition;

		for (int i = 1; i < 3; ++i)
		{
			mMouse[i].pos = mMouse[0].pos;
			mMouse[i].lastPos = mMouse[0].pos;
		}
		mLastPos = mMouse[0].pos;

#if !UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX)
		string[] args = System.Environment.GetCommandLineArgs();

		if (args != null)
		{
			for (int i = 0; i < args.Length; ++i)
			{
				string s = args[i];
				if (s == "-noMouse") useMouse = false;
				else if (s == "-noTouch") useTouch = false;
				else if (s == "-noController") { useController = false; ignoreControllerInput = true; }
				else if (s == "-noJoystick") { useController = false; ignoreControllerInput = true; }
				else if (s == "-useMouse") useMouse = true;
				else if (s == "-useTouch") useTouch = true;
				else if (s == "-useController") useController = true;
				else if (s == "-useJoystick") useController = true;
			}
		}
#endif
	}

	/// <summary>
	/// Sort the list when enabled.
	/// </summary>

	void OnEnable ()
	{
		list.Add(this);
		list.Sort(CompareFunc);
	}

	/// <summary>
	/// Remove this camera from the list.
	/// </summary>

	void OnDisable () { list.Remove(this); }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
	static bool disableControllerCheck = true;
#endif

	/// <summary>
	/// We don't want the camera to send out any kind of mouse events.
	/// </summary>

	void Start ()
	{
		list.Sort(CompareFunc);

		if (eventType != EventType.World_3D && cachedCamera.transparencySortMode != TransparencySortMode.Orthographic)
			cachedCamera.transparencySortMode = TransparencySortMode.Orthographic;

		if (Application.isPlaying)
		{
			// Always set a fall-through object
			if (fallThrough == null)
			{
				UIRoot root = NGUITools.FindInParents<UIRoot>(gameObject);
				fallThrough = (root != null) ? root.gameObject : gameObject;
			}
			cachedCamera.eventMask = 0;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
			// Automatically disable controller-based input if the game starts with a non-zero controller input.
			// This most commonly happens with Thrustmaster and other similar joystick types.
			if (!ignoreControllerInput && disableControllerCheck && useController && handlesEvents)
			{
				disableControllerCheck = false;
				if (!string.IsNullOrEmpty(horizontalAxisName) && Mathf.Abs(GetAxis(horizontalAxisName)) > 0.1f) ignoreControllerInput = true;
				else if (!string.IsNullOrEmpty(verticalAxisName) && Mathf.Abs(GetAxis(verticalAxisName)) > 0.1f) ignoreControllerInput = true;
				else if (!string.IsNullOrEmpty(horizontalPanAxisName) && Mathf.Abs(GetAxis(horizontalPanAxisName)) > 0.1f) ignoreControllerInput = true;
				else if (!string.IsNullOrEmpty(verticalPanAxisName) && Mathf.Abs(GetAxis(verticalPanAxisName)) > 0.1f) ignoreControllerInput = true;
			}
#endif
		}
	}

#if UNITY_EDITOR
	void OnValidate () { Start(); }
#endif

	[ContextMenu("Start ignoring events")] void StartIgnoring () { ignoreAllEvents = true; }
	[ContextMenu("Stop ignoring events")] void StopIgnoring () { ignoreAllEvents = false; }

	/// <summary>
	/// Check the input and send out appropriate events.
	/// </summary>

	void Update ()
	{
		// Ignore events if asked for
		if (ignoreAllEvents) return;

		// Only the first UI layer should be processing events
#if UNITY_EDITOR
		if (!Application.isPlaying || !handlesEvents) return;
#else
        if (!handlesEvents) return;
#endif
		if (processEventsIn == ProcessEventsIn.Update) ProcessEvents();
	}

	/// <summary>
	/// Keep an eye on screen size changes.
	/// </summary>

	void LateUpdate ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying || !handlesEvents) return;
#else
		if (!handlesEvents) return;
#endif
		if (processEventsIn == ProcessEventsIn.LateUpdate) ProcessEvents();

		int w = Screen.width;
		int h = Screen.height;

		if (w != mWidth || h != mHeight)
		{
			mWidth = w;
			mHeight = h;

			UIRoot.Broadcast("UpdateAnchors");

			if (onScreenResize != null)
				onScreenResize();
		}
	}

	/// <summary>
	/// Process all events.
	/// </summary>

	void ProcessEvents ()
	{
		current = this;
		NGUIDebug.debugRaycast = debug;

		// Process touch events first
		if (useTouch) ProcessTouches();
		else if (useMouse) ProcessMouse();

		// Custom input processing
		if (onCustomInput != null) onCustomInput();

		// Update the keyboard and joystick events
		if ((useKeyboard || useController) && !disableController && !ignoreControllerInput)
		{
			if (onControllerInput != null) onControllerInput(this, true);
			else ProcessControllerInput();
		}
		else if (onControllerInput != null) onControllerInput(this, false);

		// If it's time to show a tooltip, inform the object we're hovering over
		if (useMouse && mHover != null)
		{
			float scroll = !string.IsNullOrEmpty(scrollAxisName) ? GetAxis(scrollAxisName) : 0f;

			if (scroll != 0f)
			{
				if (onScroll != null) onScroll(mHover, scroll);
				Notify(mHover, "OnScroll", scroll);
			}

			if (currentScheme == ControlScheme.Mouse && showTooltips && mTooltipTime != 0f && !UIPopupList.isOpen && mMouse[0].dragged == null &&
				(mTooltipTime < Time.unscaledTime || GetKey(KeyCode.LeftShift) || GetKey(KeyCode.RightShift)))
			{
				currentTouch = mMouse[0];
				currentTouchID = -1;
				ShowTooltip(mHover);
			}
		}

		if (mTooltip != null && !NGUITools.GetActive(mTooltip)) ShowTooltip(null);

		current = null;
		currentTouchID = -100;
	}

	/// <summary>
	/// Update mouse input.
	/// </summary>

	public void ProcessMouse ()
	{
		// Is any button currently pressed?
		bool isPressed = false;
		bool justPressed = false;

		for (int i = 0; i < 3; ++i)
		{
			if (Input.GetMouseButtonDown(i))
			{
				currentKey = KeyCode.Mouse0 + i;
				justPressed = true;
				isPressed = true;
			}
			else if (Input.GetMouseButton(i))
			{
				currentKey = KeyCode.Mouse0 + i;
				isPressed = true;
			}
		}

		// We're currently using touches -- do nothing
		if (currentScheme == ControlScheme.Touch && activeTouches.Count > 0) return;

		currentTouch = mMouse[0];

		// Update the position and delta
		Vector2 pos = Input.mousePosition;

		if (currentTouch.ignoreDelta == 0)
		{
			currentTouch.delta = pos - currentTouch.pos;
		}
		else
		{
			--currentTouch.ignoreDelta;
			currentTouch.delta.x = 0f;
			currentTouch.delta.y = 0f;
		}

		float sqrMag = currentTouch.delta.sqrMagnitude;
		currentTouch.pos = pos;
		mLastPos = pos;

		bool posChanged = false;

		if (currentScheme != ControlScheme.Mouse)
		{
			if (sqrMag < 0.001f) return; // Nothing changed and we are not using the mouse -- exit
			currentKey = KeyCode.Mouse0;
			posChanged = true;
		}
		else if (sqrMag > 0.001f)
		{
			posChanged = true;
		}

		// Propagate the updates to the other mouse buttons
		for (int i = 1; i < 3; ++i)
		{
			mMouse[i].pos = currentTouch.pos;
			mMouse[i].delta = currentTouch.delta;
		}

		// No need to perform raycasts every frame
		if (isPressed || posChanged || mNextRaycast < RealTime.time)
		{
			mNextRaycast = RealTime.time + 0.02f;
			Raycast(currentTouch);

			if (isPressed)
			{
				posChanged = true;
				for (int i = 1; i < 3; ++i) mMouse[i].current = currentTouch.current;
			}
			else if (mMouse[0].current != currentTouch.current)
			{
				currentKey = KeyCode.Mouse0;
				posChanged = true;
				for (int i = 1; i < 3; ++i) mMouse[i].current = currentTouch.current;
			}
		}

		bool highlightChanged = (currentTouch.last != currentTouch.current);
		bool wasPressed = (currentTouch.pressed != null);

		if (!wasPressed && posChanged) hoveredObject = currentTouch.current;

		currentTouchID = -1;
		if (highlightChanged) currentKey = KeyCode.Mouse0;

		if (!isPressed && posChanged)
		{
			if (mTooltipTime != 0f)
			{
				// Delay the tooltip
				mTooltipTime = Time.unscaledTime + tooltipDelay;
			}
			else if (mTooltip != null && (!stickyTooltip || highlightChanged))
			{
				// Hide the tooltip
				ShowTooltip(null);
			}
		}

		// Generic mouse move notifications
		if (posChanged && onMouseMove != null)
		{
			onMouseMove(currentTouch.delta);
			currentTouch = null;
		}

		// The button was released over a different object -- remove the highlight from the previous
		if (highlightChanged && (justPressed || (wasPressed && !isPressed)))
			hoveredObject = null;

		// Process all 3 mouse buttons as individual touches
		for (int i = 0; i < 3; ++i)
		{
			bool pressed = Input.GetMouseButtonDown(i);
			bool unpressed = Input.GetMouseButtonUp(i);
			if (pressed || unpressed) currentKey = KeyCode.Mouse0 + i;
			currentTouch = mMouse[i];

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			if (commandClick && i == 0 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				currentTouchID = -2;
				currentKey = KeyCode.Mouse1;
			}
			else
#endif
			{
				currentTouchID = -1 - i;
				currentKey = KeyCode.Mouse0 + i;
			}

			// We don't want to update the last camera while there is a touch happening
			if (pressed)
			{
				currentTouch.pressedCam = currentCamera;
				currentTouch.pressTime = RealTime.time;
			}
			else if (currentTouch.pressed != null) currentCamera = currentTouch.pressedCam;

			// Process the mouse events
			ProcessTouch(pressed, unpressed);
		}

		// If nothing is pressed and there is an object under the touch, highlight it
		if (!isPressed && highlightChanged)
		{
			currentTouch = mMouse[0];
			mTooltipTime = Time.unscaledTime + tooltipDelay;
			currentTouchID = -1;
			currentKey = KeyCode.Mouse0;
			hoveredObject = currentTouch.current;
		}

		currentTouch = null;

		// Update the last value
		mMouse[0].last = mMouse[0].current;
		for (int i = 1; i < 3; ++i) mMouse[i].last = mMouse[0].last;
	}

	static bool mUsingTouchEvents = true;

	public class Touch
	{
		public int fingerId;
		public TouchPhase phase = TouchPhase.Began;
		public Vector2 position;
		public int tapCount = 0;
	}

	public delegate int GetTouchCountCallback ();
	public delegate Touch GetTouchCallback (int index);

	static public GetTouchCountCallback GetInputTouchCount;
	static public GetTouchCallback GetInputTouch;

	/// <summary>
	/// Update touch-based events.
	/// </summary>

	public void ProcessTouches ()
	{
		int count = (GetInputTouchCount == null) ? Input.touchCount : GetInputTouchCount();

		for (int i = 0; i < count; ++i)
		{
			int fingerId;
			TouchPhase phase;
			Vector2 position;
			int tapCount;

			if (GetInputTouch == null)
			{
				UnityEngine.Touch touch = Input.GetTouch(i);
				phase = touch.phase;
				fingerId = touch.fingerId;
				position = touch.position;
				tapCount = touch.tapCount;
#if UNITY_WIIU && !UNITY_EDITOR
				// Unity bug: http://www.tasharen.com/forum/index.php?topic=5821.0
				position.y = Screen.height - position.y;
#endif
			}
			else
			{
				Touch touch = GetInputTouch(i);
				phase = touch.phase;
				fingerId = touch.fingerId;
				position = touch.position;
				tapCount = touch.tapCount;
			}

			currentTouchID = allowMultiTouch ? fingerId : 1;
			currentTouch = GetTouch(currentTouchID, true);

			bool pressed = (phase == TouchPhase.Began) || currentTouch.touchBegan;
			bool unpressed = (phase == TouchPhase.Canceled) || (phase == TouchPhase.Ended);
			currentTouch.delta = position - currentTouch.pos;
			currentTouch.pos = position;
			currentKey = KeyCode.None;

			// Raycast into the screen
			Raycast(currentTouch);

			// We don't want to update the last camera while there is a touch happening
			if (pressed) currentTouch.pressedCam = currentCamera;
			else if (currentTouch.pressed != null) currentCamera = currentTouch.pressedCam;

			// Double-tap support
			if (tapCount > 1) currentTouch.clickTime = RealTime.time;

			// Process the events from this touch
			ProcessTouch(pressed, unpressed);

			// If the touch has ended, remove it from the list
			if (unpressed) RemoveTouch(currentTouchID);

			currentTouch.touchBegan = false;
			currentTouch.last = null;
			currentTouch = null;

			// Don't consider other touches
			if (!allowMultiTouch) break;
		}

		if (count == 0)
		{
			// Skip the first frame after using touch events
			if (mUsingTouchEvents)
			{
				mUsingTouchEvents = false;
				return;
			}

			if (useMouse) ProcessMouse();
#if UNITY_EDITOR
			else if (GetInputTouch == null) ProcessFakeTouches();
#endif
		}
		else mUsingTouchEvents = true;
	}

	/// <summary>
	/// Process fake touch events where the mouse acts as a touch device.
	/// Useful for testing mobile functionality in the editor.
	/// </summary>

	void ProcessFakeTouches ()
	{
		bool pressed = Input.GetMouseButtonDown(0);
		bool unpressed = Input.GetMouseButtonUp(0);
		bool held = Input.GetMouseButton(0);

		if (pressed || unpressed || held)
		{
			currentTouchID = 1;
			currentTouch = mMouse[0];
			currentTouch.touchBegan = pressed;

			if (pressed)
			{
				currentTouch.pressTime = RealTime.time;
				activeTouches.Add(currentTouch);
			}

			Vector2 pos = Input.mousePosition;
			currentTouch.delta = pos - currentTouch.pos;
			currentTouch.pos = pos;

			// Raycast into the screen
			Raycast(currentTouch);

			// We don't want to update the last camera while there is a touch happening
			if (pressed) currentTouch.pressedCam = currentCamera;
			else if (currentTouch.pressed != null) currentCamera = currentTouch.pressedCam;

			// Process the events from this touch
			currentKey = KeyCode.None;
			ProcessTouch(pressed, unpressed);

			// If the touch has ended, remove it from the list
			if (unpressed) activeTouches.Remove(currentTouch);
			currentTouch.last = null;
			currentTouch = null;
		}
	}

	/// <summary>
	/// Process keyboard and joystick events.
	/// </summary>

	public void ProcessControllerInput ()
	{
		currentTouchID = -100;
		currentTouch = controller;

		bool submitKeyDown = false;
		bool submitKeyUp = false;

		if (submitKey0 != KeyCode.None && GetKeyDown(submitKey0))
		{
			currentKey = submitKey0;
			submitKeyDown = true;
		}
		else if (submitKey1 != KeyCode.None && GetKeyDown(submitKey1))
		{
			currentKey = submitKey1;
			submitKeyDown = true;
		}
		else if ((submitKey0 == KeyCode.Return || submitKey1 == KeyCode.Return) && GetKeyDown(KeyCode.KeypadEnter))
		{
			currentKey = submitKey0;
			submitKeyDown = true;
		}

		if (submitKey0 != KeyCode.None && GetKeyUp(submitKey0))
		{
			currentKey = submitKey0;
			submitKeyUp = true;
		}
		else if (submitKey1 != KeyCode.None && GetKeyUp(submitKey1))
		{
			currentKey = submitKey1;
			submitKeyUp = true;
		}
		else if ((submitKey0 == KeyCode.Return || submitKey1 == KeyCode.Return) && GetKeyUp(KeyCode.KeypadEnter))
		{
			currentKey = submitKey0;
			submitKeyUp = true;
		}

		if (submitKeyDown) currentTouch.pressTime = RealTime.time;

		if ((submitKeyDown || submitKeyUp) && currentScheme == ControlScheme.Controller)
		{
			currentTouch.current = controllerNavigationObject;
			ProcessTouch(submitKeyDown, submitKeyUp);
			currentTouch.last = currentTouch.current;
		}

		var lastKey = KeyCode.None;

		// Handle controller events
		if (useController && !ignoreControllerInput)
		{
			// Automatically choose the first available selection object
			if (!disableController && currentScheme == ControlScheme.Controller && (currentTouch.current == null || !currentTouch.current.activeInHierarchy))
				currentTouch.current = controllerNavigationObject;

			if (!string.IsNullOrEmpty(verticalAxisName))
			{
				int vertical = GetDirection(verticalAxisName);

				if (vertical != 0)
				{
					ShowTooltip(null);
					currentScheme = ControlScheme.Controller;
					currentTouch.current = controllerNavigationObject;

					if (currentTouch.current != null)
					{
						lastKey = vertical > 0 ? KeyCode.UpArrow : KeyCode.DownArrow;
						if (onNavigate != null) onNavigate(currentTouch.current, lastKey);
						Notify(currentTouch.current, "OnNavigate", lastKey);
					}
				}
			}

			if (!string.IsNullOrEmpty(horizontalAxisName))
			{
				int horizontal = GetDirection(horizontalAxisName);

				if (horizontal != 0)
				{
					ShowTooltip(null);
					currentScheme = ControlScheme.Controller;
					currentTouch.current = controllerNavigationObject;

					if (currentTouch.current != null)
					{
						lastKey = horizontal > 0 ? KeyCode.RightArrow : KeyCode.LeftArrow;
						if (onNavigate != null) onNavigate(currentTouch.current, lastKey);
						Notify(currentTouch.current, "OnNavigate", lastKey);
					}
				}
			}

			var x = !string.IsNullOrEmpty(horizontalPanAxisName) ? GetAxis(horizontalPanAxisName) : 0f;
			var y = !string.IsNullOrEmpty(verticalPanAxisName) ? GetAxis(verticalPanAxisName) : 0f;

			if (x != 0f || y != 0f)
			{
				ShowTooltip(null);
				currentScheme = ControlScheme.Controller;
				currentTouch.current = controllerNavigationObject;

				if (currentTouch.current != null)
				{
					var delta = new Vector2(x, y);
					delta *= Time.unscaledDeltaTime;
					if (onPan != null) onPan(currentTouch.current, delta);
					Notify(currentTouch.current, "OnPan", delta);
				}
			}
		}

		// Send out all key events
		if (GetAnyKeyDown != null ? GetAnyKeyDown() : Input.anyKeyDown)
		{
			for (int i = 0, imax = NGUITools.keys.Length; i < imax; ++i)
			{
				var key = NGUITools.keys[i];
				if (lastKey == key) continue;
				if (!GetKeyDown(key)) continue;

				if (!useKeyboard && key < KeyCode.Mouse0) continue;
				if ((!useController || ignoreControllerInput) && key >= KeyCode.JoystickButton0) continue;
				if (!useMouse && (key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6)) continue;

				currentKey = key;
				if (onKey != null) onKey(currentTouch.current, key);
				Notify(currentTouch.current, "OnKey", key);
			}
		}

		currentTouch = null;
	}

	/// <summary>
	/// Process the press part of a touch.
	/// </summary>

	void ProcessPress (bool pressed, float click, float drag, bool hoverEvents)
	{
		// Send out the press message
		if (pressed)
		{
			if (mTooltip != null) ShowTooltip(null);
			mTooltipTime = Time.unscaledTime + tooltipDelay;
			currentTouch.pressStarted = true;
			if (onPress != null && currentTouch.pressed)
				onPress(currentTouch.pressed, false);

			Notify(currentTouch.pressed, "OnPress", false);

			if (hoverEvents && hoveredObject == null && currentTouch.current != null)
				hoveredObject = currentTouch.current;

			currentTouch.pressed = currentTouch.current;
			currentTouch.dragged = currentTouch.current;
			currentTouch.clickNotification = ClickNotification.BasedOnDelta;
			currentTouch.totalDelta = Vector2.zero;
			currentTouch.dragStarted = false;
			currentTouch.longPress = false;

			if (onPress != null && currentTouch.pressed)
				onPress(currentTouch.pressed, true);

			Notify(currentTouch.pressed, "OnPress", true);

			// Change the selection
			if (mSelected != currentTouch.pressed)
			{
				// Input no longer has selection, even if it did
				mInputFocus = false;

				// Remove the selection
				if (mSelected)
				{
					Notify(mSelected, "OnSelect", false);
					if (onSelect != null) onSelect(mSelected, false);
				}

				// Change the selection
				mSelected = currentTouch.pressed;

				if (currentTouch.pressed != null)
				{
					var nav = currentTouch.pressed.FastGetComponent<UIKeyNavigation>();
					if (nav != null) controller.current = currentTouch.pressed;
				}

				// Set the selection
				if (mSelected)
				{
					mInputFocus = (mSelected.activeInHierarchy && mSelected.FastGetComponent<UIInput>() != null);
					if (onSelect != null) onSelect(mSelected, true);
					Notify(mSelected, "OnSelect", true);
				}
			}
		}
		else if (currentTouch.pressed != null && (currentTouch.delta.sqrMagnitude != 0f || currentTouch.current != currentTouch.last))
		{
			// Keep track of the total movement
			currentTouch.totalDelta += currentTouch.delta;
			float mag = currentTouch.totalDelta.sqrMagnitude;
			bool justStarted = false;

			// If the drag process hasn't started yet but we've already moved off the object, start it immediately
			if (!currentTouch.dragStarted && currentTouch.last != currentTouch.current)
			{
				currentTouch.dragStarted = true;
				currentTouch.delta = currentTouch.totalDelta;
				currentTouch.clickNotification = ClickNotification.None;

				// OnDragOver is sent for consistency, so that OnDragOut is always preceded by OnDragOver
				isDragging = true;

				if (onDragStart != null) onDragStart(currentTouch.dragged);
				Notify(currentTouch.dragged, "OnDragStart", null);

				if (onDragOver != null) onDragOver(currentTouch.last, currentTouch.dragged);
				Notify(currentTouch.last, "OnDragOver", currentTouch.dragged);

				isDragging = false;
			}
			else if (!currentTouch.dragStarted && drag < mag)
			{
				// If the drag event has not yet started, see if we've dragged the touch far enough to start it
				justStarted = true;
				currentTouch.dragStarted = true;
				currentTouch.delta = currentTouch.totalDelta;
			}

			// If we're dragging the touch, send out drag events
			if (currentTouch.dragStarted)
			{
				if (mTooltip != null) ShowTooltip(null);

				isDragging = true;
				bool isDisabled = (currentTouch.clickNotification == ClickNotification.None);

				if (justStarted)
				{
					if (onDragStart != null) onDragStart(currentTouch.dragged);
					Notify(currentTouch.dragged, "OnDragStart", null);

					if (onDragOver != null) onDragOver(currentTouch.last, currentTouch.dragged);
					Notify(currentTouch.current, "OnDragOver", currentTouch.dragged);
				}
				else if (currentTouch.last != currentTouch.current)
				{
					if (onDragOut != null) onDragOut(currentTouch.last, currentTouch.dragged);
					Notify(currentTouch.last, "OnDragOut", currentTouch.dragged);

					if (onDragOver != null) onDragOver(currentTouch.last, currentTouch.dragged);
					Notify(currentTouch.current, "OnDragOver", currentTouch.dragged);
				}

				if (onDrag != null) onDrag(currentTouch.dragged, currentTouch.delta);
				Notify(currentTouch.dragged, "OnDrag", currentTouch.delta);

				currentTouch.last = currentTouch.current;
				isDragging = false;

				if (isDisabled)
				{
					// If the notification status has already been disabled, keep it as such
					currentTouch.clickNotification = ClickNotification.None;
				}
				else if (currentTouch.clickNotification == ClickNotification.BasedOnDelta && click < mag)
				{
					// We've dragged far enough to cancel the click
					currentTouch.clickNotification = ClickNotification.None;
				}
			}
		}
//#if W2
//		else if (currentTouch.delta.y != 0f && (GetKey(KeyCode.LeftControl) || GetKey(KeyCode.LeftCommand)))
//		{
//			var f = currentTouch.delta.y * 0.001f;
//			if (onScroll != null) onScroll(mHover, f);
//			Notify(currentTouch.current, "OnScroll", f);
//		}
//#endif
	}

	/// <summary>
	/// Process the release part of a touch.
	/// </summary>

	void ProcessRelease (float drag, bool hoverEvents)
	{
		// Send out the unpress message
		if (currentTouch == null) return;
		currentTouch.pressStarted = false;

		if (currentTouch.pressed != null)
		{
			// If there was a drag event in progress, make sure OnDragOut gets sent
			if (currentTouch.dragStarted)
			{
				if (onDragOut != null) onDragOut(currentTouch.last, currentTouch.dragged);
				Notify(currentTouch.last, "OnDragOut", currentTouch.dragged);

				if (onDragEnd != null) onDragEnd(currentTouch.dragged);
				Notify(currentTouch.dragged, "OnDragEnd", null);
			}

			// Send the notification of a touch ending
			if (onPress != null) onPress(currentTouch.pressed, false);
			Notify(currentTouch.pressed, "OnPress", false);

			// Send a hover message to the object
			if (hoverEvents)
			{
				var hasCollider = HasCollider(currentTouch.pressed);

				if (hasCollider)
				{
					// OnHover is sent to restore the visual state
					if (mHover == currentTouch.current)
					{
						if (onHover != null) onHover(currentTouch.current, true);
						Notify(currentTouch.current, "OnHover", true);
					}
					else hoveredObject = currentTouch.current;
				}
			}

			// If the button/touch was released on the same object, consider it a click and select it
			if (currentTouch.dragged == currentTouch.current ||
				(currentScheme != ControlScheme.Controller &&
				currentTouch.clickNotification != ClickNotification.None &&
				currentTouch.totalDelta.sqrMagnitude < drag))
			{
				// If the touch should consider clicks, send out an OnClick notification
				if (currentTouch.clickNotification != ClickNotification.None && currentTouch.pressed == currentTouch.current)
				{
					ShowTooltip(null);
					float time = RealTime.time;

					if (onClick != null) onClick(currentTouch.pressed);
					Notify(currentTouch.pressed, "OnClick", null);

					if (currentTouch.clickTime + 0.35f > time && currentTouch.lastClickGO == currentTouch.pressed)
					{
						if (onDoubleClick != null) onDoubleClick(currentTouch.pressed);
						Notify(currentTouch.pressed, "OnDoubleClick", null);
					}

					currentTouch.lastClickGO = currentTouch.pressed;
					currentTouch.clickTime = time;
				}
			}
			else if (currentTouch.dragStarted) // The button/touch was released on a different object
			{
				// Send a drop notification (for drag & drop)
				if (onDrop != null) onDrop(currentTouch.current, currentTouch.dragged);
				Notify(currentTouch.current, "OnDrop", currentTouch.dragged);
			}
		}

		currentTouch.dragStarted = false;
		currentTouch.longPress = false;
		currentTouch.pressed = null;
		currentTouch.dragged = null;
	}

	/// <summary>
	/// This function can be used to process a virtual touch the same way mouse events would be processed:
	/// with hover events and tooltips. This function can be used to create a virtual cursor controlled by a joystick.
	/// In most cases you will want to call this function from inside your UICamera.onControllerInput event listener.
	/// Be sure to assign 'currentTouchID' and 'currentCamera' before calling this function.
	/// This function returns 'true' if the touch event triggered a long press event.
	/// </summary>
	/// <param name="touch">Updated touch with 'lastPos' and 'pos' set to last frame's and current frame's values, respectively.</param>
	/// <param name="treatAsMouse">Pass 'true' if you want this virtual touch to send out mouse movement events.</param>
	/// <param name="longPressTooltip">Pass 'true' if this event is eligible to show a tooltip.</param>

	public bool ProcessVirtualTouch (MouseOrTouch touch, bool pressed, bool released, bool held, bool treatAsMouse, bool longPressTooltip)
	{
		var posChanged = (touch.lastPos - touch.pos).sqrMagnitude != 0f;

		if (pressed)
		{
			touch.pressedCam = currentCamera;
			touch.pressTime = RealTime.time;
			touch.longPress = false;
			posChanged = true;
		}
		else if (touch.pressed != null) currentCamera = touch.pressedCam;

		currentTouch = touch;
		lastEventPosition = touch.pos;

		if (touch.ignoreDelta == 0)
		{
			touch.delta = touch.pos - touch.lastPos;
		}
		else
		{
			--touch.ignoreDelta;
			touch.delta.x = 0f;
			touch.delta.y = 0f;
		}

		if (held || posChanged || touch.rayTime < RealTime.time)
		{
			touch.rayTime = RealTime.time + 0.02f;
			Raycast(touch);
		}

		var highlightChanged = (touch.last != touch.current);
		var wasPressed = (touch.pressed != null);

		if (!wasPressed && posChanged) hoveredObject = touch.current;
		if (treatAsMouse && posChanged && onMouseMove != null) onMouseMove(touch.delta);

		// The button was released over a different object -- remove the highlight from the previous
		if (highlightChanged && (pressed || (wasPressed && !held))) hoveredObject = null;

		// Process the touch fully, sending out events as needed
		var longPress = ProcessTouch(pressed, released, treatAsMouse, longPressTooltip);

		// Update the hovered object
		if (!held && highlightChanged) hoveredObject = touch.current;

		touch.last = touch.current;
		return longPress;
	}

	static bool HasCollider (GameObject go)
	{
		if (go == null) return false;

		Collider c;
		if (go.TryGetComponent(out c)) return c.enabled;

		Collider2D c2;
		if (go.TryGetComponent(out c2)) return c2.enabled;
		
		return false;
	}

	/// <summary>
	/// Process the events of the specified touch.
	/// </summary>

	public bool ProcessTouch (bool pressed, bool released, bool treatAsMouse = false, bool longPressTooltip = true)
	{
		if (released) mTooltipTime = 0f;

		// Whether we're using the mouse
		bool isMouse = treatAsMouse || (currentScheme == ControlScheme.Mouse);
		float drag   = isMouse ? mouseDragThreshold : touchDragThreshold;
		float click  = isMouse ? mouseClickThreshold : touchClickThreshold;

		// So we can use sqrMagnitude below
		drag *= drag;
		click *= click;

		if (currentTouch.pressed != null)
		{
			if (released) ProcessRelease(drag, isMouse);
			ProcessPress(pressed, click, drag, isMouse);

			if (currentTouch.pressed == currentTouch.current && !currentTouch.dragStarted)
			{
				if (longPressDelay != 0f && !currentTouch.longPress && currentTouch.deltaTime > longPressDelay)
				{
					// Long press event
					currentTouch.longPress = true;
					currentTouch.clickNotification = ClickNotification.None;
					Notify(currentTouch.current, "OnLongPress", null);

					if (mTooltipTime != 0f && longPressTooltip && this.longPressTooltip)
					{
						mTooltipTime = 0f;
						ShowTooltip(currentTouch.pressed);
					}
					return true;
				}
			}
		}
		else if (isMouse || pressed || released)
		{
			ProcessPress(pressed, click, drag, isMouse);
			if (released) ProcessRelease(drag, isMouse);
		}
		return false;
	}

	/// <summary>
	/// Cancel the next tooltip, preventing it from being shown.
	/// Moving the mouse again will reset this counter.
	/// </summary>

	static public void CancelNextTooltip () { mTooltipTime = 0f; }

	/// <summary>
	/// Show or hide the tooltip.
	/// </summary>

	static public bool ShowTooltip (GameObject go)
	{
		if (mTooltip != go)
		{
			if (mTooltip != null)
			{
				if (onTooltip != null) onTooltip(mTooltip, false);
				Notify(mTooltip, "OnTooltip", false);
			}

			mTooltip = go;
			mTooltipTime = 0f;

			if (mTooltip != null)
			{
				if (onTooltip != null) onTooltip(mTooltip, true);
				Notify(mTooltip, "OnTooltip", true);
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Hide the tooltip, if one is visible.
	/// </summary>

	static public bool HideTooltip () { return ShowTooltip(null); }

	/// <summary>
	/// Reset the tooltip timer, allowing the tooltip to show again even over the same widget.
	/// </summary>

	static public void ResetTooltip (float delay = 0.5f)
	{
		ShowTooltip(null);
		mTooltipTime = Time.unscaledTime + delay;
	}
}
