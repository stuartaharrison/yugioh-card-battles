/*
 * Author: Stuart Harrison 
 * Date: March 2016
 * Class for control a single monster zone on the either the AI or Player's
 * side of the field.
*/

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DuelMonsterZoneScript : MonoBehaviour {

	public bool isFaceDown;
	public bool isDefenceMode;
	public bool hasAttacked;
	public bool hasChangedPosition;
	public int turnsOnField = 0;
	public GameObject duelController;
	public GameObject ui_attackPosition;
	public GameObject ui_defencePosition;
	public GameObject ui_summonCircle;
	public GameObject ui_menuOverlay;
	public GameObject ui_cardDetailsOverlay;
	public GameObject ui_cardOptionsOverlay;
	public Image attackPosition;
	public Image defencePosition;
	public Image ui_cardImage;
	public Sprite faceDownImage;
	public Text ui_cardText;
	public Text ui_cardStrengthText;

	private bool isSettingCard = false;
	private bool isSummoningCard = false;
	private Card cardOnField;
	private DuelCardOptionsOverlay optionsOverlayScript;
	private DuelSceneScript duelControllerScript;
	private GameObject glob_audiomanager;

	public Card ZoneCard { get { return cardOnField; } }

	void Start () {
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");
		duelControllerScript = duelController.GetComponent<DuelSceneScript> ();
		optionsOverlayScript = ui_cardOptionsOverlay.
			GetComponent<DuelCardOptionsOverlay> ();
	}

	void Update () {
		//Only check if the object is active
		if (gameObject.activeInHierarchy) {
			if (isSummoningCard && cardOnField != null) {
				//When a monster is being summoned on this zone
				isSummoningCard = false;
				StartCoroutine (ISummonCard ());
			}
			if (isSettingCard && cardOnField != null) {
				//When a monster is being set on this zone
				isSettingCard = false;
				StartCoroutine (ISetCard ());
			}
		}
	}

	/// <summary>
	/// Command for displaying the card details that are on the field and in
	/// this zone
	/// </summary>
	public void btn_ShowCardDetails() {
		glob_audiomanager.SendMessage ("PlayClickA");
		ui_cardOptionsOverlay.SetActive (false);
		//Only display if this zone actually has a card
		if (cardOnField != null) {
			//default display
			Sprite cardImage = faceDownImage;
			string cardDetails = "?????????";
	
			if (!isFaceDown || 
				duelControllerScript.ui_playerMonsterZones.Contains(this)) {
				//We can display the card details of AI monsters too! But if
				//they are a players card and is facedown then we can also
				//display the data. We should prevent the player from being
				//able to select and see the AI's face down cards
				cardImage = cardOnField.Image;
				cardDetails = cardOnField.OutputCardDetails ();
			}

			if (duelControllerScript.isPlayerTurn && 
				duelControllerScript.ui_playerMonsterZones.Contains(this)) {
				//When it is the players turn and the monster zone selected
				//contains this script, then display the options overlay
				//that allows the player to perform functions on that card
				optionsOverlayScript.ShowCardOptionsOverlay (this);
				ui_cardOptionsOverlay.SetActive (true);
			}
			//Display the overlay
			ui_cardImage.sprite = cardImage;
			ui_cardText.text = cardDetails;
			ui_menuOverlay.SetActive (false);
			ui_cardDetailsOverlay.SetActive (true);
		}
	}

	/// <summary>
	/// Method called from outside the script that begins placing a card
	/// on the zone the script is set too
	/// </summary>
	/// <param name="card">Card to place on the field</param>
	/// <param name="isFD">If facedown on the field to <c>true</c> is F.</param>
	public void PlaceCardOnZone(Card card, bool isFD) {
		cardOnField = card;
		//Check if the card is being set face down or not
		if (isFD) {
			isSettingCard = true; //Setting the card facedown
		}
		else {
			isSummoningCard = true; //Summoning the monster in attack mode
		}
	}

	/// <summary>
	/// Coroutine for placing the scanned in card onto the field in face-up
	/// attack position
	/// </summary>
	/// <returns>void</returns>
	public IEnumerator ISummonCard() {
		//The card is not face-down or in defence mode
		isFaceDown = false;
		isDefenceMode = false;
		//Play audio for summoning
		glob_audiomanager.SendMessage ("PlayDuelCardActivate");
		yield return new WaitForSeconds (1.0f);
		//Show the summon circle animated image
		ui_summonCircle.SetActive (true);
		glob_audiomanager.SendMessage ("PlayDuelCardSummon");
		//Play the next audio
		yield return new WaitForSeconds (2.1f);
		//Set the image of the attack position image to the monster being summoned
		attackPosition.sprite = cardOnField.Image;
		ui_cardStrengthText.text = ((Monster)cardOnField).Attack.ToString() + 
			"/" + ((Monster)cardOnField).Defence.ToString ();
		//Assign the UI label associated with the monster zone to the attack
		//and defence of the monster being summoned
		//Display the image and text
		ui_attackPosition.SetActive (true);
		ui_defencePosition.SetActive (false);
		ui_summonCircle.SetActive (false); //hide the summon circle animation
		glob_audiomanager.SendMessage ("PlayLPCounterAppears");
		//Play final audio
		//Stop the user from being able to summon again this turn
		duelControllerScript.canPlayerSummon = false;
		duelControllerScript.hasPlayerSummoned = true;
		yield return new WaitForSeconds (1.0f);
	}

	/// <summary>
	/// Coroutine for placing scanned card onto the field face down 
	/// in defence position
	/// </summary>
	/// <returns>void</returns>
	public IEnumerator ISetCard() {
		//Tell the zone that the card is a face down and in defence position
		isFaceDown = true;
		isDefenceMode = true;
		//Play some audio
		glob_audiomanager.SendMessage ("PlayDuelCardSet");
		yield return new WaitForSeconds (0.5f);
		//Set the image to the defined image of a face-down card
		defencePosition.sprite = faceDownImage;
		//Display the image
		ui_attackPosition.SetActive (false);
		ui_defencePosition.SetActive (true);
		//Stop the player from being able to summon again this turn
		duelControllerScript.canPlayerSummon = false;
		duelControllerScript.hasPlayerSummoned = true;
		yield return new WaitForSeconds (0.5f);
	}

	/// <summary>
	/// Coroutine that changes the battle position of the monster in the card
	/// zone
	/// </summary>
	/// <returns>void</returns>
	public IEnumerator IChangePosition() {
		if (isDefenceMode) {
			//We are in defence mode and want to change to Attack mode
			isDefenceMode = false;
			attackPosition.sprite = cardOnField.Image;
			defencePosition.sprite = null;
			ui_attackPosition.SetActive (true);
			ui_defencePosition.SetActive (false);
		}
		else {
			//We are in attack mode and want to change to Defence mode
			isDefenceMode = true;
			attackPosition.sprite = null;
			defencePosition.sprite = cardOnField.Image;
			ui_attackPosition.SetActive (false);
			ui_defencePosition.SetActive (true);
		}
		hasChangedPosition = true; //Prevents monsters changing position
		//from attacking
		glob_audiomanager.SendMessage ("PlayLPCounterAppears");
		yield return new WaitForSeconds (1.0f);
	}

	/// <summary>
	/// Coroutine that changes the position of a set face down monster it the
	/// zone and flips it face up
	/// </summary>
	/// <returns>void</returns>
	public IEnumerator IFlipCardUp() {
		//The card is no longer facedown
		isFaceDown = false;
		isDefenceMode = false;
		//Change the image from defence to the attackers ui image
		attackPosition.sprite = cardOnField.Image;
		defencePosition.sprite = null;
		//Display the components
		ui_attackPosition.SetActive (true);
		ui_defencePosition.SetActive (false);
		glob_audiomanager.SendMessage ("PlayLPCounterAppears");
		yield return new WaitForSeconds (1.0f);
	}

	/// <summary>
	/// Coroutine for destroying the monster in the card zone. Essentially
	/// moves the card in the card zone and puts it in the graveyard of the card
	/// owner.
	/// </summary>
	/// <returns>void</returns>
	/// <param name="isPlayerCard">If set to <c>true</c> is player card.</param>
	public IEnumerator IDestroyCard(bool isPlayerCard) {
		//Decide, which card it was that got destroyed
		if (isPlayerCard) {
			duelControllerScript.player_Graveyard.Add (cardOnField);
		}
		else {
			duelControllerScript.duel_ai.ai_graveyard.Add (cardOnField);
		}
		//Reset the card zone components
		cardOnField = null;
		attackPosition.sprite = null;
		defencePosition.sprite = null;
		ui_attackPosition.SetActive (false);
		ui_defencePosition.SetActive (false);
		glob_audiomanager.SendMessage ("PlayDuelMonsterDestroyed");
		yield return new WaitForSeconds (2.1f);
	}
}