//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI Widget Creation Wizard
/// </summary>

public class UICreateWidgetWizard : EditorWindow
{
	public enum WidgetType
	{
		Label,
		Sprite,
		SlicedSprite,
		TiledSprite,
		FilledSprite,
		SimpleTexture,
		Button,
		ImageButton,
		Checkbox,
		ProgressBar,
		Slider,
		Input,
		PopupList,
		PopupMenu,
		ScrollBar,
	}

	static WidgetType mType = WidgetType.Button;
	static string mSprite = "";
	static string mSliced = "";
	static string mTiled = "";
	static string mFilled = "";
	static string mButton = "";
	static string mImage0 = "";
	static string mImage1 = "";
	static string mImage2 = "";
	static string mSliderBG = "";
	static string mSliderFG = "";
	static string mSliderTB = "";
	static string mCheckBG = "";
	static string mCheck = "";
	static string mInputBG = "";
	static string mListFG = "";
	static string mListBG = "";
	static string mListHL = "";
	static string mScrollBG = "";
	static string mScrollFG = "";
	static Color mColor = Color.white;
	static bool mLoaded = false;
	static bool mScrollCL = true;
	static UIScrollBar.Direction mScrollDir = UIScrollBar.Direction.Horizontal;

	/// <summary>
	/// Save the specified string into player prefs.
	/// </summary>

	static void SaveString (string field, string val)
	{
		if (string.IsNullOrEmpty(val))
		{
			EditorPrefs.DeleteKey(field);
		}
		else
		{
			EditorPrefs.SetString(field, val);
		}
	}

	/// <summary>
	/// Load the specified string from player prefs.
	/// </summary>

	static string LoadString (string field) { string s = EditorPrefs.GetString(field); return (string.IsNullOrEmpty(s)) ? "" : s; }

	/// <summary>
	/// Save all serialized values in editor prefs.
	/// This is necessary because static values get wiped out as soon as scripts get recompiled.
	/// </summary>

	static void Save ()
	{
		EditorPrefs.SetInt("NGUI Widget Type", (int)mType);
		EditorPrefs.SetInt("NGUI Color", NGUIMath.ColorToInt(mColor));
		EditorPrefs.SetBool("NGUI ScrollCL", mScrollCL);
		EditorPrefs.SetInt("NGUI Scroll Dir", (int)mScrollDir);

		SaveString("NGUI Sprite", mSprite);
		SaveString("NGUI Sliced", mSliced);
		SaveString("NGUI Tiled", mTiled);
		SaveString("NGUI Filled", mFilled);
		SaveString("NGUI Button", mButton);
		SaveString("NGUI Image 0", mImage0);
		SaveString("NGUI Image 1", mImage1);
		SaveString("NGUI Image 2", mImage2);
		SaveString("NGUI CheckBG", mCheckBG);
		SaveString("NGUI Check", mCheck);
		SaveString("NGUI SliderBG", mSliderBG);
		SaveString("NGUI SliderFG", mSliderFG);
		SaveString("NGUI SliderTB", mSliderTB);
		SaveString("NGUI InputBG", mInputBG);
		SaveString("NGUI ListFG", mListFG);
		SaveString("NGUI ListBG", mListBG);
		SaveString("NGUI ListHL", mListHL);
		SaveString("NGUI ScrollBG", mScrollBG);
		SaveString("NGUI ScrollFG", mScrollFG);
	}

	/// <summary>
	/// Load all serialized values from editor prefs.
	/// This is necessary because static values get wiped out as soon as scripts get recompiled.
	/// </summary>

