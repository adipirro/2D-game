using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AIBehaviour {

    //Input variables for the score test
    public Enemy actingCharacter;
    public Character targetCharacter;
    List<Character> targetList = new List<Character>();
    public Action action;
    bool multipleTargets;

    /*******************
     * Scoring Variables
     *******************/ 
    //Overall Score Value
    public int behaviourScore;

    //multipliers
    float aggressionMult = 1.5f;

    //kill scoring
    int killScore = 10;

    //heal scoring
    float[] healthLevels = {.10f, .25f, .50f, .75f};
    int[] healthScores = {10 , 5, 2, 1};

    //taunt scoring
    int tauntScore = 1000;

    public AIBehaviour(Enemy acting, Character target, Action action)
    {
        actingCharacter = acting;
        targetCharacter = target;
        this.action = action;
        multipleTargets = false;

        CalculateScore();
    }

    public AIBehaviour(Enemy acting, List<Character> targets, Action action)
    {
        actingCharacter = acting;
        targetList = targets;
        this.action = action;
        multipleTargets = true;

        CalculateScore();
    }

    void CalculateScore()
    {
        //action.SetModifiedStats(actingCharacter, targetCharacter);

        if (multipleTargets)
        {
            behaviourScore = KillScore() + HealScore() + TauntScore();
        }
        else
        {
            behaviourScore = KillScore() + HealScore() + TauntScore();
        }

        //If behaviour matches character's aggression level give it a bonus
        if (actingCharacter.aggressionLevel != AggressionLevel.Neutral && actingCharacter.aggressionLevel == GetAggressionLevel())
        {
            behaviourScore = (int)(behaviourScore * aggressionMult);
        }
        
    }

    public int GetScore()
    {
        return behaviourScore;
    }

    public Action GetAction()
    {
        return action;
    }

    public Character GetTargetCharacter()
    {
        if (multipleTargets)
        {
            return targetList[0];
        }
        else
        {
            return targetCharacter;
        }
    }

    AggressionLevel GetAggressionLevel()
    {
        if (targetCharacter.GetType() == actingCharacter.GetType())
        {
            return AggressionLevel.Passive;
        }
        else if (targetCharacter.GetType() != actingCharacter.GetType())
        {
            return AggressionLevel.Aggressive;
        }
        else
        {
            return AggressionLevel.Neutral;
        }
    }

    int KillScore()
    {
        int returnValue = 0;

        //Do not need this, but it avoides doing the damage checks
        if (action.TargetsFriendlyCharacters())
        {
            return 0;
        }

        if (multipleTargets)
        {
            //check for kill on each of the targets
            foreach (Character c in targetList)
            {
                action.SetModifiedStats(actingCharacter, c);

                if (c.currentHealth - action.GetDamageAmount() <= 0)
                {
                    returnValue += killScore;
                }
            }
        }
        else
        {
            //check for kill on single target
            if (targetCharacter.currentHealth - action.GetDamageAmount() <= 0)
            {
                returnValue = killScore;
            }
        }

        return returnValue;
    }

    int HealScore()
    {
        int returnValue = 0;

        //Do not need this, but it avoides doing the heal checks
        if (!action.TargetsFriendlyCharacters())
        {
            return 0;
        }

        if (multipleTargets)
        {
            //check for kill on each of the targets
            foreach (Character c in targetList)
            {
                action.SetModifiedStats(actingCharacter, c);

                returnValue += CharHealScore(c);
            }
        }
        else
        {
            returnValue += CharHealScore(targetCharacter);
        }

        return returnValue;
    }

    int CharHealScore(Character character)
    {
        int currHealth = character.currentHealth;
        int maxHealth = character.maxHealth;

        for(int i=0 ; i<healthLevels.Length ; i++)
        {
            //loops through health levels (<10%, <25%, etc..) until it figures
            //out how much health the character has. Then it returns the corresponding score
            if (currHealth <= (int)(maxHealth * healthLevels[i]))
            {
                return healthScores[i];
            }
        }

        return 0;
    }

    int TauntScore()
    {
        int returnValue = 0;

        //Do not need this, but it avoides doing the checks
        if (action.TargetsFriendlyCharacters())
        {
            return 0;
        }

        if (!multipleTargets)
        {
            if (targetCharacter.HasStatusEffect(EffectType.Taunt))
            {
                returnValue += tauntScore;
            }
        }

        return returnValue;
    }
}
