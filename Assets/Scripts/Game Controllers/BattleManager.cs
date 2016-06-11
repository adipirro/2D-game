using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour {

    //Here is a private reference only this class can access
    private static BattleManager _instance;

    //This is the public reference that other classes will use
    public static BattleManager instance
    {
        get
        {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<BattleManager>();
            return _instance;
        }
    }

    //Debug toggle
    public bool debugMode;

    //Turn Queue variables
    public List<Character> combatants = new List<Character>();
    public List<Character> turnQueue = new List<Character>();
    public int queueSize = 6;
    public int queuePointer;

	// Use this for initialization
	void Start () 
    {
        //For the test arena
        if (debugMode)
        {
            StartBattle();
        }
	}
	
	// Update is called once per frame
	void Update () 
    {

	}

    public void AddCharacter(Character newChar)
    {
        combatants.Add(newChar);
    }

    //Will be called by BattleCreator when the battle is ready to start
    public void StartBattle()
    {
        SoundManager.instance.PlayBattleMusic();

        combatants.Sort(); //Order combatants by speed (Highest -> Lowest)
        queuePointer = 0;  //<--Could be changed maybe
        UpdateTurnQueue(); //Initialize Turn Queue
        StartNewTurn(); //Start the first turn
    }

    //Used to start a new turn in the battle
    void StartNewTurn()
    {
        //Before character is notified, apply any status effects
        for(int i=0 ; i<combatants.Count ; i++)
        {
            combatants[i].ApplyStatusEffects(EffectApplyTime.StartEachTurn);
        }

        NotifyActingCharacter();
    }

    //Tells the character at the front of the queue to take its turn and sets the timer variables
    void NotifyActingCharacter()
    {
        turnQueue[0].TakeTurn();

        if (turnQueue[0].GetType() == typeof(Player))
        {
            GUIController.instance.StartTimer(gameObject);
        }
    }

    //recieves actions from player/enemies
    public void RegisterAction(Action action, Character affectedChar)
    {
        if (action.onlyAffectSelf)
        {
            affectedChar = turnQueue[0];
        }

        if (action.affectsAll)
        {
            List<Character> charList = new List<Character>();
            Type affectedType;

            affectedType = affectedChar.GetType();

            //Create a list of characters to affect
            foreach (Character c in combatants)
            {
                if (c.GetType() == affectedType)
                {
                    charList.Add(c);
                }
            }

            //Send action to each affected player
            foreach (Character c in charList)
            {
                action.DoAction(turnQueue[0], c);
            }
        }
        else
        {
            action.DoAction(turnQueue[0], affectedChar);
        }

        GUIController.instance.StopTimer();
    }

    //Called by the guiController if the timer times out
    public void TimeOut()
    {
        turnQueue[0].TimeOut();
        //TurnOver();                 //This should be fine since everything is run in a single thread
    }

    //When player completes their turn they call this method
    public void TurnOver()
    {
        turnQueue.RemoveAt(0); //Remove acting character from the front of the list
        UpdateTurnQueue();
        StartNewTurn();
    }

    public void CharacterDied(Character deadCharacter)
    {
        Character nextCharacter;

        //Update queue pointer if it was pointing to the dead character
        if (combatants[queuePointer].Equals(deadCharacter))
        {
            if (queuePointer == combatants.Count - 1)
            {
                queuePointer = 0;
            }
            else
            {
                queuePointer++;
            }
        }

        //Get a copy of the character pointed to before dead character removal
        nextCharacter = combatants[queuePointer];

        //Remove dead character from the queue and combatant list
        while (turnQueue.Contains(deadCharacter))
        {
            turnQueue.Remove(deadCharacter);
        }
        combatants.Remove(deadCharacter);

        //Make sure queue pointer is still pointing to the correct character
        queuePointer = combatants.IndexOf(nextCharacter);

        UpdateTurnQueue();
    }

    //Used at the end of each turn, initialization, and when a character dies
    void UpdateTurnQueue()
    {
        //Add characters to turn Queue until it is full
        while (turnQueue.Count < queueSize)
        {
            turnQueue.Add(combatants[queuePointer]);

            if (queuePointer == combatants.Count - 1)
            {
                queuePointer = 0;
            }
            else
            {
                queuePointer++;
            }
        }

        GUIController.instance.UpdateQueuePortraits(turnQueue);
    }

    public int GetQueuePosition(Character c)
    {
        return turnQueue.IndexOf(c);
    }

    public List<Character> GetCombatants()
    {
        return combatants;
    }

    public List<Character> GetCombatantsOfType(Type charType)
    {
        List<Character> returnList = new List<Character>();

        foreach (Character c in combatants)
        {
            if (c.GetType() == charType)
            {
                returnList.Add(c);
            }
        }

        return returnList;
    }
}