	static void Load ()
	{
		mType = (WidgetType)EditorPrefs.GetInt("NGUI Widget Type", 0);
		mScrollDir = (UIScrollBar.Direction)EditorPrefs.GetInt("NGUI Scroll Dir", 0);

		int color = EditorPrefs.GetInt("NGUI Color", -1);
		if (color != -1) mColor = NGUIMath.IntToColor(color);

		mSprite		= LoadString("NGUI Sprite");
		mSliced		= LoadString("NGUI Sliced");
		mTiled		= LoadString("NGUI Tiled");
		mFilled		= LoadString("NGUI Filled");
		mButton		= LoadString("NGUI Button");
		mImage0		= LoadString("NGUI Image 0");
		mImage1		= LoadString("NGUI Image 1");
		mImage2		= LoadString("NGUI Image 2");
		mCheckBG	= LoadString("NGUI CheckBG");
		mCheck		= LoadString("NGUI Check");
		mSliderBG	= LoadString("NGUI SliderBG");
		mSliderFG	= LoadString("NGUI SliderFG");
		mSliderTB	= LoadString("NGUI SliderTB");
		mInputBG	= LoadString("NGUI InputBG");
		mListFG		= LoadString("NGUI ListFG");
		mListBG		= LoadString("NGUI ListBG");
		mListHL		= LoadString("NGUI ListHL");
		mScrollBG	= LoadString("NGUI ScrollBG");
		mScrollFG	= LoadString("NGUI ScrollFG");
		mScrollCL	= EditorPrefs.GetBool("NGUI ScrollCL", true);
	}

	/// <summary>
	/// Atlas selection function.
	/// </summary>

	void OnSelectAtlas (MonoBehaviour obj)
	{
		NGUISettings.atlas = obj as UIAtlas;
		Repaint();
	}

	/// <summary>
	/// Font selection function.
	/// </summary>

	void OnSelectFont (MonoBehaviour obj)
	{
		NGUISettings.font = obj as UIFont;
		Repaint();
	}

	/// <summary>
	/// Convenience function -- creates the "Add To" button and the parent object field to the right of it.
	/// </summary>

	bool ShouldCreate (GameObject go, bool isValid)
	{
		GUI.color = isValid ? Color.green : Color.grey;

		GUILayout.BeginHorizontal();
		bool retVal = GUILayout.Button("Add To", GUILayout.Width(76f));
		GUI.color = Color.white;
		GameObject sel = EditorGUILayout.ObjectField(go, typeof(GameObject), true, GUILayout.Width(140f)) as GameObject;
		GUILayout.Label("Select the parent in the Hierarchy View", GUILayout.MinWidth(10000f));
		GUILayout.EndHorizontal();

		if (sel != go) Selection.activeGameObject = sel;

		if (retVal && isValid)
		{
			NGUIEditorTools.RegisterUndo("Add a Widget");
			return true;
		}
		return false;
	}

	/// <summary>
	/// Label creation function.
	/// </summary>

	void CreateLabel (GameObject go)
	{
		GUILayout.BeginHorizontal();
		Color c = EditorGUILayout.ColorField("Color", mColor, GUILayout.Width(220f));
		GUILayout.Label("Color tint the label will start with");
		GUILayout.EndHorizontal();

		if (mColor != c)
		{
			mColor = c;
			Save();
		}

		if (ShouldCreate(go, NGUISettings.font != null))
		{
			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
			lbl.font = NGUISettings.font;
			lbl.text = "New Label";
			lbl.color = mColor;
			lbl.MakePixelPerfect();
			Selection.activeGameObject = lbl.gameObject;
		}
	}

	/// <summary>
	/// Sprite creation function.
	/// </summary>

