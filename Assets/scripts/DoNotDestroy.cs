/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Short script for preventing important gameobjects such as the GameManager
 * from being destroyed when moving to a new scene
*/

using UnityEngine;

public class DoNotDestroy : MonoBehaviour {
	void Start () {
		DontDestroyOnLoad (this);
	}
}