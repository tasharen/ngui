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
	}

	static UIAtlas mAtlas;
	static UIFont mFont;
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
	static bool mLoaded = false;

	/// <summary>
	/// Save the specified string into player prefs.
	/// </summary>

	static void SaveString (string field, string val)
	{
		if (string.IsNullOrEmpty(val))
		{
			PlayerPrefs.DeleteKey(field);
		}
		else
		{
			PlayerPrefs.SetString(field, val);
		}
	}

	/// <summary>
	/// Load the specified string from player prefs.
	/// </summary>

	static string LoadString (string field) { string s = PlayerPrefs.GetString(field); return (string.IsNullOrEmpty(s)) ? "" : s; }

	/// <summary>
	/// Save all serialized values in player prefs.
	/// This is necessary because static values get wiped out as soon as scripts get recompiled.
	/// </summary>

	static void Save ()
	{
		PlayerPrefs.SetInt("NGUI Widget Type", (int)mType);
		PlayerPrefs.SetInt("NGUI Atlas", (mAtlas != null) ? mAtlas.GetInstanceID() : -1);
		PlayerPrefs.SetInt("NGUI Font", (mFont != null) ? mFont.GetInstanceID() : -1);

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

		PlayerPrefs.Save();
	}

	/// <summary>
	/// Load all serialized values from Player Prefs.
	/// This is necessary because static values get wiped out as soon as scripts get recompiled.
	/// </summary>

	static void Load ()
	{
		mType = (WidgetType)PlayerPrefs.GetInt("NGUI Widget Type", 0);

		int atlasID = PlayerPrefs.GetInt("NGUI Atlas", -1);
		if (atlasID != -1) mAtlas = EditorUtility.InstanceIDToObject(atlasID) as UIAtlas;

		int fontID = PlayerPrefs.GetInt("NGUI Font", -1);
		if (fontID != -1) mFont = EditorUtility.InstanceIDToObject(fontID) as UIFont;

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
	}

	/// <summary>
	/// Atlas selection function.
	/// </summary>

	void OnSelectAtlas (MonoBehaviour obj)
	{
		UIAtlas a = obj as UIAtlas;

		if (mAtlas != a)
		{
			mAtlas = a;
			Save();
			Repaint();
		}
	}

	/// <summary>
	/// Font selection function.
	/// </summary>

	void OnSelectFont (MonoBehaviour obj)
	{
		UIFont f = obj as UIFont;

		if (mFont != f)
		{
			mFont = f;
			Save();
			Repaint();
		}
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
			Undo.RegisterSceneUndo("Add a Widget");
			return true;
		}
		return false;
	}

	/// <summary>
	/// Label creation function.
	/// </summary>

	void CreateLabel (GameObject go)
	{
		if (ShouldCreate(go, mFont != null))
		{
			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
			lbl.font = mFont;
			lbl.text = "New Label";
			lbl.MakePixelPerfect();
			Selection.activeGameObject = lbl.gameObject;
		}
	}

	/// <summary>
	/// Tiled Sprite creation function.
	/// </summary>

	void CreateSprite<T> (GameObject go, ref string field) where T : UISprite
	{
		if (mAtlas != null)
		{
			GUILayout.BeginHorizontal();
			string sp = UISpriteInspector.SpriteField(mAtlas, "Sprite", field, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sprite that will be created");
			GUILayout.EndHorizontal();

			if (sp != field)
			{
				field = sp;
				Save();
			}
		}

		if (ShouldCreate(go, mAtlas != null))
		{
			T sprite = NGUITools.AddWidget<T>(go);
			sprite.atlas = mAtlas;
			sprite.spriteName = field;
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
		if (mAtlas != null)
		{
			GUILayout.BeginHorizontal();
			string bg = UISpriteInspector.SpriteField(mAtlas, "Background", mButton, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sliced Sprite for the background");
			GUILayout.EndHorizontal();
			if (mButton != bg) { mButton = bg; Save(); }
		}

		if (ShouldCreate(go, mAtlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Button";

			UISlicedSprite bg = NGUITools.AddWidget<UISlicedSprite>(go);
			bg.depth = depth;
			bg.atlas = mAtlas;
			bg.spriteName = mButton;
			bg.transform.localScale = new Vector3(150f, 40f, 1f);
			bg.MakePixelPerfect();

			if (mFont != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.font = mFont;
				lbl.text = go.name;
				lbl.MakePixelPerfect();
			}

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the scripts
			go.AddComponent<UIButtonColor>().tweenTarget = bg.gameObject;
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
		if (mAtlas != null)
		{
			GUILayout.BeginHorizontal();
			string bg = UISpriteInspector.SpriteField(mAtlas, "Normal", mImage0, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Normal state sprite");
			GUILayout.EndHorizontal();
			if (mImage0 != bg) { mImage0 = bg; Save(); }

			GUILayout.BeginHorizontal();
			bg = UISpriteInspector.SpriteField(mAtlas, "Hover", mImage1, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Hover state sprite");
			GUILayout.EndHorizontal();
			if (mImage1 != bg) { mImage1 = bg; Save(); }

			GUILayout.BeginHorizontal();
			bg = UISpriteInspector.SpriteField(mAtlas, "Pressed", mImage2, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Pressed state sprite");
			GUILayout.EndHorizontal();
			if (mImage2 != bg) { mImage2 = bg; Save(); }
		}

		if (ShouldCreate(go, mAtlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Image Button";

			UIAtlas.Sprite sp = mAtlas.GetSprite(mImage0);
			UISprite sprite = (sp.inner == sp.outer) ? NGUITools.AddWidget<UISprite>(go) :
				(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

			sprite.depth = depth;
			sprite.atlas = mAtlas;
			sprite.spriteName = mImage0;
			sprite.transform.localScale = new Vector3(150f, 40f, 1f);
			sprite.MakePixelPerfect();

			if (mFont != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.font = mFont;
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
		if (mAtlas != null)
		{
			GUILayout.BeginHorizontal();
			string bg = UISpriteInspector.SpriteField(mAtlas, "Background", mCheckBG, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sliced Sprite for the background");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			string ck = UISpriteInspector.SpriteField(mAtlas, "Checkmark", mCheck, GUILayout.Width(200f));
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

		if (ShouldCreate(go, mAtlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Checkbox";

			UISlicedSprite bg = NGUITools.AddWidget<UISlicedSprite>(go);
			bg.depth = depth;
			bg.atlas = mAtlas;
			bg.spriteName = mCheckBG;
			bg.transform.localScale = new Vector3(26f, 26f, 1f);
			bg.MakePixelPerfect();

			UISprite fg = NGUITools.AddWidget<UISprite>(go);
			fg.atlas = mAtlas;
			fg.spriteName = mCheck;
			fg.MakePixelPerfect();

			if (mFont != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.font = mFont;
				lbl.text = go.name;
				lbl.pivot = UIWidget.Pivot.Left;
				lbl.transform.localPosition = new Vector3(16f, 0f, 0f);
				lbl.MakePixelPerfect();
			}

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the scripts
			go.AddComponent<UICheckbox>().checkSprite = fg;
			go.AddComponent<UIButtonColor>().tweenTarget = bg.gameObject;
			go.AddComponent<UIButtonScale>().tweenTarget = bg.transform;
			go.AddComponent<UIButtonSound>();

			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Progress bar creation function.
	/// </summary>

	void CreateSlider (GameObject go, bool slider)
	{
		if (mAtlas != null)
		{
			GUILayout.BeginHorizontal();
			string bg = UISpriteInspector.SpriteField(mAtlas, "Background", mSliderBG, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sprite for the background (empty bar)");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			string fg = UISpriteInspector.SpriteField(mAtlas, "Foreground", mSliderFG, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sprite for the foreground (full bar)");
			GUILayout.EndHorizontal();

			string tb = mSliderTB;

			if (slider)
			{
				GUILayout.BeginHorizontal();
				tb = UISpriteInspector.SpriteField(mAtlas, "Thumb", mSliderTB, GUILayout.Width(200f));
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

		if (ShouldCreate(go, mAtlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = slider ? "Slider" : "Progress Bar";

			// Background sprite
			UIAtlas.Sprite bgs = mAtlas.GetSprite(mSliderBG);
			UISprite back = (bgs.inner == bgs.outer) ?
				(UISprite)NGUITools.AddWidget<UISprite>(go) :
				(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

			back.name = "Background";
			back.depth = depth;
			back.pivot = UIWidget.Pivot.Left;
			back.atlas = mAtlas;
			back.spriteName = mSliderBG;
			back.transform.localScale = new Vector3(200f, 30f, 1f);
			back.MakePixelPerfect();

			// Fireground sprite
			UIAtlas.Sprite fgs = mAtlas.GetSprite(mSliderFG);
			UISprite front = (fgs.inner == fgs.outer) ?
				(UISprite)NGUITools.AddWidget<UIFilledSprite>(go) :
				(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

			front.name = "Foreground";
			front.pivot = UIWidget.Pivot.Left;
			front.atlas = mAtlas;
			front.spriteName = mSliderFG;
			front.transform.localScale = new Vector3(200f, 30f, 1f);
			front.MakePixelPerfect();

			// Add a collider
			if (slider) NGUITools.AddWidgetCollider(go);

			// Add the slider script
			UISlider uiSlider = go.AddComponent<UISlider>();
			uiSlider.foreground = front.transform;

			// Thumb sprite
			if (slider)
			{
				UIAtlas.Sprite tbs = mAtlas.GetSprite(mSliderTB);
				UISprite thb = (tbs.inner == tbs.outer) ?
					(UISprite)NGUITools.AddWidget<UISprite>(go) :
					(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

				thb.name = "Thumb";
				thb.atlas = mAtlas;
				thb.spriteName = mSliderTB;
				thb.transform.localPosition = new Vector3(200f, 0f, 0f);
				thb.transform.localScale = new Vector3(20f, 40f, 1f);
				thb.MakePixelPerfect();

				NGUITools.AddWidgetCollider(thb.gameObject);
				thb.gameObject.AddComponent<UIButtonColor>();
				thb.gameObject.AddComponent<UIButtonScale>();

				uiSlider.thumb = thb.transform;
			}

			// Select the slider
			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Input field creation function.
	/// </summary>

	void CreateInput (GameObject go)
	{
		if (mAtlas != null)
		{
			GUILayout.BeginHorizontal();
			string bg = UISpriteInspector.SpriteField(mAtlas, "Background", mInputBG, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Sliced Sprite for the background");
			GUILayout.EndHorizontal();
			if (mInputBG != bg) { mInputBG = bg; Save(); }
		}

		if (ShouldCreate(go, mAtlas != null && mFont != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Input";

			float padding = 3f;

			UISlicedSprite bg = NGUITools.AddWidget<UISlicedSprite>(go);
			bg.depth = depth;
			bg.atlas = mAtlas;
			bg.spriteName = mInputBG;
			bg.pivot = UIWidget.Pivot.Left;
			bg.transform.localScale = new Vector3(400f, mFont.size + padding * 2f, 1f);
			bg.MakePixelPerfect();

			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
			lbl.font = mFont;
			lbl.pivot = UIWidget.Pivot.Left;
			lbl.transform.localPosition = new Vector3(padding, 0f, 0f);
			lbl.multiLine = false;
			lbl.supportEncoding = false;
			lbl.lineWidth = 400f - padding * 2f;
			lbl.text = "You can type here";
			lbl.MakePixelPerfect();

			// Add a collider to the background
			NGUITools.AddWidgetCollider(bg.gameObject);

			// Add an input script to the background and have it point to the label
			UIInput input = bg.gameObject.AddComponent<UIInput>();
			input.label = lbl;

			// Update the selection
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
			ComponentSelector.Draw<UIAtlas>(mAtlas, OnSelectAtlas, GUILayout.Width(140f));
			GUILayout.Label("Texture atlas used by widgets", GUILayout.MinWidth(10000f));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			ComponentSelector.Draw<UIFont>(mFont, OnSelectFont, GUILayout.Width(140f));
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
			}
		}
	}
}