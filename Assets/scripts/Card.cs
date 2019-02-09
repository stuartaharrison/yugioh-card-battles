/*
 * Author: Stuart Harrison 
 * Date: January 2016
 * Class for holding all the information for a Spell/Trap card. Also acts as 
 * superclass to the Monster card class
*/
using UnityEngine;
using System;
using System.Collections;

[Serializable()]
public class Card {

	protected int id;
	protected Sprite image;
	protected string name;
	protected string text;
	protected string formula;
	private string type;

	//Getters
	public int ID { get { return id; } }
	public Sprite Image { get { return image; } }
	public string Name { get { return name; } }
	public string Text { get { return text; } }
	public string Formula { get { return formula; } }
	public string Type { get { return type; } }

	public Card(int id, string name, string text, string formula, Texture2D p) {
		this.id = id;
		this.name = name;
		this.text = text;
		this.formula = formula;

		Rect spriteRect = new Rect (0, 0, p.width, p.height);
		this.image = Sprite.Create (p, spriteRect, new Vector2 (0.5f, 0.5f));
	}

	public Card(int id, string name, string type, string text, string formula, 
		Texture2D p) {
		this.id = id;
		this.name = name;
		this.type = type;
		this.text = text;
		this.formula = formula;

		Rect spriteRect = new Rect (0, 0, p.width, p.height);
		this.image = Sprite.Create (p, spriteRect, new Vector2 (0.5f, 0.5f));
    }

	/// <summary>
	/// Outputs the card details for use in the UnityUI Text
	/// </summary>
	/// <returns>The complete card details</returns>
	public virtual string OutputCardDetails() {
		string returnString = name + "," + 
							"------------------------------" + "," + 
							type + "," +
							"------------------------------" + "," +
							text;
		returnString = returnString.Replace (",", "\n"); //Insert new lines
		//where I want them!
		return returnString;
	}
}