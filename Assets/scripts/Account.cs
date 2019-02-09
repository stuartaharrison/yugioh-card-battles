/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Class for storing and handling the players account information
*/

using UnityEngine;
using System.Collections;

public class Account {

	//Private variables
	private int id;
	private int level;
	private int exp;
	private int wins;
	private int loss;
	private int draw;
	private int privilages;
	private Sprite displayPicture;
	private string name;

	//Getter methods
	public int ID { get { return id; } }
	public int Level { get { return level; } }
	public int EXP { get { return exp; } }
	public int Wins { get { return wins; } }
	public int Loss { get { return loss; } }
	public int Draw { get { return draw; } }
	public int Privilages { get { return privilages; } }
	public Sprite DisplayPicture { get { return displayPicture; } }
	public string Username { get { return name; } }

	/// <summary>
	/// Initializes a new instance of the Account class.
	/// </summary>
	/// <param name="id">Identifier.</param>
	/// <param name="name">Name.</param>
	/// <param name="level">Level.</param>
	/// <param name="experience">Experience.</param>
	/// <param name="wins">Wins.</param>
	/// <param name="loss">Loss.</param>
	/// <param name="draw">Draw.</param>
	/// <param name="dp">Accoutn display picture</param>
	/// <param name="privilages">Admin privilages</param>
	public Account(int id, string name, int level, int experience, int wins,
		int loss, int draw, Texture2D dp, int privilages) {

		this.id = id;
		this.level = level;
		this.exp = experience;
		this.wins = wins;
		this.loss = loss;
		this.draw = draw;
		this.privilages = privilages;
	
		this.name = name;

		//Unity UI displays sprites but the WWW class can only get Texture2D's
		//back. Therefore we need to do a quick convert.
		Rect spriteRect = new Rect (0, 0, dp.width, dp.height);
		this.displayPicture = Sprite.Create (dp, spriteRect, 
			new Vector2 (0.5f, 0.5f));
	}

	/// <summary>
	/// Creates a string of data from the account object ready to send up to
	/// the web server to update the account data in the database.
	/// </summary>
	/// <returns>String containing the data we want to update</returns>
	public string ToUpdateString() {
		return this.id + "|" + this.level + "|" + this.exp + "|" + this.wins +
		"|" + this.loss + "|" + this.draw;
	}

	/// <summary>
	/// Add a set amount of experience to the accounts experience. Check if we
	/// are beyond the limit to 'level up' to the next level.
	/// </summary>
	/// <returns><c>true</c>, if the exp that was added goes beyond what is 
	/// needed to level, <c>false</c> If the amount of exp added does not go
	/// beyond what is needed to level.</returns>
	/// <param name="expToAdd">EXP Gain</param>
	public bool AddExp(int expToAdd) {
		bool hasLeveledUp = false;
		int expToNext = ExpToNextLevel ();
		this.exp += expToAdd;
		if (this.exp >= expToNext) {
			this.level++;
			hasLeveledUp = true;
			this.exp = 0;
		}
		return hasLeveledUp;
	}

	/// <summary>
	/// Adds 1 to the number of wins the account has. This prevents extra wins
	/// and direct access to the variable within the object.
	/// </summary>
	public void AddWin() {
		this.wins++;
	}

	/// <summary>
	/// Adds 1 to the number of losses the account has. This prevents extra wins
	/// and direct access to the variable within the object.
	/// </summary>
	public void AddLoss() {
		this.loss++;
	}

	/// <summary>
	/// Adds 1 to the number of draws the account has. This prevents extra wins
	/// and direct access to the variable within the object.
	/// </summary>
	public void AddDraw() {
		this.draw++;
	}

	/// <summary>
	/// Gets the amount of EXP the player has to gather in order to advance to
	/// the next level.
	/// </summary>
	/// <returns>The total exp required to reach the players next level</returns>
	private int ExpToNextLevel() {
		return this.level * 100; //Temp calculations
	}
}