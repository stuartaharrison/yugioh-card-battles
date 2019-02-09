/*
 * Author: Stuart Harrison 
 * Date: April 2016
 * Short script used for handling the display of the top card in each players
 * graveyard
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DuelGraveyardScript : MonoBehaviour {

	public bool forAi; //Assigned in Unity Editor
	public GameObject duel_SceneController;
	public GameObject ui_CardImage;
	public Image topCardPicture;

	private Card assignedCard = null;
	private DuelSceneScript duel_Scene;

	void Start () {
		duel_Scene = duel_SceneController.GetComponent<DuelSceneScript> ();
	}

	void Update () {
		//If this Script is for the AI's graveyard, then we check that list
		if (forAi) {
			if (duel_Scene.duel_ai.ai_graveyard.Count > 0) {
				assignedCard = duel_Scene.duel_ai.
					ai_graveyard [duel_Scene.duel_ai.ai_graveyard.Count - 1];
				topCardPicture.sprite = assignedCard.Image;
			}
		}
		else { //Otherwise we check the players graveyard list
			if (duel_Scene.player_Graveyard.Count > 0) {
				assignedCard = duel_Scene.
					player_Graveyard[duel_Scene.player_Graveyard.Count - 1];
				topCardPicture.sprite = assignedCard.Image;
			}
		}
		//If we get an image we can display it!
		if (assignedCard != null) {
			ui_CardImage.SetActive (true);
		}
		else {
			ui_CardImage.SetActive (false);
		}
	}
}