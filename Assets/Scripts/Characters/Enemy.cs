using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum AggressionLevel
{
    Neutral,
    Aggressive,
    Passive
};

public class Enemy : Character {

    float turnDelayTime = 0.5f;

    //Action variables
    public List<Action> actionList = new List<Action>();

    public AggressionLevel aggressionLevel;

    //AI variables
    public List<AIBehaviour> behaviourList = new List<AIBehaviour>();
    List<Character> enemyList = new List<Character>();
    List<Character> playerList = new List<Character>();

    // Use this for initialization
    public override void Awake()
    {

        base.Awake();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    //extension of the method from character
    public override bool TakeTurn()
    {
        if (!base.TakeTurn())
        {
            return false;
        }

        StartCoroutine(AIRoutine());

        return true;
    }

    public override void UpdateTargetedCharacter(Character c)
    {
        foreach (Action a in actionList)
        {
            a.SetModifiedStats(this, c);
        }

        base.UpdateTargetedCharacter(c);
    }

    IEnumerator AIRoutine()
    {
        yield return new WaitForSeconds(turnDelayTime);

        CreateBehaviourList();
        ExcecuteBestBehaviour();
    }

    void CreateBehaviourList()
    {
        //Reset the behaviour list
        behaviourList = new List<AIBehaviour>();

        //update enemy/player list in case people died
        enemyList = BattleManager.instance.GetCombatantsOfType(typeof(Enemy));
        playerList = BattleManager.instance.GetCombatantsOfType(typeof(Player));

        foreach (Action action in actionList)
        {
            if(HasRequiredEnergy(action))
            {
                if (action.TargetsFriendlyCharacters())
                {
                    if (action.affectsAll)
                    {
                        behaviourList.Add(new AIBehaviour(this, enemyList, action));
                    }
                    else
                    {
                        foreach (Enemy enemy in enemyList)
                        {
                            behaviourList.Add(new AIBehaviour(this, enemy, action));
                        }
                    }
                }
                else
                {
                    if (action.affectsAll)
                    {
                        behaviourList.Add(new AIBehaviour(this, playerList, action));
                    }
                    else
                    {
                        foreach (Player player in playerList)
                        {
                            behaviourList.Add(new AIBehaviour(this, player, action));
                        }
                    }
                }
            }
        }
    }

    public void ExcecuteBestBehaviour()
    {
        int maxScore = 0;
        int score = 0;
        AIBehaviour behaviourToExecute;

        //First find the best score
        for (int i = 0; i < behaviourList.Count; i++)
        {
            score = behaviourList[i].GetScore();

            if (score > maxScore)
            {
                maxScore = score;
            }
        }

        //Then delete all behaviours which are not equal to the max
        for (int i = 0; i < behaviourList.Count; i++)
        {
            if (behaviourList[i].GetScore() != maxScore)
            {
                behaviourList.RemoveAt(i);
                i--;
            }
        }

        //Pick a Random Behaviour from those left over
        behaviourToExecute = behaviourList[Random.Range(0, behaviourList.Count)];

        print("Acting: " + behaviourToExecute.actingCharacter + " Target: " + behaviourToExecute.targetCharacter + " Action: " + behaviourToExecute.action.actionName + " Score: " + behaviourToExecute.GetScore());

        UseAction(behaviourToExecute.GetAction(), behaviourToExecute.GetTargetCharacter());
    }
}
