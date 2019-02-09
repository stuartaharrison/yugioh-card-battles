/*
 * Author: Stuart Harrison 
 * Date: April 2016
 * Script for controlling the Players attack target selection and which card
 * he/she will be attacking with.
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DuelAttackOverlayScript : MonoBehaviour {

	public GameObject duelController; //Duel Controller object
	public GameObject[] aiZones; //List of all the AI zones attack buttons
	//that has been set in the Unity Editor

	private bool directAttack; //Determines whether its a direct attack or not
	private DuelSceneScript duelControllerScript; //Gotten from the duelcontroller
	private DuelMonsterZoneScript attackingMonster;
	private DuelMonsterZoneScript defendingMonster;

	void Start () {
		duelControllerScript = duelController.GetComponent<DuelSceneScript> ();
	}

	//Called one per fram
	void Update () {
		//If this whole gameobject is active on the scene then we can do this:
		if (gameObject.activeInHierarchy ) {
			//Loop around all the ai monster zones
			for (int i = 0;
				i < duelControllerScript.ui_ai_monsterZones.Length; i++) {
				if (duelControllerScript.ui_ai_monsterZones [i].
					ZoneCard != null) { //If the zone HAS a monster then,
					//Display the the appropriate UI button that overlays
					//that monster zone
					aiZones [i].SetActive (true);
				}
				else {
					//Otherwise do not display it
					aiZones [i].SetActive (false);
				}
			}
		}
		else {
			//If its not active, then we can just disable all the buttons
			foreach (GameObject obj in aiZones) {
				obj.SetActive (false);
			}
		}
	}

	/// <summary>
	/// Called when the Player selects a monster on his/her side of the field
	/// and selects the attack option. Brings up this overlay
	/// </summary>
	/// <param name="aMonster">Selected monster</param>
	public void StartTargetSelection(DuelMonsterZoneScript aMonster) {
		attackingMonster = aMonster; //Assign it to the attacking monster pos
	}

	/// <summary>
	/// Button to cancel the attack and close the overlay. Will also reset any
	/// variables that have been effected during the attack process
	/// </summary>
	public void btn_CloseOverlay() {
		attackingMonster.hasAttacked = false; //Fixed a bug here!
		attackingMonster = null;
		defendingMonster = null;
		gameObject.SetActive (false);
	}

	/// <summary>
	/// Called when the user presses one of the UI buttons over the AI monster
	/// zone. Gets the defending monsters details depending on which button it
	/// was that was pressed.
	/// </summary>
	/// <param name="positionOnField">Position of targeted monster</param>
	public void btn_Selected(int positionOnField) {
		//Get the defending monster details
		defendingMonster = 
			duelControllerScript.ui_ai_monsterZones [positionOnField];
		//Start the battle sequence/animation
		duelControllerScript.StartCoroutine (
			duelControllerScript.IPlayerBattle(
				attackingMonster,
				defendingMonster, false));
		//Close down this overlay
		attackingMonster = null;
		defendingMonster = null;
		gameObject.SetActive (false);
	}
}