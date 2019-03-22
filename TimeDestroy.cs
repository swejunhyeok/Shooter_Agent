using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeDestroy : MonoBehaviour {
    public float timeSpan;
    public float checkTime;

	// Use this for initialization
	void Start () {
        timeSpan = 0.0f;
        checkTime = 5.0f;
	}
	
	// Update is called once per frame
	void Update () {
        timeSpan += Time.deltaTime;
        if(timeSpan > checkTime)
        {
            Destroy(gameObject);
        }
	}
}
