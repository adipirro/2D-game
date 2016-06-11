using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player: Character {

    //Used for saving
    public string savePrefix;

    //Action variables
    public List<Action> actionList = new List<Action>();
    public List<Action> actionTypes = new List<Action>();
    int actionMultiplier = -1;
    int actionOffset = -1;

    //Targeting variables
    RaycastHit2D hit;

	// Use this for initialization
	public override void Awake () {

        base.Awake();
	}
	
	// Update is called once per frame
	public override void Update () 
    {
        base.Update();

        if (canUseAction)
        {
            //Get target updates
            if (Input.GetMouseButtonDown(0))
            {
                hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector2(0, 0), 1);

                if (hit.transform != null) //may want to check tags
                {
                    UpdateTargetedCharacter(hit.transform.gameObject.GetComponent<Character>());
                }
            }

            GetActionInput();

        }
        else
        {
            //guiController.HideTargetIcon();
        }
	}

    public override void UseAction(Action action, Character target)
    {
        GUIController.instance.HideTargetIcon();
        base.UseAction(action, target);
    }

    //Sets an initial target for the player character
    void SetInitialTarget()
    {
        Character initialTarget = null;

        foreach (Character c in BattleManager.instance.combatants)
        {
            if (c.GetType() == typeof(Enemy))
            {
                if (initialTarget == null || c.currentHealth <= initialTarget.GetComponent<Enemy>().currentHealth)
                {
                    initialTarget = c;   
                }
            }
        }

        UpdateTargetedCharacter(initialTarget);
    }

    //Called every frame during the player's turn
    void GetActionInput()
    {
        //First, try to get input if multiplier or offset not set
        if (actionMultiplier == -1)
        {
            actionMultiplier = GetKeyInput();

            if (actionMultiplier != -1)
            {
                SoundManager.instance.PlaySelectSound();
                SetActionDisplayText();
            }
        }
        else if (actionOffset == -1)
        {
            actionOffset = GetKeyInput();
        }

        //When multiplier and offset are set an action can be used
        if (actionMultiplier != -1 && actionOffset != -1)
        {
            //Back Selected
            if (actionOffset == 3)
            {
                SoundManager.instance.PlayBackSound();
                GUIController.instance.UpdateActionDiplay(actionTypes);

                //Reset these to their non-set values
                actionMultiplier = -1;
                actionOffset = -1;
            }
            else
            {
                SoundManager.instance.PlaySelectSound();
                UseAction(actionList[(3 * actionMultiplier) + actionOffset], targetedCharacter);
                GUIController.instance.MoveADOffScreen();
            }
        }
    }

    //Returns a number based on which key the user pressed
    //Used to set multiplier and offset in GetActionInput
    int GetKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
            return 0;
        else if (Input.GetKeyDown(KeyCode.D))
            return 1;
        else if (Input.GetKeyDown(KeyCode.S))
            return 2;
        else if (Input.GetKeyDown(KeyCode.A))
            return 3;
        else
            return -1;
    }

    //Called when Action Multiplier is set
    void SetActionDisplayText()
    {
        int baseIndex = actionMultiplier * 3;
        string[] sArray = new string[4];
        List<Action> newList = new List<Action>();

        for (int i = 0 ; i < sArray.Length ; i++)
        {
            if (i == sArray.Length - 1)
            {
                //sArray[i] = "Back";
                newList.Add(new Action("Back"));
            }
            else
            {
                //sArray[i] = actionList[i + baseIndex].actionName;
                newList.Add(actionList[i + baseIndex]);
            }
        }

        GUIController.instance.UpdateActionDiplay(newList);
    }

    //Called by BattleManager when it is this player's turn
    //extension of the method from character
    public override bool TakeTurn()
    {
        if (!base.TakeTurn())
        {
            return false;
        }

        SetInitialTarget();

        GUIController.instance.UpdateActionDiplay(actionTypes);
        GUIController.instance.MoveADOnScreen();

        return true;
    }

    public override void TimeOut()
    {
        GUIController.instance.MoveADOffScreen();

        base.TimeOut();
    }

    public override void EndTurn()
    {
        //Reset these for the next turn
        actionMultiplier = -1;
        actionOffset = -1;

        base.EndTurn();
    }

    public override void UpdateTargetedCharacter(Character c)
    {
        foreach (Action a in actionList)
        {
            a.SetModifiedStats(this, c);
        }

        base.UpdateTargetedCharacter(c);
    }

}
