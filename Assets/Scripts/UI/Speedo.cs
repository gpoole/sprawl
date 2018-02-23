using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Speedo : MonoBehaviour {

	public CarController car;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		GetComponent<Text>().text = Mathf.Round(car.GetSpeed()) + " km/h";
	}
}
