using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UILabels.
/// </summary>

[CustomEditor(typeof(UILabel))]
public class UILabelInspector : UIWidgetInspector
{
	UILabel mLabel;
	bool mShow = false;

	/// <summary>
	/// Font selection callback.
	/// </summary>

	void OnSelectFont (MonoBehaviour obj)
	{
		Undo.RegisterUndo(mLabel, "Font Selection");

		if (mLabel != null)
		{
			mLabel.font = obj as UIFont;
			EditorUtility.SetDirty(mLabel.gameObject);
		}
	}

	override protected bool OnCustomStart ()
	{
		mLabel = mWidget as UILabel;
		UIFont font = ComponentSelector.Draw<UIFont>(mLabel.font, OnSelectFont);
		if (font != mLabel.font) OnSelectFont(font);
		if (mLabel.font == null) return false;

		string text = EditorGUILayout.TextField("Text", mLabel.text);
		if (!string.Equals(text, mLabel.text)) { RegisterUndo(); mLabel.text = text; }

		bool encoding = EditorGUILayout.Toggle("Encoding", mLabel.supportEncoding, GUILayout.Width(100f));
		if (encoding != mLabel.supportEncoding) { RegisterUndo(); mLabel.supportEncoding = encoding; }

		GUITools.DrawSeparator();
		return true;
	}

	override protected void OnCustomEnd ()
	{
		Texture2D tex = mLabel.mainTexture;

		if (tex != null)
		{
			mShow = EditorGUILayout.Toggle("Show Atlas", mShow, GUILayout.Width(110f));

			if (mShow)
			{
				// Draw the atlas
				EditorGUILayout.Separator();
				GUITools.DrawSprite(tex, mLabel.font.uvRect, mUseShader ? mLabel.font.material : null);

				// Sprite size label
				Rect rect = GUILayoutUtility.GetRect(Screen.width, 18f);
				EditorGUI.DropShadowLabel(rect, "Font Size: " + mLabel.font.size);
			}
		}
	}
}