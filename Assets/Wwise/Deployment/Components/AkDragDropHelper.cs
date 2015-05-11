#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

/// <summary>
///  @brief This class is used to perform DragAndDrop operations from the AkWwisePicker to any GameObject.
///  We found out that DragAndDrop operations in Unity do not transfer components, but only scripts. This
///  prevented us to set the name and ID of our components before performing the drag and drop. To fix this,
///  the DragAndDrop operation always transfers a AkDragDropHelper component that gets instantiated on the 
///  target GameObject. On its first Update() call, it will parse the DragAndDrop structure, which contains
///  all necessary information to instantiate the correct component, with the correct information
/// </summary>
[ExecuteInEditMode]
public class AkDragDropHelper : MonoBehaviour
{
    void Awake()
    {
        // Need a minimum of 4 members in DragAndDrop.paths:
        // DragAndDrop.paths[0] contains the component's name
        // DragAndDrop.paths[1] contains the component's Guid
        // DragAndDrop.paths[2] contains the component's AkGameObjID
        // DragAndDrop.paths[3] contains the object's type
        // We need two more fields for states and switches:
        // DragAndDrop.paths[4] contains the state or switch group Guid
        // DragAndDrop.paths[5] contains the state or switch group AkGameObjID
        if( DragAndDrop.paths.Length >= 4 )
        {
            string componentGuid = DragAndDrop.paths[1];
            int ID = Convert.ToInt32(DragAndDrop.paths[2]);
            string type = DragAndDrop.paths[3];

            switch(type)
            {
                case "AuxBus":
                    CreateAuxBus(componentGuid, ID);
                    break;
                case "Event":
					CreateAmbient(componentGuid, ID);
                    break;
                case "Bank":
					CreateBank(componentGuid, DragAndDrop.paths[0]);
                    break;
                case "State":
                    if (DragAndDrop.paths.Length == 6)
                    {
                        CreateState(componentGuid, ID, DragAndDrop.paths[4], Convert.ToInt32(DragAndDrop.paths[5]));
                    }
                    break;
                case "Switch":
                    if (DragAndDrop.paths.Length == 6)
                    {
                        CreateSwitch(componentGuid, ID, DragAndDrop.paths[4], Convert.ToInt32(DragAndDrop.paths[5]));
                    }
                    break;
            }
				
			GUIUtility.hotControl = 0;
        }
    }


    
    void Start()
    {
		// Don't forget to destroy the AkDragDropHelper when we're done!
		Component.DestroyImmediate(this);
    }

    void CreateAuxBus(string componentGuid, int ID)
    {
        AkEnvironment[] akEnvironments = gameObject.GetComponents<AkEnvironment>();

        bool found = false;
        for (int i = 0; i < akEnvironments.Length; i++)
        {
			if (new System.Guid(akEnvironments[i].valueGuid) == new System.Guid(componentGuid))
			{
				found = true;
                break;
            }
        }

        if (found == false)
        {
			AkEnvironment akEnvironment = Undo.AddComponent<AkEnvironment>(gameObject);
			if (akEnvironment != null)
            {
				akEnvironment.valueGuid = new System.Guid(componentGuid).ToByteArray();
				akEnvironment.SetAuxBusID(ID);
			}
		}
	}

	void CreateAmbient(string componentGuid, int ID)
    {
		AkAmbient ambient = Undo.AddComponent<AkAmbient>(gameObject);

        if (ambient != null)
        {
			ambient.valueGuid = new System.Guid(componentGuid).ToByteArray();
            ambient.eventID = ID;
        }
    }

	void CreateBank(string componentGuid, string name)
    {
		AkBank bank = Undo.AddComponent<AkBank>(gameObject);

		if (bank != null)
        {
			bank.valueGuid = new System.Guid(componentGuid).ToByteArray();
			bank.bankName = name;
        }
    }

    void CreateState(string componentGuid, int ID, string groupGuid, int groupID)
    {
		AkState akState = Undo.AddComponent<AkState>(gameObject);
		
		if (akState != null)
        {
			akState.groupGuid = new System.Guid(groupGuid).ToByteArray();
			akState.groupID = groupID;
            akState.valueGuid = new System.Guid(componentGuid).ToByteArray();
            akState.valueID = ID;
        }
    }

    void CreateSwitch(string componentGuid, int ID, string groupGuid, int groupID)
    {
		AkSwitch akSwitch = Undo.AddComponent<AkSwitch>(gameObject);
		
		if (akSwitch != null)
        {
			akSwitch.groupGuid = new System.Guid(groupGuid).ToByteArray();
			akSwitch.groupID = groupID;
			akSwitch.valueGuid = new System.Guid(componentGuid).ToByteArray();
			akSwitch.valueID = ID;
        }
    }


}
#endif // UNITY_EDITOR
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.