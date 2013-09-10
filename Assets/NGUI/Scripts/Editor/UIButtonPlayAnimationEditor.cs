//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIButtonPlayAnimation))]
public class UIButtonPlayAnimationEditor : Editor
{
	enum ResetOnPlay
	{
		Continue,
		StartFromBeginning,
	}

	enum SelectedObject
	{
		KeepCurrent,
		SetToNothing,
	}

	public override void OnInspectorGUI ()
	{
		EditorGUIUtility.LookLikeControls(120f);
		UIButtonPlayAnimation pa = target as UIButtonPlayAnimation;
		GUILayout.Space(6f);

		GUI.changed = false;
		Animation anim = (Animation)EditorGUILayout.ObjectField("Target", pa.target, typeof(Animation), true);

		GUILayout.BeginHorizontal();
		string clipName = EditorGUILayout.TextField("Clip Name", pa.clipName);
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		AnimationOrTween.Trigger trigger = (AnimationOrTween.Trigger)EditorGUILayout.EnumPopup("Trigger condition", pa.trigger);
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		AnimationOrTween.Direction dir = (AnimationOrTween.Direction)EditorGUILayout.EnumPopup("Play direction", pa.playDirection);
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();

		SelectedObject so = pa.clearSelection ? SelectedObject.SetToNothing : SelectedObject.KeepCurrent;

		GUILayout.BeginHorizontal();
		bool clear = (SelectedObject)EditorGUILayout.EnumPopup("Selected object", so) == SelectedObject.SetToNothing;
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		AnimationOrTween.EnableCondition enab = (AnimationOrTween.EnableCondition)EditorGUILayout.EnumPopup("If disabled on start", pa.ifDisabledOnPlay);
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();

		ResetOnPlay rs = pa.resetOnPlay ? ResetOnPlay.StartFromBeginning : ResetOnPlay.Continue;

		GUILayout.BeginHorizontal();
		bool reset = (ResetOnPlay)EditorGUILayout.EnumPopup("If already playing", rs) == ResetOnPlay.StartFromBeginning;
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		AnimationOrTween.DisableCondition dis = (AnimationOrTween.DisableCondition)EditorGUILayout.EnumPopup("When finished", pa.disableWhenFinished);
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();

		if (GUI.changed)
		{
			NGUIEditorTools.RegisterUndo("PlayAnimation Change", pa);
			pa.target = anim;
			pa.clipName = clipName;
			pa.trigger = trigger;
			pa.playDirection = dir;
			pa.clearSelection = clear;
			pa.ifDisabledOnPlay = enab;
			pa.resetOnPlay = reset;
			pa.disableWhenFinished = dis;
			UnityEditor.EditorUtility.SetDirty(pa);
		}

		EditorGUIUtility.LookLikeControls(80f);
		NGUIEditorTools.DrawEvents("On Finished", pa, pa.onFinished);
	}
}
