using UnityEngine;
using System.Collections;


public class SFXVariation_Wwise : MonoBehaviour
{
	uint bankID;
	PlayerMovement playerScript; // A reference to the movement script
	// Use this for initialization
	void Start ()
	{
		playerScript = gameObject.GetComponent<PlayerMovement> (); //getting the script reference
		playerScript.PlayerMoved += PlaySFX; //Assigning the PlayerMoved event to PlaySFX() Funciton. So every time PlayerMoved event is fired PlaySFX() Function is fired too

		AkSoundEngine.LoadBank ("ExampleBank", AkSoundEngine.AK_DEFAULT_POOL_ID, out bankID); //Load the sound bank, need to happened only once in a scene no reason to add this on every single script
		//Side note - my recommendation is to create your own handler Class and that will handle all Play, and Stop SFX function
		//For more information watch https://www.youtube.com/watch?v=j5Aq5hg1dcA
	}

	void PlaySFX()
	{
		AkSoundEngine.PostEvent ("Play_RoboSound",gameObject); //Play an event from the Wwise engine
	}
	
	void OnDestroy()
	{
		playerScript.PlayerMoved -= PlaySFX; //Unassign the event
	}
}