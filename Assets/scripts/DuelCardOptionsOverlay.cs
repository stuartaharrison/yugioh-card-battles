/*
 * Author: Stuart Harrison 
 * Date: April 2016
 * Script for controlling the Overlay that gives the player control over the
 * functions that there monsters can do in the game. Like change battle position,
 * attack, flip face-up, or activate its effect
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DuelCardOptionsOverlay : MonoBehaviour {

	public GameObject duelController;
	public GameObject ui_detailsOverlay;
	public GameObject ui_attackOverlay;
	public Button btn_CardAttack;
	public Button btn_CardPosition;
	public Button btn_CardFlip;
	public Button btn_CardActivate;

	private DuelMonsterZoneScript selectedZone;
	private DuelSceneScript duelControllerScript;
	private DuelAttackOverlayScript attackOverlayScript;
	private GameObject glob_audiomanager;

	void Start () {
		//Get the audio manager for playing the button noises when a button
		//is pressed
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");
		//Get the DuelSceneScript
		duelControllerScript = duelController.GetComponent<DuelSceneScript> ();
		//Also need the Attacking overlay Script for when the Player decides
		//to make an attack
		attackOverlayScript = 
			ui_attackOverlay.GetComponent<DuelAttackOverlayScript> ();
	}

	void Update () {
		//When this object is active in the scene, then we need to make checks
		//as to what the user/player can do with the card they have selected
		if (gameObject.activeInHierarchy) {
			//Player can attack when it's the battle phase, the monster is not
			//in defence mode, and it hasn't already made an attack this turn
			if (!selectedZone.isFaceDown && !selectedZone.isDefenceMode &&
				!selectedZone.hasAttacked &&
				(duelControllerScript.canPlayerAttack ||
				duelControllerScript.canPlayerDirectAttack)) {
				btn_CardAttack.interactable = true;
			}
			else {
				//Failed checks, make it non-interactable
				btn_CardAttack.interactable = false;
			}

			//Can only change it to defence mode if it hasn't attack this turn,
			//the monster has been on the field longer than a turn and hasn't
			//already changed battle position
			if (!selectedZone.isFaceDown && selectedZone.turnsOnField >= 1 &&
				!selectedZone.hasChangedPosition && !selectedZone.hasAttacked) {
				btn_CardPosition.interactable = true;
			}
			else {
				btn_CardPosition.interactable = false;
			}

			//You can only flip a monster if it's face-down and been on the
			//field for longer than a turn
			if (selectedZone.isFaceDown && selectedZone.turnsOnField >= 1) {
				btn_CardFlip.interactable = true;
			}
			else {
				btn_CardFlip.interactable = false;
			}
		}
	}

	/// <summary>
	/// Shows the card options overlay when a monster zone has been selected
	/// </summary>
	/// <param name="selectedCard">Selected card.</param>
	public void ShowCardOptionsOverlay(DuelMonsterZoneScript selectedCard) {
		selectedZone = selectedCard; //Assign the selected card to the var
		//so when one of the button functions is called it knows which card
		//to check/use
	}

	/// <summary>
	/// Player has decided to make an attack. Show the Attack Overlay that has
	/// been assigned in the Unity Editor
	/// </summary>
	public void btn_Attack() {
		glob_audiomanager.SendMessage ("PlayClickA");
		Debug.Log ("DuelCardOptionsOverlay: Attacking button with card " +
			selectedZone.ZoneCard.Name);
		//Mark the monster has having attack so that it cannot make a second
		//or even third attack!
		selectedZone.hasAttacked = true;
		//If the AI's field is open then we can just go and reduce our opponents
		//life points by the monsters attack
		if (duelControllerScript.canPlayerDirectAttack) {
			//We can directly attack with this monster
			Debug.Log ("DuelCardOptionsOverlay: Can make a direct attack");
			duelControllerScript.StartCoroutine(
				duelControllerScript.IPlayerBattle (
				selectedZone, false));
		}
		else { //Otherwise the AI has monsters and we need the player to select
			//and attack target
			Debug.Log ("DuelCardOptionsOverlay: Cannot make a direct attack");
			//Assign targeted monster and set the AttackOverlay active
			attackOverlayScript.StartTargetSelection (selectedZone);
			ui_attackOverlay.SetActive (true);
		}
		//Close the card details/options overlay
		ui_detailsOverlay.SetActive (false);
		gameObject.SetActive (false);
	}

	/// <summary>
	/// Changes the battle position of the monster. From either Attack mode
	/// into Defence mode. Or Defence mode into Attack mode
	/// </summary>
	public void btn_ChangePosition() {
		glob_audiomanager.SendMessage ("PlayClickA");
		Debug.Log ("DuelCardOptionsOverlay: Changing card position " +
			selectedZone.ZoneCard.Name);
		//Start the animation/sequence for changing the cards field position
		selectedZone.StartCoroutine (selectedZone.IChangePosition ());
		//Close this overlay and the details
		ui_detailsOverlay.SetActive (false);
		gameObject.SetActive (false);
	}

	/// <summary>
	/// Flips the selected face-down card into face-up attack position
	/// </summary>
	public void btn_Flip() {
		glob_audiomanager.SendMessage ("PlayClickA");
		Debug.Log ("DuelCardOptionsOverlay: Flipping button with card " +
			selectedZone.ZoneCard.Name);
		//Start the animation/sequence
		selectedZone.StartCoroutine (selectedZone.IFlipCardUp ());
		ui_detailsOverlay.SetActive (false);
		gameObject.SetActive (false);
	}

	/// <summary>
	/// Button method call to activate the monsters effect.
	/// TODO: Finish
	/// </summary>
	public void btn_Activate() {
		glob_audiomanager.SendMessage ("PlayClickA");
		ui_detailsOverlay.SetActive (false);
		gameObject.SetActive (false);
	}
}