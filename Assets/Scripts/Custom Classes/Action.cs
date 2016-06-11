using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BuffType
{
    None,
    Health,
    Energy
};

[System.Serializable]
public class Action {

    //Name shown on screen for the action
    public string actionName;

    public Sprite actionImage;

    public bool showStats = true;

    //Action Stats
    public int energyCost;
    public int actionValue;
    public float hitPercentage = 1;
    public bool affectsAll = false;
    public bool stationaryAction = false;
    public bool onlyAffectSelf = false;
    public bool hasProjectile = false;

    public BuffType buffType;

    public List<EffectType> effectsCanceledTarget;
    public List<EffectType> effectsCanceledUser;

    public StatusEffect statusEffect;

    public List<ActionModifier> actionModifiers = new List<ActionModifier>();

    int modifiedActionValue;
    int modifiedEnergyCost;
    float modifiedHitChance;

    //Basic constructor(possibly used for failed attacks)
    public Action()
    {
        this.actionName = "No Name";
        actionValue = 0;
        buffType = BuffType.None;
    }

    public Action(string name)
    {
        actionName = name;
        showStats = false;
    }

    //Constructor for creating an action
    public Action(string actionName, int hpAmount, BuffType buffType, Character actingCharacter)
    {
        this.actionName = actionName;
        this.actionValue = hpAmount;
        this.buffType = buffType;
        effectsCanceledTarget = new List<EffectType>();
        effectsCanceledUser = new List<EffectType>();
        statusEffect = new StatusEffect();
    }

    //Called when an action is ready to be applied
    public void DoAction(Character actingCharacter, Character targetCharacter)
    {
        int damageAmount;
        float randNum = UnityEngine.Random.value;

        //Make sure modified stats are set
        SetModifiedStats(actingCharacter, targetCharacter);

        //Check for miss
		if (randNum > GetHitChance())
        {
            targetCharacter.PlayMissEffect();
            return;
        }

        //might want to reorder these with addstatuseffect
        if (effectsCanceledTarget.Count != 0)
        {
            foreach (EffectType effect in effectsCanceledTarget)
            {
                targetCharacter.RemoveStatusEffect(effect);
            }
        }
        if (effectsCanceledUser.Count != 0)
        {
            foreach (EffectType effect in effectsCanceledUser)
            {
                actingCharacter.RemoveStatusEffect(effect);
            }
        }

        targetCharacter.AddStatusEffect(statusEffect);

        if (buffType == BuffType.Energy)
        {
            targetCharacter.AddEnergy(actionValue);
        }
        else if ((damageAmount = GetDamageAmount()) != 0)
        {
            targetCharacter.TakeDamage(damageAmount);
        }
    }

    //Called to get the damage this action deals
    //Trys to get modified stats using acting character's target
    //This is the actual damage which will be dealt to the targeted character
    //If this action heals it deals negative damage
    public int GetDamageAmount()
    {
        int damageAmount = modifiedActionValue;

        //Deal negative damage to heal
        if (buffType == BuffType.Health)
        {
            damageAmount = -damageAmount;
        }

        return damageAmount;
    }

    public int GetEnergyCost()
    {
        return modifiedEnergyCost;
    }

    public float GetHitChance()
    {
        return modifiedHitChance;
    }

    //Buff value functions compare modified stats to base stats and
    //return 1 if positive buff, 0 if equal, and -1 if negative buff
    public int DamageBuffValue()
    {
        if (modifiedActionValue > actionValue)
        {
            return 1;
        }
        else if (modifiedActionValue == actionValue)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public int EnergyCostBuffValue()
    {
        if (modifiedEnergyCost < energyCost)
        {
            return 1;
        }
        else if (modifiedEnergyCost == energyCost)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public int HitChanceBuffValue()
    {
        if (modifiedHitChance > hitPercentage)
        {
            return 1;
        }
        else if (modifiedHitChance == hitPercentage)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public bool HasLastingStatusEffect()
    {
        return (statusEffect != null && statusEffect.effectType != EffectType.None && statusEffect.numTurns != 0);
    }

    public bool TargetsFriendlyCharacters()
    {
        if (buffType != BuffType.None)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetModifiedStats(Character actingCharacter, Character targetCharacter)
    {
        modifiedActionValue = actionValue;
        modifiedEnergyCost = energyCost;
        modifiedHitChance = hitPercentage;

        //Get modifications from action modifiers
        foreach (ActionModifier am in actionModifiers)
        {
            if (am.bothStatusesRequired)
            {
                if (actingCharacter.HasStatusEffect(am.actingCharStatus) && targetCharacter.HasStatusEffect(am.targetCharStatus))
                {
                    modifiedActionValue += am.valueModifyAmount;
                    modifiedEnergyCost += am.energyModifyAmount;
                    modifiedHitChance += am.prctModifyAmount;
                }
            }
            else if (am.actingCharStatus != EffectType.None && actingCharacter.HasStatusEffect(am.actingCharStatus))
            {
                modifiedActionValue += am.valueModifyAmount;
                modifiedEnergyCost += am.energyModifyAmount;
                modifiedHitChance += am.prctModifyAmount;
            }
            else if (am.targetCharStatus != EffectType.None && targetCharacter.HasStatusEffect(am.targetCharStatus))
            {
                modifiedActionValue += am.valueModifyAmount;
                modifiedEnergyCost += am.energyModifyAmount;
                modifiedHitChance += am.prctModifyAmount;
            }
        }

        //Get modifications from attack buffs
        if(actingCharacter.HasStatusEffect(EffectType.AttackUp))
        {
            modifiedActionValue += (int)Mathf.Ceil((modifiedActionValue * actingCharacter.GetStatusEffect(EffectType.AttackUp).effectValue) / 100);
        }
    }

}
