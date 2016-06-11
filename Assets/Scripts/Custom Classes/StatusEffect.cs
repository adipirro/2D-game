using UnityEngine;
using System.Collections;

public enum EffectType
{
    None,
    Poison,
    Healing,
    Stealth,
    Freeze,
    Taunt,
    Counter,
    ShieldWall,
    AttackUp
};

public enum EffectApplyTime
{
    StartEachTurn,
    StartCharTurn,
    EndCharTurn
};

[System.Serializable]
public class StatusEffect{

    public EffectType effectType;
    public int effectValue;
    public int numTurns;
    public bool noCompoundTurns = false;

    public EffectApplyTime effectApplyTime;

    public StatusEffect()
    {
        effectType = EffectType.None;
    }

    public StatusEffect(StatusEffect s)
    {
        effectType = s.effectType;
        effectValue = s.effectValue;
        numTurns = s.numTurns;
        effectApplyTime = s.effectApplyTime;
    }

    public void ApplyStatusEffect(Character applyChar)
    {
        //apply based on the type of effect
        switch (effectType)
        {
            case EffectType.Poison:
                applyChar.TakeDamage(effectValue);
                break;
            case EffectType.Healing:
                applyChar.TakeDamage(-effectValue);
                break;
            default:
                break;
        }

        //decrement turn counter
        numTurns--;

    }

    public bool isFinished()
    {
        return numTurns == 0;
    }

}
