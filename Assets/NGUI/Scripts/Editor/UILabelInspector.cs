using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UILabels.
/// </summary>

[CustomEditor(typeof(UILabel))]
public class UILabelInspector : UIWidgetInspector
{
	UILabel mLabel;
	UIFont mFont;
	bool mShow = false;

	override protected bool OnCustomStart ()
	{
		mLabel = mWidget as UILabel;
		mFont = EditorGUILayout.ObjectField("Font", mLabel.font, typeof(UIFont), true) as UIFont;
		if (mFont == null) return false;

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
				GUITools.DrawSprite(tex, mFont.uvRect, mFont.material);

				// Sprite size label
				Rect rect = GUILayoutUtility.GetRect(Screen.width, 18f);
				EditorGUI.DropShadowLabel(rect, "Font Size: " + mFont.size);
			}
		}
	}

	override protected void OnCustomSave ()
	{
		mLabel.font = mFont;
	}
}