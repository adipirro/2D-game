using UnityEngine;
using System.Collections;

[System.Serializable]
public class ActionModifier {

    public EffectType actingCharStatus;
    public EffectType targetCharStatus;
    public bool bothStatusesRequired;

    public int valueModifyAmount;
    public int energyModifyAmount;
    public float prctModifyAmount;

}