	void CreateSprite<T> (GameObject go, ref string field) where T : UISprite
	{
		if (NGUISettings.atlas != null)
		{
			GUILayout.BeginHorizontal();
			string sp = UISpriteInspector.SpriteField(NGUISettings.atlas, "Sprite", field, GUILayout.Width(200f));

			if (!string.IsNullOrEmpty(sp))
			{
				GUILayout.Space(20f);
				GUILayout.Label("Sprite that will be created");
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				NGUISettings.pivot = (UIWidget.Pivot)EditorGUILayout.EnumPopup("Pivot", NGUISettings.pivot, GUILayout.Width(200f));
				GUILayout.Space(20f);
				GUILayout.Label("Initial pivot point used by the sprite");
				GUILayout.EndHorizontal();
			}
			else GUILayout.EndHorizontal();

			if (sp != field)
			{
				field = sp;
				Save();
			}
		}

		if (ShouldCreate(go, NGUISettings.atlas != null))
		{
			T sprite = NGUITools.AddWidget<T>(go);
			sprite.name = sprite.name + " (" + field + ")";
			sprite.atlas = NGUISettings.atlas;
			sprite.spriteName = field;
			sprite.pivot = NGUISettings.pivot;
			sprite.MakePixelPerfect();
			Selection.activeGameObject = sprite.gameObject;
		}
	}

	/// <summary>
	/// UI Texture doesn't do anything other than creating the widget.
	/// </summary>

	void CreateSimpleTexture (GameObject go)
	{
		if (ShouldCreate(go, true))
		{
			UITexture tex = NGUITools.AddWidget<UITexture>(go);
			Selection.activeGameObject = tex.gameObject;
		}
	}

	/// <summary>
	/// Button creation function.
	/// </summary>

