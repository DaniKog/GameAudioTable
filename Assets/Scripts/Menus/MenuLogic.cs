using UnityEngine;
using System.Collections;


public class MenuLogic : MonoBehaviour
{
	void Start()
	{
		DontDestroyOnLoad (gameObject);
	}
	
	public void GoToSFXMenu()
	{
		Application.LoadLevel ("SFXMenu");
	}
	public void GoTo3DMenu()
	{
		
	}
	public void GoToMusicMenu()
	{
		
	}

	public void BackToMain()
	{
		Application.LoadLevel ("MainMenu");
	}
	public void Quit()
	{
		Application.Quit ();
	}

	#region SFX Menu
	public void SFX_ChooseVariation()
	{
		Application.LoadLevel("SFX_Variation_Native");
	}
	public void SFX_ChoosePitchMod()
	{
		Application.LoadLevel("SFX_PitchMod_Native");
	}
	#endregion

}
