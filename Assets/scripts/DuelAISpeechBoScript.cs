/*
 * Author: Stuart Harrison 
 * Date: April 2016
 * Class for handling connection to the web server/cloud storage space
 * Gets & Saves account and card data used within the game
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DuelAISpeechBoScript : MonoBehaviour {

	public Image ai_displayPicture;
	public Text ai_displayText;

	private float timer = 0.0f;
	private float defaultTimer = 2.5f; //2 1/2 seconds

	void Update () {
		if (gameObject.activeInHierarchy) {
			timer -= 1 * Time.deltaTime;
			if (timer <= 0) {
				gameObject.SetActive (false);
			}
		}
	}

	/// <summary>
	/// Displays the MessageBox with the AI's message and picture, which stay
	/// on the screen for the default time
	/// </summary>
	/// <param name="aiDisplayPicture">Ai display picture.</param>
	/// <param name="aiMessage">Ai message.</param>
	public void DisplayMessageBox(Sprite aiDisplayPicture, string aiMessage) {
		timer = defaultTimer; //Assign default time
		ai_displayPicture.sprite = aiDisplayPicture;
		ai_displayText.text = aiMessage;
	}

	/// <summary>
	/// Displays the MessageBox with the AI's message and picture, which stay
	/// on the screen for the specified time
	/// </summary>
	/// <param name="aiDisplayPicture">Ai display picture.</param>
	/// <param name="aiMessage">Ai message.</param>
	/// <param name="timeOnDisplay">Time to display.</param>
	public void DisplayMessageBox(Sprite aiDisplayPicture, string aiMessage, 
		float timeOnDisplay) {
		timer = timeOnDisplay; //Assign a specific time
		ai_displayPicture.sprite = aiDisplayPicture;
		ai_displayText.text = aiMessage;
	}
}