	void CreateButton (GameObject go)
	{
		if (NGUISettings.atlas != null)
		{
			GUILayout.BeginHorizontal();
			string bg = UISpriteInspector.SpriteField(NGUISettings.atlas, "Background", mButton, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sliced Sprite for the background");
			GUILayout.EndHorizontal();
			if (mButton != bg) { mButton = bg; Save(); }
		}

		if (ShouldCreate(go, NGUISettings.atlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Button";

			UISlicedSprite bg = NGUITools.AddWidget<UISlicedSprite>(go);
			bg.name = "Background";
			bg.depth = depth;
			bg.atlas = NGUISettings.atlas;
			bg.spriteName = mButton;
			bg.transform.localScale = new Vector3(150f, 40f, 1f);
			bg.MakePixelPerfect();

			if (NGUISettings.font != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.font = NGUISettings.font;
				lbl.text = go.name;
				lbl.MakePixelPerfect();
			}

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the scripts
			go.AddComponent<UIButton>().tweenTarget = bg.gameObject;
			go.AddComponent<UIButtonScale>();
			go.AddComponent<UIButtonOffset>();
			go.AddComponent<UIButtonSound>();

			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Button creation function.
	/// </summary>

	void CreateImageButton (GameObject go)
	{
		if (NGUISettings.atlas != null)
		{
			GUILayout.BeginHorizontal();
			string bg = UISpriteInspector.SpriteField(NGUISettings.atlas, "Normal", mImage0, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Normal state sprite");
			GUILayout.EndHorizontal();
			if (mImage0 != bg) { mImage0 = bg; Save(); }

			GUILayout.BeginHorizontal();
			bg = UISpriteInspector.SpriteField(NGUISettings.atlas, "Hover", mImage1, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Hover state sprite");
			GUILayout.EndHorizontal();
			if (mImage1 != bg) { mImage1 = bg; Save(); }

			GUILayout.BeginHorizontal();
			bg = UISpriteInspector.SpriteField(NGUISettings.atlas, "Pressed", mImage2, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Pressed state sprite");
			GUILayout.EndHorizontal();
			if (mImage2 != bg) { mImage2 = bg; Save(); }
		}

		if (ShouldCreate(go, NGUISettings.atlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Image Button";

			UIAtlas.Sprite sp = NGUISettings.atlas.GetSprite(mImage0);
			UISprite sprite = (sp.inner == sp.outer) ? NGUITools.AddWidget<UISprite>(go) :
				(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

			sprite.name = "Background";
			sprite.depth = depth;
			sprite.atlas = NGUISettings.atlas;
			sprite.spriteName = mImage0;
			sprite.transform.localScale = new Vector3(150f, 40f, 1f);
			sprite.MakePixelPerfect();

			if (NGUISettings.font != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.font = NGUISettings.font;
				lbl.text = go.name;
				lbl.MakePixelPerfect();
			}

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the scripts
			UIImageButton ib = go.AddComponent<UIImageButton>();
			ib.target		 = sprite;
			ib.normalSprite  = mImage0;
			ib.hoverSprite	 = mImage1;
			ib.pressedSprite = mImage2;
			go.AddComponent<UIButtonSound>();

			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Checkbox creation function.
	/// </summary>

	void CreateCheckbox (GameObject go)
	{
		if (NGUISettings.atlas != null)
		{
			GUILayout.BeginHorizontal();
			string bg = UISpriteInspector.SpriteField(NGUISettings.atlas, "Background", mCheckBG, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sliced Sprite for the background");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			string ck = UISpriteInspector.SpriteField(NGUISettings.atlas, "Checkmark", mCheck, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sprite visible when the state is 'checked'");
			GUILayout.EndHorizontal();

			if (mCheckBG != bg || mCheck != ck)
			{
				mCheckBG = bg;
				mCheck = ck;
				Save();
			}
		}

		if (ShouldCreate(go, NGUISettings.atlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Checkbox";

			UISlicedSprite bg = NGUITools.AddWidget<UISlicedSprite>(go);
			bg.name = "Background";
			bg.depth = depth;
			bg.atlas = NGUISettings.atlas;
			bg.spriteName = mCheckBG;
			bg.transform.localScale = new Vector3(26f, 26f, 1f);
			bg.MakePixelPerfect();

			UISprite fg = NGUITools.AddWidget<UISprite>(go);
			fg.name = "Checkmark";
			fg.atlas = NGUISettings.atlas;
			fg.spriteName = mCheck;
			fg.MakePixelPerfect();

			if (NGUISettings.font != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.font = NGUISettings.font;
				lbl.text = go.name;
				lbl.pivot = UIWidget.Pivot.Left;
				lbl.transform.localPosition = new Vector3(16f, 0f, 0f);
				lbl.MakePixelPerfect();
			}

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the scripts
			go.AddComponent<UICheckbox>().checkSprite = fg;
			go.AddComponent<UIButton>().tweenTarget = bg.gameObject;
			go.AddComponent<UIButtonScale>().tweenTarget = bg.transform;
			go.AddComponent<UIButtonSound>();

			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Scroll bar template.
	/// </summary>

	void CreateScrollBar (GameObject go)
	{
		if (NGUISettings.atlas != null)
		{
			GUILayout.BeginHorizontal();
			string bg = UISpriteInspector.SpriteField(NGUISettings.atlas, "Background", mScrollBG, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sprite for the background");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			string fg = UISpriteInspector.SpriteField(NGUISettings.atlas, "Foreground", mScrollFG, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sprite for the foreground (thumb)");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			UIScrollBar.Direction dir = (UIScrollBar.Direction)EditorGUILayout.EnumPopup("Direction", mScrollDir, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Add colliders?", GUILayout.Width(80f));
			bool draggable = EditorGUILayout.Toggle(mScrollCL);
			GUILayout.EndHorizontal();

			if (mScrollBG != bg || mScrollFG != fg || mScrollCL != draggable || mScrollDir != dir)
			{
				mScrollBG = bg;
				mScrollFG = fg;
				mScrollCL = draggable;
				mScrollDir = dir;
				Save();
			}
		}

		if (ShouldCreate(go, NGUISettings.atlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Scroll Bar";

			UISlicedSprite bg = NGUITools.AddWidget<UISlicedSprite>(go);
			bg.name = "Background";
			bg.depth = depth;
			bg.atlas = NGUISettings.atlas;
			bg.spriteName = mScrollBG;
			bg.transform.localScale = new Vector3(400f + bg.border.x + bg.border.z, 14f + bg.border.y + bg.border.w, 1f);
			bg.MakePixelPerfect();

			UISlicedSprite fg = NGUITools.AddWidget<UISlicedSprite>(go);
			fg.name = "Foreground";
			fg.atlas = NGUISettings.atlas;
			fg.spriteName = mScrollFG;

			UIScrollBar sb = go.AddComponent<UIScrollBar>();
			sb.background = bg;
			sb.foreground = fg;
			sb.direction = mScrollDir;
			sb.barSize = 0.3f;
			sb.scrollValue = 0.3f;
			sb.ForceUpdate();

			if (mScrollCL)
			{
				NGUITools.AddWidgetCollider(bg.gameObject);
				NGUITools.AddWidgetCollider(fg.gameObject);
			}
			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Progress bar creation function.
	/// </summary>

	void CreateSlider (GameObject go, bool slider)
	{
		if (NGUISettings.atlas != null)
		{
			GUILayout.BeginHorizontal();
			string bg = UISpriteInspector.SpriteField(NGUISettings.atlas, "Background", mSliderBG, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sprite for the background (empty bar)");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			string fg = UISpriteInspector.SpriteField(NGUISettings.atlas, "Foreground", mSliderFG, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sprite for the foreground (full bar)");
			GUILayout.EndHorizontal();

			string tb = mSliderTB;

			if (slider)
			{
				GUILayout.BeginHorizontal();
				tb = UISpriteInspector.SpriteField(NGUISettings.atlas, "Thumb", mSliderTB, GUILayout.Width(200f));
				GUILayout.Space(20f);
				GUILayout.Label("Sprite for the thumb indicator");
				GUILayout.EndHorizontal();
			}

			if (mSliderBG != bg || mSliderFG != fg || mSliderTB != tb)
			{
				mSliderBG = bg;
				mSliderFG = fg;
				mSliderTB = tb;
				Save();
			}
		}

		if (ShouldCreate(go, NGUISettings.atlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = slider ? "Slider" : "Progress Bar";

			// Background sprite
			UIAtlas.Sprite bgs = NGUISettings.atlas.GetSprite(mSliderBG);
			UISprite back = (bgs.inner == bgs.outer) ?
				(UISprite)NGUITools.AddWidget<UISprite>(go) :
				(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

			back.name = "Background";
			back.depth = depth;
			back.pivot = UIWidget.Pivot.Left;
			back.atlas = NGUISettings.atlas;
			back.spriteName = mSliderBG;
			back.transform.localScale = new Vector3(200f, 30f, 1f);
			back.MakePixelPerfect();

			// Fireground sprite
			UIAtlas.Sprite fgs = NGUISettings.atlas.GetSprite(mSliderFG);
			UISprite front = (fgs.inner == fgs.outer) ?
				(UISprite)NGUITools.AddWidget<UIFilledSprite>(go) :
				(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

			front.name = "Foreground";
			front.pivot = UIWidget.Pivot.Left;
			front.atlas = NGUISettings.atlas;
			front.spriteName = mSliderFG;
			front.transform.localScale = new Vector3(200f, 30f, 1f);
			front.MakePixelPerfect();

			// Add a collider
			if (slider) NGUITools.AddWidgetCollider(go);

			// Add the slider script
			UISlider uiSlider = go.AddComponent<UISlider>();
			uiSlider.foreground = front.transform;
			uiSlider.fullSize = front.transform.localScale;

			// Thumb sprite
			if (slider)
			{
				UIAtlas.Sprite tbs = NGUISettings.atlas.GetSprite(mSliderTB);
				UISprite thb = (tbs.inner == tbs.outer) ?
					(UISprite)NGUITools.AddWidget<UISprite>(go) :
					(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

				thb.name = "Thumb";
				thb.atlas = NGUISettings.atlas;
				thb.spriteName = mSliderTB;
				thb.transform.localPosition = new Vector3(200f, 0f, 0f);
				thb.transform.localScale = new Vector3(20f, 40f, 1f);
				thb.MakePixelPerfect();

				NGUITools.AddWidgetCollider(thb.gameObject);
				thb.gameObject.AddComponent<UIButtonColor>();
				thb.gameObject.AddComponent<UIButtonScale>();

				uiSlider.thumb = thb.transform;
			}
			uiSlider.sliderValue = 0.75f;

			// Select the slider
			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Input field creation function.
	/// </summary>

	void CreateInput (GameObject go)
	{
		if (NGUISettings.atlas != null)
		{
			GUILayout.BeginHorizontal();
			string bg = UISpriteInspector.SpriteField(NGUISettings.atlas, "Background", mInputBG, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sliced Sprite for the background");
			GUILayout.EndHorizontal();
			if (mInputBG != bg) { mInputBG = bg; Save(); }
		}

		if (ShouldCreate(go, NGUISettings.atlas != null && NGUISettings.font != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Input";

			float padding = 3f;

			UISlicedSprite bg = NGUITools.AddWidget<UISlicedSprite>(go);
			bg.name = "Background";
			bg.depth = depth;
			bg.atlas = NGUISettings.atlas;
			bg.spriteName = mInputBG;
			bg.pivot = UIWidget.Pivot.Left;
			bg.transform.localScale = new Vector3(400f, NGUISettings.font.size + padding * 2f, 1f);
			bg.MakePixelPerfect();

			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
			lbl.font = NGUISettings.font;
			lbl.pivot = UIWidget.Pivot.Left;
			lbl.transform.localPosition = new Vector3(padding, 0f, 0f);
			lbl.multiLine = false;
			lbl.supportEncoding = false;
			lbl.lineWidth = Mathf.RoundToInt(400f - padding * 2f);
			lbl.text = "You can type here";
			lbl.MakePixelPerfect();

			// Add a collider to the background
			NGUITools.AddWidgetCollider(go);

			// Add an input script to the background and have it point to the label
			UIInput input = go.AddComponent<UIInput>();
			input.label = lbl;

			// Update the selection
			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Create a popup list or a menu.
	/// </summary>

	void CreatePopup (GameObject go, bool isDropDown)
	{
		if (NGUISettings.atlas != null)
		{
			GUILayout.BeginHorizontal();
			string sprite = UISpriteInspector.SpriteField(NGUISettings.atlas, "Foreground", mListFG, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Foreground sprite (shown on the button)");
			GUILayout.EndHorizontal();
			if (mListFG != sprite) { mListFG = sprite; Save(); }

			GUILayout.BeginHorizontal();
			sprite = UISpriteInspector.SpriteField(NGUISettings.atlas, "Background", mListBG, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Background sprite (envelops the options)");
			GUILayout.EndHorizontal();
			if (mListBG != sprite) { mListBG = sprite; Save(); }

			GUILayout.BeginHorizontal();
			sprite = UISpriteInspector.SpriteField(NGUISettings.atlas, "Highlight", mListHL, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sprite used to highlight the selected option");
			GUILayout.EndHorizontal();
			if (mListHL != sprite) { mListHL = sprite; Save(); }
		}

		if (ShouldCreate(go, NGUISettings.atlas != null && NGUISettings.font != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = isDropDown ? "Popup List" : "Popup Menu";

			UIAtlas.Sprite sphl = NGUISettings.atlas.GetSprite(mListHL);
			UIAtlas.Sprite spfg = NGUISettings.atlas.GetSprite(mListFG);

			Vector2 hlPadding = new Vector2(
				Mathf.Max(4f, sphl.inner.xMin - sphl.outer.xMin),
				Mathf.Max(4f, sphl.inner.yMin - sphl.outer.yMin));

			Vector2 fgPadding = new Vector2(
				Mathf.Max(4f, spfg.inner.xMin - spfg.outer.xMin),
				Mathf.Max(4f, spfg.inner.yMin - spfg.outer.yMin));

			// Background sprite
			UISprite sprite = NGUITools.AddSprite(go, NGUISettings.atlas, mListFG);
			sprite.depth = depth;
			sprite.atlas = NGUISettings.atlas;
			sprite.transform.localScale = new Vector3(150f + fgPadding.x * 2f, NGUISettings.font.size + fgPadding.y * 2f, 1f);
			sprite.pivot = UIWidget.Pivot.Left;
			sprite.MakePixelPerfect();

			// Text label
			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
			lbl.font = NGUISettings.font;
			lbl.text = go.name;
			lbl.pivot = UIWidget.Pivot.Left;
			lbl.cachedTransform.localPosition = new Vector3(fgPadding.x, 0f, 0f);
			lbl.MakePixelPerfect();

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the popup list
			UIPopupList list = go.AddComponent<UIPopupList>();
			list.atlas = NGUISettings.atlas;
			list.font = NGUISettings.font;
			list.backgroundSprite = mListBG;
			list.highlightSprite = mListHL;
			list.padding = hlPadding;
			if (isDropDown) list.textLabel = lbl;
			for (int i = 0; i < 5; ++i) list.items.Add(isDropDown ? ("List Option " + i) : ("Menu Option " + i));

			// Add the scripts
			go.AddComponent<UIButton>().tweenTarget = sprite.gameObject;
			go.AddComponent<UIButtonSound>();

			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Repaint the window on selection.
	/// </summary>

	void OnSelectionChange () { Repaint(); }

	/// <summary>
	/// Draw the custom wizard.
	/// </summary>

	void OnGUI ()
	{
		// Load the saved preferences
		if (!mLoaded) { mLoaded = true; Load(); }

		EditorGUIUtility.LookLikeControls(80f);
		GameObject go = NGUIEditorTools.SelectedRoot();

		if (go == null)
		{
			GUILayout.Label("You must create a UI first.");
			
			if (GUILayout.Button("Open the New UI Wizard"))
			{
				EditorWindow.GetWindow<UICreateNewUIWizard>(false, "New UI", true);
			}
		}
		else
		{
			GUILayout.Space(4f);

			GUILayout.BeginHorizontal();
			ComponentSelector.Draw<UIAtlas>(NGUISettings.atlas, OnSelectAtlas, GUILayout.Width(140f));
			GUILayout.Label("Texture atlas used by widgets", GUILayout.MinWidth(10000f));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			ComponentSelector.Draw<UIFont>(NGUISettings.font, OnSelectFont, GUILayout.Width(140f));
			GUILayout.Label("Font used by labels", GUILayout.MinWidth(10000f));
			GUILayout.EndHorizontal();

			GUILayout.Space(-2f);
			NGUIEditorTools.DrawSeparator();

			GUILayout.BeginHorizontal();
			WidgetType wt = (WidgetType)EditorGUILayout.EnumPopup("Template", mType, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Select a widget template to use");
			GUILayout.EndHorizontal();

			if (mType != wt) { mType = wt; Save(); }

			switch (mType)
			{
				case WidgetType.Label:			CreateLabel(go); break;
				case WidgetType.Sprite:			CreateSprite<UISprite>(go, ref mSprite); break;
				case WidgetType.SlicedSprite:	CreateSprite<UISlicedSprite>(go, ref mSliced); break;
				case WidgetType.TiledSprite:	CreateSprite<UITiledSprite>(go, ref mTiled); break;
				case WidgetType.FilledSprite:	CreateSprite<UIFilledSprite>(go, ref mFilled); break;
				case WidgetType.SimpleTexture:	CreateSimpleTexture(go); break;
				case WidgetType.Button:			CreateButton(go); break;
				case WidgetType.ImageButton:	CreateImageButton(go); break;
				case WidgetType.Checkbox:		CreateCheckbox(go); break;
				case WidgetType.ProgressBar:	CreateSlider(go, false); break;
				case WidgetType.Slider:			CreateSlider(go, true); break;
				case WidgetType.Input:			CreateInput(go); break;
				case WidgetType.PopupList:		CreatePopup(go, true); break;
				case WidgetType.PopupMenu:		CreatePopup(go, false); break;
				case WidgetType.ScrollBar:		CreateScrollBar(go); break;
			}
		}
	}
}