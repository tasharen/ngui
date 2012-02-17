using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Inspector class used to edit UILabels.
/// </summary>

[CustomEditor(typeof(UILabel))]
public class UILabelInspector : UIWidgetInspector
{
	UILabel mLabel;

	/// <summary>
	/// Font selection callback.
	/// </summary>

	void OnSelectFont (MonoBehaviour obj)
	{
		if (mLabel != null)
		{
			NGUIEditorTools.RegisterUndo("Font Selection", mLabel);
			bool resize = (mLabel.font == null);
			mLabel.font = obj as UIFont;
			if (resize) mLabel.MakePixelPerfect();
		}
	}

	override protected void OnInit () { mShowTexture = false; }

	override protected bool OnDrawProperties ()
	{
		mLabel = mWidget as UILabel;
		ComponentSelector.Draw<UIFont>(mLabel.font, OnSelectFont);
		if (mLabel.font == null) return false;

		string text = EditorGUILayout.TextArea(mLabel.text, GUILayout.Height(100f));
		if (!text.Equals(mLabel.text)) { RegisterUndo(); mLabel.text = text; }

		GUILayout.BeginHorizontal();
		{
			float len = EditorGUILayout.FloatField("Line Width", mLabel.lineWidth, GUILayout.Width(120f));
			if (len != mLabel.lineWidth) { RegisterUndo(); mLabel.lineWidth = len; }

			bool multi = EditorGUILayout.Toggle("Multi-line", mLabel.multiLine, GUILayout.Width(100f));
			if (multi != mLabel.multiLine) { RegisterUndo(); mLabel.multiLine = multi; } 
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		{
			bool password = EditorGUILayout.Toggle("Password", mLabel.password, GUILayout.Width(120f));
			if (password != mLabel.password) { RegisterUndo(); mLabel.password = password; }

			bool encoding = EditorGUILayout.Toggle("Encoding", mLabel.supportEncoding, GUILayout.Width(100f));
			if (encoding != mLabel.supportEncoding) { RegisterUndo(); mLabel.supportEncoding = encoding; }
		}
		GUILayout.EndHorizontal();
		return true;
	}

	override protected void OnDrawTexture ()
	{
		Texture2D tex = mLabel.mainTexture as Texture2D;

		if (tex != null)
		{
			// Draw the atlas
			EditorGUILayout.Separator();
			NGUIEditorTools.DrawSprite(tex, mLabel.font.uvRect, mUseShader ? mLabel.font.material : null);

			// Sprite size label
			Rect rect = GUILayoutUtility.GetRect(Screen.width, 18f);
			EditorGUI.DropShadowLabel(rect, "Font Size: " + mLabel.font.size);
		}
	}
}