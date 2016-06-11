using UnityEngine;
using System.Collections;

public class DamageIndicator : MonoBehaviour {

    public float lifetime;
    public float moveSpeed;

	// Use this for initialization
	void Start () {

       Destroy(gameObject, lifetime);
	
	}
	
	// Update is called once per frame
	void Update () {

        transform.SetPosition2D(transform.position.x, transform.position.y +(moveSpeed * Time.deltaTime));
	}
}
