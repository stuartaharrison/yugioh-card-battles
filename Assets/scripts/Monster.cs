/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Class for storing and handling monster card information.
*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable()]
public class Monster : Card {

	protected int level;
	protected int attack;
	protected int defence;
	protected string monsterTypeA;
	protected string monsterTypeB;
	protected string attribute;

	public int Level { get { return level; } }
	public int Attack { get { return attack; } }
	public int Defence { get { return defence; } }
	public string MonsterTypeA { get { return monsterTypeA; } }
	public string MonsterTypeB { get { return monsterTypeB; } }
	public string Attribute { get { return attribute; } } 

	public Monster(int id, string name, string a, string b,
		string attribute, int level, int attack, int defence, string text,
		string formula, Texture2D p) : base (id, name, text, formula, p) {
		this.level = level;
		this.attack = attack;
		this.defence = defence;
		this.monsterTypeA = a;
		this.monsterTypeB = b;
		this.attribute = attribute;
	}

	/// <summary>
	/// Outputs the card details for use in the UnityUI Text
	/// </summary>
	/// <returns>The complete card details</returns>
	public override string OutputCardDetails() {
		string returnString = name + "," + 
			"------------------------------" + "," + 
			monsterTypeA + "/" + monsterTypeB + "," +
			"------------------------------" + "," +
			"Level/Rank: " + level + "," +
			"Attribute: " + attribute + "," +
			"Attack: " + attack + "," +
			"Defence: " + defence + "," +
			"------------------------------" + "," +
			text;
		returnString = returnString.Replace (",", "\n");
		return returnString;
	}
}