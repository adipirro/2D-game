using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BattleCreator : MonoBehaviour {

    //array of characters being used by the 
    public Transform playerContainer;
    public Player[] playerCharacters = new Player[3];

    //pool of enemy characters
    public Transform enemyContainer;
    public List<Character> enemyCharacters = new List<Character>();

    //Gizmo locations
    Transform[] enemyLocations = new Transform[3];
    Transform[] playerLocations = new Transform[3];
    Transform attackPosition;

	// Use this for initialization
	void Start () {

        InitializeGizmoLocations();

        InitializePlayerCharacters();

        BattleManager.instance.StartBattle();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void InitializeGizmoLocations()
    {
        playerLocations[0] = GameObject.Find("Player1 Pos").transform;
        playerLocations[1] = GameObject.Find("Player2 Pos").transform;
        playerLocations[2] = GameObject.Find("Player3 Pos").transform;

        enemyLocations[0] = GameObject.Find("Enemy1 Pos").transform;
        enemyLocations[1] = GameObject.Find("Enemy2 Pos").transform;
        enemyLocations[2] = GameObject.Find("Enemy3 Pos").transform;

        attackPosition = GameObject.Find("Attack Pos").transform;
    }

    void InitializePlayerCharacters()
    {
        GameObject newCharacter;

        for (int i = 0; i < playerCharacters.Length; i++)
        {
            playerCharacters[i].GetComponent<Renderer>().sortingOrder = GetSortingOrder(i);

            //instantiate the character and update the player reference
            newCharacter = (GameObject)Instantiate(playerCharacters[i].gameObject);
            playerCharacters[i] = newCharacter.GetComponent<Player>();

            //position character and add it to the player container
            playerCharacters[i].attackPosition = attackPosition;
            playerCharacters[i].playerBasePosition = playerLocations[i];
            playerCharacters[i].transform.position = playerLocations[i].position;
            playerCharacters[i].transform.parent = playerContainer;

            BattleManager.instance.AddCharacter(playerCharacters[i]);
        }

        PlayerPrefs.DeleteAll();

    }

    //Used to get the sorting order for each character
    int GetSortingOrder(int index)
    {
        if (index == 0)
        {
            return 10;
        }
        else if(index == 1)
        {
            return 0;
        }
        else return 20;
    }
}
