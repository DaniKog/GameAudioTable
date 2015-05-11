using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FMOD_StudioEventEmitter))]
public class FMODEventEmitterInspector : Editor
{
    FMOD_StudioEventEmitter emitter;
	
	bool is3D;
	float minDistance, maxDistance;
	
	void Awake()
	{
        emitter = (FMOD_StudioEventEmitter)target;
		
		is3D = false;

        if (emitter == null || emitter.asset == null)
        {
            return;
        }
		
		FMOD.Studio.EventDescription desc = FMODEditorExtension.GetEventDescription(emitter.asset.id);
		
		if (desc != null)
		{
			desc.is3D(out is3D);
			desc.getMinimumDistance(out minDistance);
			desc.getMaximumDistance(out maxDistance);
		}
	}
	
	public override void OnInspectorGUI()
	{	
		if (emitter.asset != null)
		{
			emitter.path = emitter.asset.id; // Note: set path to guid just in case the asset gets deleted
			emitter.asset = (FMODAsset)EditorGUILayout.ObjectField(emitter.asset, typeof(FMODAsset), false);
			
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Path:");
				EditorGUILayout.SelectableLabel(emitter.asset.path, GUILayout.Height(14));
			}
			GUILayout.EndHorizontal();	
			GUILayout.BeginHorizontal();	
			{
				GUILayout.Label("GUID:");
				EditorGUILayout.SelectableLabel(emitter.asset.id, GUILayout.Height(14));
			}
			GUILayout.EndHorizontal();
			
			GUILayout.Label(is3D ? "3D" : "2D");
			if (is3D)
			{
				GUILayout.Label("Distance: (" + minDistance + " - " + maxDistance + ")");
			}
			
			bool isDirty = false;
			{
				bool oldState = emitter.startEventOnAwake;
				emitter.startEventOnAwake = GUILayout.Toggle(oldState, "Start Event on Awake");
				isDirty = isDirty || (oldState != emitter.startEventOnAwake);
			}
			
			if (isDirty)
				EditorUtility.SetDirty(emitter);
		}
		else
		{
			DrawDefaultInspector();
		}
	}
}
