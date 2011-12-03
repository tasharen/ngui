using UnityEngine;

/// <summary>
/// Change the shader level-of-detail to match the quality settings.
/// Also allows changing of the quality level from within the editor without having
/// to open up the quality preferences, seeing the results right away.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Examples/Editor Quality Settings")]
public class EditorQualitySettings : MonoBehaviour
{
	public QualityLevel qualityLevel = QualityLevel.Fantastic;

	void Update ()
	{
		if (qualityLevel != QualitySettings.currentLevel)
		{
			QualitySettings.currentLevel = qualityLevel;
			Shader.globalMaximumLOD = ((int)qualityLevel + 1) * 100;
		}
	}
}
