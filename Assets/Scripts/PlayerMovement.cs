using UnityEngine;
using System.Collections;
using System; // This library allow us to use events

public class PlayerMovement : MonoBehaviour
{
	int currentWall = 0; // Index to know on which wall the player is at
	[System.NonSerialized] // Because CanMove is a internal bool but it is public, we make sure the this varablie is not displayed in the editor
	public bool canMove = true; // a bool to make sure that the player can trigger the move function only once he stopped moving.
	Wall[] walls; // An Array of walls to check where the player should go next
	public event Action PlayerMoved; // an Public event that fires up every time the player moves (We attach the audio to this event)

	// Use this for initialization
	void Start ()
	{
		walls = GameObject.FindObjectsOfType<Wall>(); // Initializing the array
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(canMove == true)
		{
			if(Input.GetKeyDown(KeyCode.LeftArrow))
			{
				moveChar(currentWall-1); // calling the move function when the key is pressed
			}
			if(Input.GetKeyDown(KeyCode.RightArrow))
			{
				moveChar(currentWall+1); // calling the move function when the key is pressed
			}
		}
	}

	void moveChar(int wallIndexToMove)
	{
		foreach (Wall wall in walls) 
		{
			if(wall.wallindex == wallIndexToMove) // checking where to move the character
			{
				Vector3 posToReach = wall.transform.position; // grabbing to position of that wall
				StartCoroutine (MovePlayer (posToReach)); // moving character to that wall
				PlayerMoved(); // firing up the public event action (Plays Audio on a different script)
				currentWall = wall.wallindex; // updates current wall
				canMove = false; //disables movement
				break;
			}

		}


	}

	private IEnumerator MovePlayer(Vector3 _posToReach)
	{
		Vector3 currentVelocity = Vector3.one; // creates a new Vector 3 of (1,1,1) that will determine the velocity
		do 
		{
			gameObject.transform.position = Vector3.SmoothDamp(gameObject.transform.position, _posToReach, ref currentVelocity , 0.1f); // moves chracter towards wall
			
			yield return null;
		} 
		while (Vector3.Distance(gameObject.transform.position, _posToReach) > 0.01f); // checks if character reached wall

		canMove = true; // enables movement
		yield return null; // End Coroutine
	}
}
