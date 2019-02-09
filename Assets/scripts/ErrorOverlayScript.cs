/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Temporary script for displaying a generic error message to the player
*/

using UnityEngine;
using System.Collections;

public class ErrorOverlayScript : MonoBehaviour {

	public bool switch_ShowOverlay;
	public GUISkin guiSkin;
	public string errorMessage;

	/// <summary>
	/// Called from any class that needs to display an error message on the
	/// screen.
	/// </summary>
	/// <param name="message">Message to be displayed.</param>
	void DisplayErrorMessage(string message) {
		errorMessage = message;
		switch_ShowOverlay = true;
	}

	void OnGUI() {
		GUI.skin = guiSkin;
		if (switch_ShowOverlay) {
			//Create a box to 'darken' the background
			GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
			//Then make the error message label show
			GUI.color = Color.red;
			GUI.Label (new Rect ((Screen.width / 2) - 225, Screen.height / 2, 
				550, 50), errorMessage);
			//Display the button to then close the GUI
			GUI.color = Color.white;
			if (GUI.Button (new Rect ((Screen.width / 2) - 75, 
				(Screen.height / 2 + 75), 150, 50), "Close")) {
				switch_ShowOverlay = false;
			}
		}
	}
}