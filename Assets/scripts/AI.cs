/*
 * Author: Stuart Harrison 
 * Date: March 2016
 * Superclass from which all AI's within the game get their base logic and
 * variables required to function
*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AI : MonoBehaviour {

	public bool deckLoaded = false;
	public bool deckedOut = false;
	public bool ai_turnComplete = false;
	public bool hasAILost = false;
	public int ai_LifePoints;
	public int ai_liveLifePoints;
	public List<Card> ai_hand;
	public List<Card> ai_graveyard;
	public List<Card> ai_deck;
	public List<Card> ai_extradeck;
	public List<DuelMonsterZoneScript> ai_monsterzones;

	//Private controls but accessed from the subclasses
	protected AIDetails ai_details;
	protected DuelOptions duel_options;
	protected DuelSceneScript duel_controller;
	protected bool coroutine_loaddeck;
	protected bool coroutine_taketurn;
	protected bool isDownloadingDeck = false;
	protected bool canSummon = false;
	protected bool goDefensive = false;
	protected float timer_deckdownload = 0.0f;
	protected GameObject glob_gamemanager;
	protected GameObject glob_audiomanager;

	//Called when the object is instaniated
	void Start () {
		//Get the Gamemanager and Audio manager from the scene
		glob_gamemanager = GameObject.FindGameObjectWithTag ("GameManager");
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");
		ai_details = gameObject.GetComponent<AIDetails> ();
		//Get the AI details component from the same object as the one this
		//component is attached too!
		//Setup the AI field, deck, hand, ect
		ai_hand = new List<Card> ();
		ai_graveyard = new List<Card> ();
		ai_deck = new List<Card> ();
		ai_extradeck = new List<Card> ();
		ai_monsterzones = new List<DuelMonsterZoneScript> ();
	}

	//Called once per frame
	void Update () {
		#region Complete Deck Download Timer Kit
		//Although the download manager handles timing single card downloads,
		//this keeps track of the complete download of the entire AI deck
		if (isDownloadingDeck) { timer_deckdownload += 1 * Time.deltaTime; }
		if (isDownloadingDeck && deckLoaded) {
			isDownloadingDeck = false;
			Debug.Log ("AI: Finished downloading deck in " +
			timer_deckdownload + "s");
		}
		#endregion
		//When the command is sent to the AI to load the deck, and start
		//downloading all the cards
		if (coroutine_loaddeck) {
			coroutine_loaddeck = false; //Prevents coroutine from being started
			//again
			isDownloadingDeck = true; //Start timer controls
			StartCoroutine (ILoadDeck ());
		}
		//When the command is sent to the AI for it to take a turn
		if (coroutine_taketurn) {
			coroutine_taketurn = false; //Prevent it from taking multiple turns
			StartCoroutine (IAITurn ());
		}
	}

	#region AI Functionality
	/// <summary>
	/// Shuffles the AIs deck so that a random combination of cards can be
	/// had from drawing cards.
	/// </summary>
	/// <returns>The the parametered deck in random order</returns>
	/// <param name="deckToShuffle">Deck list to shuffle</param>
	protected List<Card> ShuffleDeck(List<Card> deckToShuffle) {
		List<Card> returnDeck; //Return list
		Card[] deckArray = deckToShuffle.ToArray ();
		//Put the deck list into an array for the shuffle
		System.Random rand = new System.Random (); 
		//Loop around the size of the deck
		for (int i = deckArray.Length - 1; i > 0; i--) {
			int n = rand.Next (i + 1); //New random position
			//Swap the cards
			Card temp = deckArray [i];
			deckArray [i] = deckArray [n];
			deckArray [n] = temp;
		}
		//Now put all the cards back into a deck list and return it
		returnDeck = deckArray.ToList();
		return returnDeck;
	}

	/// <summary>
	/// Draws the card from the top of the AI's deck list and add it to the
	/// AI's hand before removing it from the deck list
	/// </summary>
	/// <param name="numberToDraw">Number of cards to draw from the deck.</param>
	protected void DrawCard(int numberToDraw) {
		//Loop for the number of cards we want to draw
		for (int i = 0; i < numberToDraw; i++) {
			if (ai_deck.Count == 0) { //If we cannot draw then the AI decks out
				//This means that that the AI has lost because they cannot
				//draw anymore cards
				deckedOut = true;
				break;
			}
			else { //Otherwise we can draw card
				if (deckLoaded) { //Prevents sound effect when loading AI
					glob_audiomanager.SendMessage ("PlayCardDraw");
				}
				//Add the card to the AI hand and remove it from the deck list
				ai_hand.Add (ai_deck [0]);
				ai_deck.RemoveAt (0);
			}
		}
	}
	#endregion

	#region AI Field and Check Methods
	/// <summary>
	/// Checks to see if the Ai's monster zones are full
	/// </summary>
	/// <returns><c>true</c>if monster zones are all full
	/// <c>false</c>Otherwise</returns>
	protected bool isMonsterZonesFull() {
		bool flag = true;
		//Loop around for every AI monster zone
		foreach (DuelMonsterZoneScript zone in ai_monsterzones) {
			if (zone.ZoneCard == null) { //NULL means that no card is in that
				//'zone' and therefore it's an empty zone
				flag = false;
			}
			if (!flag) { break; } //Break from the loop if we find an empty zone
			//no point in continuing
		}
		return flag;
	}

	/// <summary>
	/// Checks if the AI can make a direct attack on the player
	/// </summary>
	/// <returns><c>true</c>If the AI can directly attack the player and no
	/// monsters exist on the players field 
	/// <c>false</c> otherwise.</returns>
	protected bool canDirectAttack() {
		bool flag = true;
		foreach (DuelMonsterZoneScript m in
			duel_controller.ui_playerMonsterZones) { //Loop around for every
			//player monster zone
			if (m.ZoneCard != null) { //Check if the field zone is empty
				flag = false; //If a monster exists then the AI cannot make
				//a direct attack
			}
		}
		return flag;
	}
		
	/// <summary>
	/// Get a list of monsters on the AI's field are able to make an attack
	/// this battle phase against the players monsters or directly attack the
	/// player
	/// </summary>
	/// <returns>The monsters available to attack.</returns>
	protected List<DuelMonsterZoneScript> GetMonstersAvailableToAttack() {
		List<DuelMonsterZoneScript> ml = new List<DuelMonsterZoneScript> ();
		//Create a return list and then loop around each monster zone of the AI's
		//field
		foreach (DuelMonsterZoneScript zScript in ai_monsterzones) {
			if (zScript.ZoneCard != null && !zScript.isDefenceMode &&
				!zScript.hasAttacked) {
				//Checks that there is a monster in that zone and it has not
				//attacked yet and a monster in defence mode cannot make an
				//attack
				ml.Add(zScript); //Add that monsterzone to the list
			}
		}
		return ml; //return the list
	}

	/// <summary>
	/// Gets a list with the monsters available on the AIs field sorted into
	/// strongest to weakest.
	/// </summary>
	/// <returns>The strongest to weakest monsters.</returns>
	/// <param name="ml">Monsterzone list to sort</param>
	protected List<DuelMonsterZoneScript> GetStrongestToWeakestMonsters (
		DuelMonsterZoneScript[] ml) {
		bool flag = true;
		for (int o = 1; (o < ml.Length) && flag; o++) { //Do an outer loop
			//around the array
			flag = false;
			for (int i = 0; i < ml.Length - 1; i++) {
				//Foreach of the numbers next to the outer loop point
				if (((Monster)ml [i + 1].ZoneCard).Attack > 
					((Monster)ml [i].ZoneCard).Attack) { //Compare the attacks
					//Swap the monsters in the array
					DuelMonsterZoneScript temp = ml [i];
					ml [i] = ml [i + 1];
					ml [i + 1] = temp;
					flag = true;
				}
			}
		}
		return ml.ToList (); //Return the array back as list
	}

	/// <summary>
	/// Gets the first empty monster zone on the AI's side of the field
	/// </summary>
	/// <returns>The first empty monster zone.</returns>
	protected DuelMonsterZoneScript GetFirstEmptyMonsterZone() {
		DuelMonsterZoneScript returnScript = null; //The returned zone
		if (!isMonsterZonesFull()) { //If the monster zones are all full
			//then no point in even going into the loop
			foreach (DuelMonsterZoneScript zone in ai_monsterzones) {
				if (zone.ZoneCard == null) { //Check if the zonecard is null
					//If it is null then it means a monster doesn't occupy
					//that space and we can return it
					returnScript = zone;
					break;
				}
			}
		}
		return returnScript;
	}

	/// <summary>
	/// Gets the highest attack monster in hand.
	/// </summary>
	/// <returns>The highest attack monster in hand.</returns>
	protected Monster GetHighestAttackMonsterInHand() {
		Monster returnCard = null;
		int flaggedStrongest = 0; //Set the base score to beat
		if (ai_hand.Count > 0) { //Check we actually have cards to check
			foreach (Card crd in ai_hand) {
				if (crd is Monster) { //Only do it if its a monster card
					//as spell or trap cards do not have attack values
					Monster temp = (Monster)crd;
					if (temp.Attack > flaggedStrongest) { //Compare to strongest
						flaggedStrongest = temp.Attack;
						returnCard = temp;
					}
				}
			}
		}
		return returnCard;
	}

	/// <summary>
	/// Gets the highest defence monster in hand.
	/// </summary>
	/// <returns>The highest defence monster in hand.</returns>
	protected Monster GetHighestDefenceMonsterInHand() {
		Monster returnCard = null;
		int flaggedStrongest = 0;
		if (ai_hand.Count > 0) {
			foreach (Card crd in ai_hand) {
				if (crd is Monster) {
					Monster temp = (Monster)crd;
					if (temp.Defence > flaggedStrongest) {
						flaggedStrongest = temp.Defence;
						returnCard = temp;
					}
				}
			}
		}
		return returnCard;
	}

	/// <summary>
	/// Gets the highest attack monster on field.
	/// </summary>
	/// <returns>The highest attack monster on field.</returns>
	protected Monster GetHighestAttackMonsterOnField() {
		Monster returnCard = null;
		int flaggedStrongest = 0;
		foreach (DuelMonsterZoneScript mon in ai_monsterzones) {
			if (mon.ZoneCard != null && !mon.isDefenceMode) {
				Monster temp = (Monster)mon.ZoneCard;
				if (temp.Attack > flaggedStrongest) {
					flaggedStrongest = temp.Attack;
					returnCard = temp;
				}
			}
		}
		return returnCard;
	}

	/// <summary>
	/// Gets the highest defence monster on field.
	/// </summary>
	/// <returns>The highest defence monster on field.</returns>
	protected Monster GetHighestDefenceMonsterOnField() {
		Monster returnCard = null;
		int flaggedStrongest = 0;
		foreach (DuelMonsterZoneScript mon in ai_monsterzones) {
			if (mon.ZoneCard != null && mon.isDefenceMode) {
				Monster temp = (Monster)mon.ZoneCard;
				if (temp.Defence > flaggedStrongest) {
					flaggedStrongest = temp.Defence;
					returnCard = temp;
				}
			}
		}
		return returnCard;
	}
	#endregion

	#region AI Turn Phases
	/// <summary>
	/// Execute all the AI logic for the DrawPhase.
	/// </summary>
	private void DrawPhase() {
		Debug.Log ("AI: Entering Draw Phase");
		//Sets the phase
		duel_controller.ChangeTurnPhase (DuelSceneScript.TurnPhase.DRAWPHASE);
		//In the new rules the person on the first turn does not draw
		if (duel_controller.turnCounter > 1) {
			Debug.Log ("AI: Drawing a card");
			//Display a message from the AI and draw the card
			duel_controller.ShowMessageFromAI (ai_details.ai_image, 
				ai_details.speech_DrawCard);
			DrawCard (1);
		}
	}

	/// <summary>
	/// Executes all the logic to be done by the AI during the Standby Phase
	/// </summary>
	private void StandbyPhase() {
		//Use this space for checking any negative effects and reducing counters
		//E.g. Reducing number of turns Swords of Revealing Light has been active
		Debug.Log("AI: Entering Standby Phase");
		duel_controller.ChangeTurnPhase (DuelSceneScript.TurnPhase.STANDBYPHASE);
		canSummon = true; //Resets the ability for the AI to summon
		goDefensive = false; //Resets the decision to go defensive
		//This idea of going defensive can change if the AI draws a card to beat
		//the player this turn
	}

	/// <summary>
	/// Executes all the logic done in the Main Phase 1 of the AIs turn
	/// Typically this is all logic to do with summoning a monster
	/// </summary>
	/// <returns>void</returns>
	protected IEnumerator MainPhase1 () {
		//Use this space for activiating Spell/Trap cards and Summoning monsters
		Debug.Log("AI: Entering Main Phase 1");
		duel_controller.ChangeTurnPhase (DuelSceneScript.TurnPhase.MAINPHASE1);
		//Setup variables used in checking
		DuelMonsterZoneScript emptyZone;
		Monster strongestInHand;
		Monster aStrongestField;
		Monster pStrongestField;
		Monster aDefenseInHand;
		//Monster aDefenseOnField;
		if (canSummon && !isMonsterZonesFull ()) {
			Debug.Log("AI: Can summon a monster");
			//Then we can summon a monster
			emptyZone = GetFirstEmptyMonsterZone ();
			if (duel_controller.turnCounter <= 1) { //If AI is playing first turn,
				//Then let us pick the strongest monster and place it in attack
				Monster monster = GetHighestAttackMonsterInHand ();
				if (monster != null && emptyZone != null) {
					yield return StartCoroutine (
						ISummonMonster (emptyZone, monster));
				}
			}
			else { //Otherwise lets check the Player field and make a decision
				strongestInHand = GetHighestAttackMonsterInHand ();
				aStrongestField = GetHighestAttackMonsterOnField ();

				DuelMonsterZoneScript tempFind = 
					duel_controller.GetStrongestMonsterOnPlayerField ();
				pStrongestField = (tempFind != null) ? 
					(Monster)tempFind.ZoneCard : null; 
				//
				if (pStrongestField == null) {
					//The players field is empty! We can summon our strongest
					//monster and go for a direct attack!
					if (strongestInHand != null && emptyZone != null) {
						//We have space on our field and got a monster from
						//our hand. We can now summon it!
						yield return StartCoroutine (
							ISummonMonster (emptyZone, strongestInHand));
					}
				}
				else { //The player has a monster on the field
					if (strongestInHand != null && aStrongestField != null) {
						//The AIs hand is not empty and the AI's field is not
						//empty
						if (strongestInHand.Attack >= pStrongestField.Attack ||
						    aStrongestField.Attack >= pStrongestField.Attack) {
							yield return StartCoroutine (
								ISummonMonster (emptyZone, strongestInHand));
						}
						else {
							goDefensive = true;
						}
					}
					else if (strongestInHand != null) {
						//The AIs hand is not empty but the field may be empty
						if (strongestInHand.Attack >= pStrongestField.Attack) {
							yield return StartCoroutine (
								ISummonMonster (emptyZone, strongestInHand));
						}
						else {
							goDefensive = true;
						}
					}
					else if (aStrongestField != null) {
						//The AI's field is not empty but the hand might be
						if (aStrongestField.Attack >= pStrongestField.Attack) {
							if (aStrongestField.Attack == pStrongestField.Attack
							    && ai_monsterzones.Count > 1) {
								//When the AI has a monster of equal attack
								//which is can 'throw away' in a monster for
								//monster battle but still have monsters to
								//defend or attack with
								if (strongestInHand != null) {
									yield return StartCoroutine (
										ISummonMonster (emptyZone, 
											strongestInHand));
								}
							}
						}
						else {
							//The AI has nothing strong enough, go defensive
							goDefensive = true;
						}
					}
				}
			}
		}
		else {
			//AI cannot summon or monster zones are full
			Debug.Log("AI: Cannot summon a monster");
			//Check field to see if the AI needs to go defensive as we cannot
			//summon this turn
			aStrongestField = GetHighestAttackMonsterOnField ();
			pStrongestField = 
				(Monster)duel_controller.
				GetStrongestMonsterOnPlayerField ().ZoneCard;

			if (pStrongestField != null) {
				if (aStrongestField != null) {
					if (aStrongestField.Attack < pStrongestField.Attack) {
						goDefensive = true;
					}
				}
				else {
					goDefensive = true;
				}
			}
		}

		if (goDefensive && canSummon && !isMonsterZonesFull ()) {
			//Check if we can place a Facedown defense position mosnter
			//instead
			Debug.Log("AI: going defensive");
			emptyZone = GetFirstEmptyMonsterZone ();
			aDefenseInHand = GetHighestDefenceMonsterInHand ();
			if (aDefenseInHand != null && emptyZone != null) {
				yield return StartCoroutine (ISetMonster (emptyZone, 
					aDefenseInHand));
			}
		}
	}

	/// <summary>
	/// Executes all logic for the AIs Battle Phase. Normally has to do with
	/// selecting which monster battles or if the AI can directly attack the
	/// player
	/// </summary>
	/// <returns>void</returns>
	protected IEnumerator BattlePhase() {
		//You cannot conduct a Battle Phase on your first turn, therefore
		Debug.Log("AI: Entering Battle Phase");
		duel_controller.ChangeTurnPhase (DuelSceneScript.TurnPhase.BATTLEPHASE);
		//Get a list of monsters that are able to attack first!
		//First check if the opponents field is empty
		List<DuelMonsterZoneScript> ml = GetMonstersAvailableToAttack();
		//If the list is empty, then no point in doing any more check!
		if (ml != null && ml.Count > 0) {
			//Avaialble for a direct attack?
			if (canDirectAttack ()) {
				Debug.Log ("AI: Is able to make a direct attack");
				foreach (DuelMonsterZoneScript monster in ml) {
					yield return StartCoroutine (IDirectAttack (monster));
				}
			}
			else {
				//Player has some monsters on the field
				//Scan field and pair AI monsters to attack/stats of Players
				Debug.Log ("AI: Cannot make a direct attack");
				if (!goDefensive) {
					ml = GetStrongestToWeakestMonsters (ml.ToArray());

					for (int i = 0; i < ml.Count; i++) {
						if (canDirectAttack ()) {
							//Direct attack
							yield return StartCoroutine (
								IDirectAttack (ml[i]));
						}
						else {
							//Get the strongest monster on the field, or next
							//strongest monster as the AI counts downwards
							//from it's strongest monster
							DuelMonsterZoneScript pMonster = duel_controller.
								GetStrongestMonsterOnPlayerField();
							//Compare and attack
							//Check first if the defending monster is in defence
							//mode or attack mode
							if (pMonster != null) {
								if (pMonster.isDefenceMode) {
									//Monster is in defence
									if (((Monster)ml [i].ZoneCard).Attack >
									   ((Monster)pMonster.ZoneCard).Defence) {
										//Attack the defensive monster!
										yield return StartCoroutine (
											IMakeAttack (ml [i], pMonster));
									}
								}
								else {
									if (((Monster)ml [i].ZoneCard).Attack >=
									   ((Monster)pMonster.ZoneCard).Attack) {
										//Attack the attacking monster,
										//this AI is going all out!
										yield return StartCoroutine (
											IMakeAttack (ml [i], pMonster));
									}
								}
							}
						}
					}
				
				}
				else {
					Debug.Log ("AI: Going defensive so not conducting battle");
				}
			}
		}
		else {
			Debug.Log ("AI: No monsters available to attack");
		}
	}

	/// <summary>
	/// Executes AI Main Phase 2 logic
	/// </summary>
	/// <returns>void</returns>
	protected IEnumerator MainPhase2() {
		//Use this space for setting trap cards
		Debug.Log("AI: Entering Main Phase 2");
		duel_controller.ChangeTurnPhase (DuelSceneScript.TurnPhase.MAINPHASE2);
		if (goDefensive) {
			//If we are going defensive then switch all available and possible
			//monsters into defense mode
			foreach (DuelMonsterZoneScript zone in ai_monsterzones) {
				if (zone.ZoneCard != null && !zone.defencePosition) {
					//Switch battle position of that monster zone
					yield return StartCoroutine (zone.IChangePosition ());
				}
			}
		}
	}

	/// <summary>
	/// Executes the AI's End Phase
	/// </summary>
	private void EndPhase() {
		//Use this for discarding a card if the hand size exceeds 7
		//Reducing any effect cards counters
		//Passing the turn over to the player
		Debug.Log("AI: Entering End Phase");
		duel_controller.ChangeTurnPhase (DuelSceneScript.TurnPhase.ENDPHASE);
		//Check to see if we need to discard a card
		//The rules say that no more than 7 cards can be in your hand at the
		//end of each End Phase
		if (ai_hand.Count > 7) {
			Debug.Log ("AI: Have to discard a card from hand");
			//TODO: randomly select the card to discard
			ai_graveyard.Add(ai_hand[0]);
			ai_hand.RemoveAt (0);
		}
		//Send an End Turn speech message to the UI
		duel_controller.ShowMessageFromAI (ai_details.ai_image, 
			ai_details.speech_EndTurn);
	}
	#endregion

	#region Message Command Methods
	/// <summary>
	/// Triggers the AI to start loading up and downloading the decklist
	/// ready for the player to play against
	/// </summary>
	/// <param name="duelController">Script that is in the duel scene</param>
	void CommandLoadAI(DuelSceneScript duelController) {
		//Load AI assigns the DuelScene script for controlling
		//The AI's interactivity with the UI and player objects
		this.duel_controller = duelController; //Assign controller script
		coroutine_loaddeck = true; //Used to control the starting of a coroutine
	}

	/// <summary>
	/// Command to tell the AI that it can start taking its turn
	/// </summary>
	void CommandTakeTurn() {
		ai_turnComplete = false;
		coroutine_taketurn = true;
	}

	/// <summary>
	/// Command to the AI to draw a card from the deck
	/// </summary>
	/// <param name="numberToDraw">Number to draw.</param>
	void CommandDrawCard(int numberToDraw) {
		DrawCard (numberToDraw);
	}

	/// <summary>
	/// Debug command sent to the AI to output the hand contents to the console
	/// </summary>
	void CommandRevealHand() {
		string handString = "";
		foreach (Card crd in ai_hand) {
			handString += crd.Name + "|";
		}
		Debug.Log ("AI Hand: " + handString);
		//Need to output it to the UI in the game and not just the debug log
	}

	/// <summary>
	/// Debug command sent to the AI to output the deck contents to the console
	/// </summary>
	void CommandRevealDeck() {
		string deckString = "";
		foreach (Card crd in ai_deck) {
			deckString += crd.Name + "|";
		}
		Debug.Log ("AI Deck: " + deckString);
		//Need to output it to the UI in the game and not just the debug log
	}
	#endregion
	/// <summary>
	/// Performs all the Phases in the correct order at a slowed down rate
	/// to make it easier for the player to understand what is going on.
	/// Performs all the logic needed for the AI to 'work'
	/// </summary>
	/// <returns>void</returns>
	protected IEnumerator IAITurn() {
		Debug.Log ("AI: Beginning Turn");
		//WaitForSeconds are for slowing down the transition between phases
		yield return new WaitForSeconds (2.0f);
		if (duel_controller.turnCounter <= 1) {
			//AI Welcome message that only displays when it's the very
			//start of the duel
			duel_controller.ShowMessageFromAI (ai_details.ai_image, 
				ai_details.speech_DuelStart);
		}
		DrawPhase ();
		yield return new WaitForSeconds (3.0f);
		StandbyPhase ();
		yield return new WaitForSeconds (3.0f);
		yield return StartCoroutine (MainPhase1 ());
		yield return new WaitForSeconds (3.0f);
		if (duel_controller.turnCounter > 1) {
			yield return StartCoroutine (BattlePhase ());
		}
		yield return new WaitForSeconds (3.0f);
		yield return StartCoroutine (MainPhase2 ());
		yield return new WaitForSeconds (3.0f);
		EndPhase ();
		yield return new WaitForSeconds (3.0f);
		duel_controller.EndAITurn ();
	}

	/// <summary>
	/// Logic for the AI to summon a monster
	/// </summary>
	/// <returns>void</returns>
	/// <param name="emptyZone">Empty monster zone to summon the card too</param>
	/// <param name="cardToSummon">Card to summon to the empty zone</param>
	protected IEnumerator ISummonMonster(DuelMonsterZoneScript emptyZone, 
		Monster cardToSummon) {
		Debug.Log ("AI: Summoning " + cardToSummon.Name);
		//Show summoning message
		duel_controller.ShowMessageFromAI (ai_details.ai_image, 
			ai_details.speech_SummonCard + cardToSummon.Name);
		//Start the summoning sequence and animation in the DuelMonsterZoneScript
		emptyZone.PlaceCardOnZone (cardToSummon, false);
		//Since the card has been played onto the field, we can
		//now remove it from the AIs hand
		ai_hand.Remove(cardToSummon);
		yield return new WaitForSeconds (1.3f);
	}

	/// <summary>
	/// Logic for the AI setting a monster
	/// </summary>
	/// <returns>void</returns>
	/// <param name="emptyZone">Empty monster zone to set the card too</param>
	/// <param name="cardToSet">Card to set</param>
	protected IEnumerator ISetMonster(DuelMonsterZoneScript emptyZone,
		Monster cardToSet) {
		Debug.Log ("AI: Setting " + cardToSet.Name);
		duel_controller.ShowMessageFromAI (ai_details.ai_image, 
			ai_details.speech_SetCard); //Display message
		emptyZone.PlaceCardOnZone (cardToSet, true);
		ai_hand.Remove (cardToSet); //Remove card from the AI's hand
		yield return new WaitForSeconds (1.3f);
	}

	/// <summary>
	/// Logic for making a direct attack
	/// </summary>
	/// <returns>void</returns>
	/// <param name="cardToAttack">Card zone attacking with</param>
	protected IEnumerator IDirectAttack(DuelMonsterZoneScript cardToAttack) {
		duel_controller.ShowMessageFromAI (ai_details.ai_image, 
			ai_details.speech_DirectAttack + cardToAttack.ZoneCard.Name);
		//Output message
		yield return new WaitForSeconds (1.3f);
		yield return StartCoroutine(duel_controller.
			IMakeAttack(cardToAttack, true)); //Start the animation/sequence
		yield return new WaitForSeconds (0.7f);
		//Reduce the life points of either the Player or AI depending on who
		//took what damage
		yield return StartCoroutine (duel_controller.IReduceLifePoints ());
	}

	/// <summary>
	/// Logic for the AI attackin a monster with one of its own
	/// </summary>
	/// <returns>void</returns>
	/// <param name="attackingMonster">Attacking monster.</param>
	/// <param name="defendingMonster">Defending monster.</param>
	protected IEnumerator IMakeAttack(DuelMonsterZoneScript attackingMonster,
		DuelMonsterZoneScript defendingMonster) {
		duel_controller.ShowMessageFromAI (ai_details.ai_image, 
			ai_details.speech_NormalAttack + attackingMonster.ZoneCard.Name);
		//Output message and wait
		yield return new WaitForSeconds (1.3f);
		yield return StartCoroutine(duel_controller.
			IMakeAttack(attackingMonster, defendingMonster, true)); //Start
		//the attacking sequence/animation
		yield return new WaitForSeconds (0.7f); //Wait
		//Reduce the Life Points of either the Player or AI depending on who
		//took the damage
		yield return StartCoroutine (duel_controller.IReduceLifePoints ());
	}

	/// <summary>
	/// Logic for downloading each card in the AIDetails deck list, creating
	/// the card object and assigning it to the correct list. Such as Deck,
	/// or Extra Deck depending on the card type
	/// </summary>
	/// <returns>void</returns>
	protected IEnumerator ILoadDeck() {
		if (ai_details != null) { //Cannot do anything if the deck list is
			//not available
			foreach (string cardCode in ai_details.ai_decklist) {
				DownloadManager dlManager = new DownloadManager (this,
					                            cardCode, false);
				yield return dlManager.Coroutine;
				if (dlManager.Result is Monster) {
					//Monsters go into the ai deck unless they are a Fusion,
					//XYZ, Synchro, or Synchro-Tuner type monster
					Monster result = (Monster)dlManager.Result;
					if (result.MonsterTypeA == "Fusion" ||
					    result.MonsterTypeA == "XYZ" ||
					    result.MonsterTypeA == "Synchro" ||
					    result.MonsterTypeA == "Synchro-Tuner") {
						//Add the extra deck monster to the extra deck
						ai_extradeck.Add ((Monster)dlManager.Result);
					}
					else {
						//Otherwise it belongs in the main deck
						ai_deck.Add ((Monster)dlManager.Result);
					}
				}
				else if (dlManager.Result is Card) {
					//Let's just assume that it's a Spell or Trap card
					//These types of cards go straight into the deck
					ai_deck.Add((Card)dlManager.Result);
				}
				else {
					//The card returned an error, do something
					Debug.LogError("AI: Failed to get card. Error Code: " + 
						(string)dlManager.Result);
				}
			}
			ai_deck = ShuffleDeck (ai_deck);
			//Once the deck is loaded, we assume it is being loaded because
			//the AI is starting up. (There is no other reason why)
			//So we can draw the AI's initial hand within this function
			DrawCard(5);
			yield return new WaitForSeconds (3.0f);
			deckLoaded = true;
		}
	}
}