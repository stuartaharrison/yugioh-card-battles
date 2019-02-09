/*
 * Author: Stuart Harrison 
 * Date: March 2016
 * Class for control a single monster zone on the either the AI or Player's
 * side of the field.
*/

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DuelSceneScript : MonoBehaviour {

	//Used to determine the current phase
	public enum TurnPhase
	{
		DRAWPHASE,
		STANDBYPHASE,
		MAINPHASE1,
		BATTLEPHASE,
		MAINPHASE2,
		ENDPHASE
	};

	public AI duel_ai;
	public bool hasPlayerSummoned = false;
	public bool isPlayerTurn = true;
	public bool canPlayerSummon = false;
	public bool canPlayerAttack = false;
	public bool canPlayerDirectAttack = false;
	public DuelMonsterZoneScript[] ui_ai_monsterZones;
	public DuelMonsterZoneScript[] ui_playerMonsterZones;
	public GameObject ui_duelloader;
	public GameObject ui_duelmenu;
	public GameObject ui_ai_messageBox;
	public GameObject ui_yourturnOverlay;
	public GameObject ui_attackingOverlay;
	public Image ui_attackersPicture;
	public Image ui_defendersPicture;
	public Image ui_img_PlayerPicture;
	public Image ui_img_AIPicture;
	public int turnCounter = 1;
	public int player_lifePoints;
	public List<Card> player_Graveyard;
	public Text ui_lbl_playerName;
	public Text ui_lbl_aiName;
	public Text ui_lbl_playerLifePoints;
	public Text ui_lbl_aiLifePoints;
	public Text ui_lbl_turnCounter;
	public Text ui_lbl_phaseDisplay;
	public Text ui_attackersDamage;
	public Text ui_defendersDamage;

	private bool isReducingLifePoints = false;
	private bool hasPlayerLost = false;
	private DuelOptions duel_options;
	private GameObject duel_opponent;
	private GameObject glob_gamemanager;
	private GameObject glob_audiomanager;
	private int player_liveLifePoints;
	private TurnPhase currentTurnPhase;

	public TurnPhase CurrentTurnPhase { get { return CurrentTurnPhase; }
										set { currentTurnPhase = value; } }

	void Start () {
		//Get the game management objects
		glob_gamemanager = GameObject.FindGameObjectWithTag ("GameManager");
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");
		//Get the duel opponent object
		duel_opponent = GameObject.FindGameObjectWithTag ("DUEL_OPPONENT");
		//Get the AI and then Duel Options
		duel_ai = duel_opponent.GetComponent<AI> ();
		duel_options = duel_opponent.GetComponent<DuelOptions> ();

		//Setup the game based on the duel options the player has selected
		if (duel_options.duelopt_traditionalrules) {
			duel_ai.ai_LifePoints = 4000;
			duel_ai.ai_liveLifePoints = 4000;
			player_lifePoints = 4000;
			player_liveLifePoints = 4000;
		}

		//Setup player objects
		player_Graveyard = new List<Card> ();
		//Setup the Player and AI profile pictures as they do not change
		//during the duel
		ui_img_PlayerPicture.sprite = glob_gamemanager.
			GetComponent<AccountController>().LoggedInAccount.DisplayPicture;
		ui_img_AIPicture.sprite = duel_opponent.
			GetComponent<AIDetails> ().ai_image;
		//Setup the Player and AI names as these also do not change
		ui_lbl_playerName.text = glob_gamemanager.
			GetComponent<AccountController>().LoggedInAccount.Username;
		ui_lbl_aiName.text = duel_opponent.
			GetComponent<AIDetails> ().ai_name;

		//Get the AI monster zones
		foreach (DuelMonsterZoneScript script in ui_ai_monsterZones) {
			duel_ai.ai_monsterzones.Add (script);
		}
		duel_opponent.SendMessage ("CommandLoadAI", this); //Tell the AI
		//to start loading up!
	}

	void Update () {
		//First check if either the AI or Player has lost the Duel
		if (hasPlayerLost || duel_ai.hasAILost) {
			duel_options.hasPlayerLost = hasPlayerLost;
			//If this is false then lets assume the AI lost
			//Move to a new Scene!
			SceneManager.LoadScene("duelendscene");
		}

		if (duel_ai.deckLoaded && ui_duelloader.activeInHierarchy) {
			//Once the deck is loaded we can shift AI menus
			ui_duelloader.SetActive (false);
			ui_duelmenu.SetActive (true);
			//once the deck is loaded and the UI's shift, we can play the duel
			//audio
			glob_audiomanager.SendMessage ("StopMainMenuAudio");
			glob_audiomanager.SendMessage ("PlayDuelAudio");
		}

		if (ui_duelmenu.activeInHierarchy) {
			if (isReducingLifePoints) {
				isReducingLifePoints = false;
				StartCoroutine (IReduceLifePoints ());
			}
			//Display the Life Points (these change)
			ui_lbl_playerLifePoints.text = player_lifePoints.ToString();
			ui_lbl_aiLifePoints.text = duel_ai.ai_LifePoints.ToString();
			//The turn counter
			ui_lbl_turnCounter.text = "Turn: " + turnCounter.ToString();
		}
	}

	/// <summary>
	/// Checks if the player can make an attack
	/// </summary>
	/// <returns><c>true</c>, if can player can attack, 
	/// <c>false</c> otherwise.</returns>
	private bool checkCanPlayerAttack() {
		bool flag = false;
		//To make an attack on a monster then the player has to 
		//have a monster on the field and so does the AI
		//TODO: Make checks for any effects that would prevent an attack
		//from occuring this turn
		//First of all, if it's the very first turn then you cannot attack
		if (turnCounter > 1) {
			foreach (DuelMonsterZoneScript pDz in ui_playerMonsterZones) {
				if (pDz.ZoneCard != null && !pDz.isFaceDown) {
					//Facedown monsters cannot make an attack
					flag = true;
				}
			}
			if (flag) { //We found some monsters on the players side of the field
				//Loop to check if the opponent has some
				foreach (DuelMonsterZoneScript aDz in ui_ai_monsterZones) {
					if (aDz != null) { //Doesn't matter if the AI has a facedown
						flag = true;
					}
				}
			}
		}
		return flag;
	}

	/// <summary>
	/// Checks if the player can make a direct attack agaist the AI
	/// </summary>
	/// <returns><c>true</c>, if can player can make direct attack, 
	/// <c>false</c> otherwise.</returns>
	private bool checkCanPlayerDirectAttack() {
		bool flag = false;
		//To make a direct attack then the player has to have at least 1 face-up
		//monster and the AI must have no monsters on the field
		//Although we cannot attack if it's the very first turn
		if (turnCounter > 1) {
			foreach (DuelMonsterZoneScript pDz in ui_playerMonsterZones) {
				if (pDz.ZoneCard != null && !pDz.isFaceDown) {
					//Facedown monsters cannot make an attack
					flag = true;
				}
			}
			if (flag) {
				foreach (DuelMonsterZoneScript aDz in ui_ai_monsterZones) {
					if (aDz.ZoneCard != null) {
						flag = false;
					}
				}
			}
		}
		return flag;
	}

	/// <summary>
	/// Checks if the player is currently able to summon a monster
	/// </summary>
	/// <returns><c>true</c>, if can player can summon, 
	/// <c>false</c> otherwise.</returns>
	private bool checkCanPlayerSummon() {
		return true;
		//TODO: Make checks for anything that would prevent a player from
		//summoning
	}

	/// <summary>
	/// Gets the strongest monster on player field.
	/// </summary>
	/// <returns>The strongest monster on player field.</returns>
	public DuelMonsterZoneScript GetStrongestMonsterOnPlayerField() {
		DuelMonsterZoneScript returnCard = null;
		int flaggedStrongest = 0;
		foreach (DuelMonsterZoneScript mon in ui_playerMonsterZones) {
			//foreach monster card in the monster card zone
			if (mon.ZoneCard != null) { //If it's not empty
				DuelMonsterZoneScript temp = mon;
				//Compare the monsters attack with the previously found attack
				if (((Monster)mon.ZoneCard).Attack > flaggedStrongest) {
					//Assign it
					flaggedStrongest = ((Monster)mon.ZoneCard).Attack;
					returnCard = temp;
				}
			}
		}
		return returnCard; //Return
	}

	/// <summary>
	/// Gets the list of monsters on player field.
	/// </summary>
	/// <returns>The list of monsters on player field.</returns>
	public List<DuelMonsterZoneScript> GetListOfMonstersOnPlayerField() {
		List<DuelMonsterZoneScript> ml = new List<DuelMonsterZoneScript> ();
		//Foreach monster card zone on the player field
		foreach (DuelMonsterZoneScript zScript in ui_playerMonsterZones) {
			if (zScript.ZoneCard != null) { //If the is not null
				ml.Add (zScript); //Then add it to the list
			}
		}
		return ml; //Return the list
	}

	/// <summary>
	/// Shows a message on the UI from the AI
	/// </summary>
	/// <param name="aiImage">Ai sprite/picture</param>
	/// <param name="message">Message to display</param>
	public void ShowMessageFromAI(Sprite aiImage, string message) {
		ui_ai_messageBox.GetComponent<DuelAISpeechBoScript> ().
			DisplayMessageBox (aiImage, message);
		ui_ai_messageBox.SetActive (true); //Display
	}

	/// <summary>
	/// Sets player lifepoints to 0 and starts reducing the players life points
	/// </summary>
	public void PlayerSurrender() {
		player_liveLifePoints = 0;
		isReducingLifePoints = true;
	}

	/// <summary>
	/// Sets AI lifepoints to 9 and starts reducing the life points
	/// </summary>
	public void AISurrender() {
		duel_ai.ai_liveLifePoints = 0;
		isReducingLifePoints = true;
	}

	/// <summary>
	/// Changes the current turn to the next one in the order
	/// </summary>
	public void NextTurnPhase() {
		switch (currentTurnPhase) {
		case TurnPhase.DRAWPHASE:
			currentTurnPhase = TurnPhase.STANDBYPHASE;
			break;
		case TurnPhase.STANDBYPHASE:
			currentTurnPhase = TurnPhase.MAINPHASE1;
			canPlayerSummon = checkCanPlayerSummon ();
			break;
		case TurnPhase.MAINPHASE1:
			if (turnCounter > 1) { //First turns cannot enter their battlephase!
				currentTurnPhase = TurnPhase.BATTLEPHASE;
				canPlayerAttack = checkCanPlayerAttack ();
				canPlayerDirectAttack = checkCanPlayerDirectAttack ();
			}
			else {
				currentTurnPhase = TurnPhase.MAINPHASE2;
				canPlayerAttack = false;
				canPlayerDirectAttack = false;
			}
			break;
		case TurnPhase.BATTLEPHASE:
			currentTurnPhase = TurnPhase.MAINPHASE2;
			canPlayerSummon = checkCanPlayerSummon ();
			break;
		case TurnPhase.MAINPHASE2:
			currentTurnPhase = TurnPhase.ENDPHASE;
			break;
		case TurnPhase.ENDPHASE:
			EndPlayerTurn ();
			break;
		}
		ChangeTurnPhaseText ();
	}

	/// <summary>
	/// Command called by the player pressing a button on the UI. Moves the
	/// current turn phase to the next phase
	/// </summary>
	public void ChangeTurnPhase(TurnPhase phase) {
		currentTurnPhase = phase;
		ChangeTurnPhaseText ();
	}

	/// <summary>
	/// Changes the turn phase text.
	/// </summary>
	private void ChangeTurnPhaseText() {
		switch (currentTurnPhase) {
		case TurnPhase.DRAWPHASE:
			ui_lbl_phaseDisplay.text = "Draw Phase";
			break;
		case TurnPhase.STANDBYPHASE:
			ui_lbl_phaseDisplay.text = "Standby Phase";
			break;
		case TurnPhase.MAINPHASE1:
			ui_lbl_phaseDisplay.text = "Main Phase 1";
			break;
		case TurnPhase.BATTLEPHASE:
			ui_lbl_phaseDisplay.text = "Battle Phase";
			break;
		case TurnPhase.MAINPHASE2:
			ui_lbl_phaseDisplay.text = "Main Phase 2";
			break;
		case TurnPhase.ENDPHASE:
			ui_lbl_phaseDisplay.text = "End Phase";
			break;
		}
	}

	/// <summary>
	/// Sets the AI to being the first turn and sends the command to the AI
	/// to start taking its turn
	/// </summary>
	public void AITurnFirst() {
		isPlayerTurn = false;
		duel_opponent.SendMessage ("CommandTakeTurn");
	}

	/// <summary>
	/// Sets the players turn to being the first turn and begins to let the
	/// player take command of the game
	/// </summary>
	public void PlayerTurnFirst() {
		isPlayerTurn = true;
		ChangeTurnPhase (TurnPhase.DRAWPHASE);
	}

	/// <summary>
	/// Ends the players turn. Resets all the player turn variables and then
	/// sends a command to the AI to take its turn
	/// </summary>
	public void EndPlayerTurn() {
		glob_audiomanager.SendMessage ("PlayDuelEndTurn");
		this.turnCounter++; //increase the turn counter
		foreach (DuelMonsterZoneScript dMz in ui_playerMonsterZones) {
			if (dMz.ZoneCard != null) {
				//increment the number of turns each monster has been on
				//the field
				dMz.turnsOnField++;
				//Reset the stats
				dMz.hasChangedPosition = false;
				dMz.hasAttacked = false;
			}
		}
		canPlayerSummon = false;
		canPlayerAttack = false;
		canPlayerDirectAttack = false;
		isPlayerTurn = false;
		duel_opponent.SendMessage ("CommandTakeTurn");
	}

	/// <summary>
	/// Called when the AI ends its turn. Sets it to the players turn and
	/// notifies them that they can start performing functions
	/// </summary>
	public void EndAITurn() {
		glob_audiomanager.SendMessage ("PlayDuelEndTurn");
		this.turnCounter++;
		foreach (DuelMonsterZoneScript dMz in ui_ai_monsterZones) {
			if (dMz.ZoneCard != null) {
				//increment the number of turns each monster has been on
				//the field
				dMz.turnsOnField++;
			}
		}
		isPlayerTurn = true; //Set players turn
		hasPlayerSummoned = false; //Reset some variables
		ChangeTurnPhase (TurnPhase.DRAWPHASE); //Change turn phase
		ui_yourturnOverlay.SetActive (true); //Display message to user
	}

	#region LifePoint Enumerator's
	/// <summary>
	/// Coroutine for reducing the life point score of either the AI or the 
	/// player
	/// </summary>
	/// <returns>void</returns>
	public IEnumerator IReduceLifePoints() {
		//Play opening the lp counter
		//Start countdown loop music
		//reduce lifepoints loop
		//play stop of zero sound effect depending on LP's remaining
		glob_audiomanager.SendMessage ("PlayLPCounterAppears");
		yield return new WaitForSeconds (0.5f);
		glob_audiomanager.SendMessage ("PlayLPCounterDownLoop");
		while (player_lifePoints != player_liveLifePoints) {
			player_lifePoints -= 50;
			ui_lbl_playerLifePoints.text = player_lifePoints.ToString();
			yield return new WaitForSeconds (0.1f);
		}
		while (duel_ai.ai_LifePoints != duel_ai.ai_liveLifePoints) {
			duel_ai.ai_LifePoints -= 50;
			ui_lbl_aiLifePoints.text = duel_ai.ai_LifePoints.ToString();
			yield return new WaitForSeconds (0.1f);
		}
		//Bit of Error Checking Here in the rare chance we go under the new life
		//points score of the player/AI
		if (player_lifePoints < player_liveLifePoints) { 
			player_lifePoints = player_liveLifePoints; 
		}
		if (duel_ai.ai_LifePoints < duel_ai.ai_liveLifePoints) {
			duel_ai.ai_LifePoints = duel_ai.ai_liveLifePoints;
		}
		//Once score has adjusted, stop the looped counter noise
		glob_audiomanager.SendMessage ("StopLPCounterDownLoop");
		//Check if Player OR the AI has Lost the Duel
		if (player_lifePoints <= 0 || duel_ai.ai_LifePoints <= 0) {
			glob_audiomanager.SendMessage ("PlayLPCounterZero");
			yield return new WaitForSeconds (0.5f);
			if (player_lifePoints <= 0) {
				Debug.Log ("Player has lost the duel");
				hasPlayerLost = true;
			}
			else {
				Debug.Log ("AI has lost the duel");
				duel_ai.hasAILost = true;
			}
		}
		else {
			glob_audiomanager.SendMessage ("PlayLPCounterStops");
			yield return new WaitForSeconds (0.5f);
		}
	}

	/// <summary>
	/// Coroutine to begin a battle between an attacking monster and a direct
	/// attack against the opponent
	/// </summary>
	/// <returns>void</returns>
	/// <param name="attackingMonster">Attacking monster.</param>
	/// <param name="againstPlayer">If set to <c>true</c> against player.</param>
	public IEnumerator IPlayerBattle(DuelMonsterZoneScript attackingMonster, 
		bool againstPlayer) {
		yield return StartCoroutine (IMakeAttack (attackingMonster, false));
		yield return new WaitForSeconds (1.0f);
		yield return StartCoroutine (IReduceLifePoints ());
	}

	/// <summary>
	/// Coroutine to begin a battle between an attacking monster and a defending
	/// monster
	/// </summary>
	/// <returns>void</returns>
	/// <param name="attackingMonster">Attacking monster.</param>
	/// <param name="defendingMonster">Defending monster.</param>
	/// <param name="againstPlayer">If set to <c>true</c> against player.</param>
	public IEnumerator IPlayerBattle(DuelMonsterZoneScript attackingMonster, 
		DuelMonsterZoneScript defendingMonster, bool againstPlayer) {
		yield return StartCoroutine (IMakeAttack (attackingMonster, 
			defendingMonster, false));
		yield return new WaitForSeconds (1.0f);
		yield return StartCoroutine (IReduceLifePoints ());
	}

	/// <summary>
	/// Coroutine to conduct the animation and logic for a direct attack battle
	/// between the attacking monster and the defending monsters life points
	/// </summary>
	/// <returns>void</returns>
	/// <param name="attackingMonster">Attacking monster.</param>
	/// <param name="againstPlayer">If set to <c>true</c> against player.</param>
	public IEnumerator IMakeAttack(DuelMonsterZoneScript attackingMonster, 
		bool againstPlayer) {
		yield return new WaitForSeconds (1.0f);
		//This is when a direct attack has been made
		int attackersScore = ((Monster)attackingMonster.ZoneCard).Attack;
		int defendersScore = 0;
		int damage = defendersScore - attackersScore;
		ui_attackersPicture.sprite = ((Monster)attackingMonster.ZoneCard).Image;
		ui_defendersPicture.sprite = null;
		ui_defendersPicture.color = Color.black;
		ui_attackersDamage.text = "";
		ui_defendersDamage.text = damage.ToString ();

		ui_attackingOverlay.SetActive (true);

		glob_audiomanager.SendMessage ("PlayDuelDirectHit");
		yield return new WaitForSeconds (2.1f);
		ui_attackingOverlay.SetActive (false);

		if (againstPlayer) {
			//This means the AI is attacking the player
			player_liveLifePoints -= attackersScore;
		}
		else {
			//The player is attacking the AI directly
			duel_ai.ai_liveLifePoints -= attackersScore;
		}
	}

	/// <summary>
	/// Coroutine to conduct the animation and logic for a battle involving
	/// an attacking monster and the defending monster.
	/// </summary>
	/// <returns>void</returns>
	/// <param name="attackingMonster">Attacking monster.</param>
	/// <param name="defendingMonster">Defending monster.</param>
	/// <param name="againstPlayer">If set to <c>true</c> against player.</param>
	public IEnumerator IMakeAttack(DuelMonsterZoneScript attackingMonster, 
		DuelMonsterZoneScript defendingMonster, bool againstPlayer) {
		yield return new WaitForSeconds (1.0f);
		int attackersScore = ((Monster)attackingMonster.ZoneCard).Attack;
		int defendersScore = 0;
		//Decided whether we are using the defenders Defence or Attack score
		if (defendingMonster.isDefenceMode) {
			//Use defence mode score
			defendersScore = ((Monster)defendingMonster.ZoneCard).Defence;
		}
		else {
			//Use attack mode score
			defendersScore = ((Monster)defendingMonster.ZoneCard).Attack;
		}
		//Workout the damage
		int damage = defendersScore - attackersScore;
		int lifePointsToReduceBy = 0;
		//Assign monster images
		ui_attackersPicture.sprite = ((Monster)attackingMonster.ZoneCard).Image;
		ui_defendersPicture.color = Color.white; //Reset incase a direct
		//attack as happened as the black with hide the picutre we set
		ui_defendersPicture.sprite = ((Monster)defendingMonster.ZoneCard).Image;
		//Check what damage is done to what card
		if (damage <= 0) {
			ui_attackersDamage.text = "";
			ui_defendersDamage.text = damage.ToString ();
		}
		else {
			ui_attackersDamage.text = (-damage).ToString();
			ui_defendersDamage.text = "";
		}
		//Display the Attackers overlay
		ui_attackingOverlay.SetActive (true);
		glob_audiomanager.SendMessage ("PlayDuelDirectHit");
		yield return new WaitForSeconds (2.1f);
		ui_attackingOverlay.SetActive (false);
		//Work out, which monster(s) gets destroyed and the life points to
		//reduce
		if (damage < 0) {
			//Defending monster is destroyed
			if (!defendingMonster.isDefenceMode) {
				lifePointsToReduceBy = -damage;
			}
			yield return StartCoroutine (
				defendingMonster.IDestroyCard (againstPlayer));
		}
		else if (damage == 0 && !defendingMonster.isDefenceMode) {
			//When the battle is a tie and the monster is in defence, then
			//no monsters are destroyed, otherwise...
			yield return StartCoroutine (
				defendingMonster.IDestroyCard (againstPlayer));
			yield return StartCoroutine (
				attackingMonster.IDestroyCard (!againstPlayer));
			//Damage would be 0 because Attack was equal
			//Therefore we do not touch the lifepoints to reduce variable
		}
		else {
			//The attacking monster is weaker than the opponents defending,
			//so destroy the attacking monster
			yield return StartCoroutine (
				attackingMonster.IDestroyCard (againstPlayer));
			lifePointsToReduceBy = damage;
			//Reverse the damage because the attacker is taking the damage
			againstPlayer = !againstPlayer;
		}
		//Do damage to the appropriate player
		if (againstPlayer) {
			player_liveLifePoints -= lifePointsToReduceBy;
		}
		else {
			duel_ai.ai_liveLifePoints -= lifePointsToReduceBy;
		}
		canPlayerAttack = checkCanPlayerAttack ();
		canPlayerDirectAttack = checkCanPlayerDirectAttack ();
		yield return new WaitForSeconds (0.5f);
	}
	#endregion
}