/*
 * Author: Stuart Harrison 
 * Date: February 2016
 * Class from which the AIs base details are stored. Such as the words that
 * they say during the duel, how much EXP defeating them gives, and what cards
 * are in their deck. Also has a value that tells the game what level the player
 * needs to be in order for this AI to be playable against
*/
using UnityEngine;
using System.Collections;

public class AIDetails : MonoBehaviour {
	//These are all assignable from the Unity Editor when creating the AI object

	//Base details
	public int ai_unlocklevel;
	public int ai_winExp;
	public int ai_lossExp;
	public int ai_drawExp;
	public Sprite ai_image;
	public string ai_name;
	public string ai_deckname;
	public string[] ai_decklist;

	//Speech variables
	public string speech_DuelStart = "Get your Game On! Let's Duel..";
	public string speech_EndTurn = "That will do for now!";
	public string speech_DrawCard = "I draw!";
	public string speech_SummonCard = "I summon ";
	public string speech_SetCard = "I am going to throw down a face-down!";
	public string speech_DirectAttack = "I attack you directly with my ";
	public string speech_NormalAttack = "I attack your monster with my ";
}
