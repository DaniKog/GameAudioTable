using UnityEngine;
using System.Collections;

public class SFXVariation_FMOD : MonoBehaviour
{

	PlayerMovement playerScript; // A reference to the movement script
	
	// Use this for initialization
	void Start ()
	{
		playerScript = gameObject.GetComponent<PlayerMovement> (); //getting the script reference
		playerScript.PlayerMoved += PlaySFX; //Assigning the PlayerMoved event to PlaySFX() Funciton. So every time PlayerMoved event is fired PlaySFX() Function is fired too
	}
	
	void PlaySFX()
	{
		FMOD_StudioSystem.instance.PlayOneShot ("event:/RoboMove",gameObject.transform.position); // Play one shot SFX from the FMOD engine
	}

	void OnDestroy()
	{
		playerScript.PlayerMoved -= PlaySFX; //Unassign the event
	}
}
