/*
 * Author: Stuart Harrison 
 * Date: February 2016
 * Class for holding the Duel Options when in the duel scene. Controls how the
 * AI can behave and what Life points it should assign at the start. It is also
 * used to store the outcome of the duel!
*/

using UnityEngine;
using System.Collections;

public class DuelOptions : MonoBehaviour {

	//AI behaviour modification here! Traditional rules means AI does not
	//have to think about sacrificing monsters to play stronger ones!

	public bool duelopt_traditionalrules;
	public bool duelopt_monstersonly;
	public bool duelopt_noeffectmonsters;

	public bool hasDrawnGame = false;
	public bool hasPlayerLost = false;
}