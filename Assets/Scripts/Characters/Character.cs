using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class Character : MonoBehaviour, IComparable<Character> {

    public Vector3 someVector;

    public bool onRightSide;

    //GUI Variables
    public Sprite portrait;

    //Character Stats
    public int maxHealth;
    public int maxEnergy;
    public int speedAttribute;

    //Updatable current stats
    [HideInInspector]
    public int currentHealth;
    [HideInInspector]
    public int currentEnergy;

    //Moving variables
    float movSpeed = .5f;
    protected bool inAttackPos;
    public Transform attackPosition;
    public Transform playerBasePosition;

    //Attaking variables
    protected bool canUseAction;
    bool attackPositionReached;
    bool basePositionReached;

    //Turn variables
    protected bool isCharactersTurn;

    //Targeting variables
    protected Character targetedCharacter = null;

    //Status variables
    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    [HideInInspector]
    public CharStatus charStatus;
    float statusBarsOffset = 0.2f;

    //Color variables
    SpriteRenderer spriteRenderer;
    Color currentColor;
    Color normalColor = Color.white;
    Color pulseColor = new Color(.25f, .25f, .25f, 1);
    Color frozenColor = new Color(0f, 1f, 1f , 1f);
    float stealthAlpha = .3f;
    //Color poisonColor = Color.green;
    float pulseTime = 1f;



    //Function used to delay a method call
    protected delegate void DelayedMethod();
    protected IEnumerator WaitAndDo(float time, DelayedMethod method)
    {
        yield return new WaitForSeconds(time);
        method();
    }

    //Inverse Comparer, higher speed goes first
    public int CompareTo(Character other)
    {
        if (other == null) return -1;
        else return -1 * (this.speedAttribute).CompareTo(other.speedAttribute);
    }

	// Use this for initialization
	public virtual void Awake () {

        currentHealth = maxHealth;
        currentEnergy = maxEnergy;

        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        StartCoroutine(ColorShift());

        InitializeStatusGUI();

	}

    public virtual void Start()
    {

    }

	// Update is called once per frame
	public virtual void Update () {

	}

    public virtual void UseAction(Action action, Character target)
    {
        //Subtract energy cost, need to also check if character has the required energy
        currentEnergy -= action.energyCost;
        UpdateStatusGUI();

        GUIController.instance.ChangeActionNotification(action.actionName);

        StartCoroutine(ExecuteAction(action, target));

        ////float angle = Vector3.Angle(renderer.bounds.center, target.renderer.bounds.center);
        ////print(angle);

        canUseAction = false;  //only one action per turn
    }

    IEnumerator ExecuteAction(Action action, Character target)
    {
        if (action.stationaryAction)
        {
            BattleManager.instance.RegisterAction(action, target);

            //stay in base position and finish turn
            StartCoroutine(WaitAndDo(movSpeed, EndTurn));
            yield return 0;
        }
        else
        {
            MoveToAttack();

            //wait until attack position is reached
            while (!attackPositionReached)
            {
                yield return 0;
            }

            //once attack position is reached, register the action and move back to base position
            BattleManager.instance.RegisterAction(action, target);

            attackPositionReached = false;

            yield return StartCoroutine(CheckForCounterAttack(target));
            
            StartCoroutine(WaitAndDo(movSpeed, MoveBack));  //delay the moveback to simulate the time it would take to play the animation

            //wait until base position is reached
            while (!basePositionReached)
            {
                yield return 0;
            }

            //once base position is reached, end the turn
            basePositionReached = false;
            EndTurn();
        }
    }

    public void MoveToAttack()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", attackPosition, "time", movSpeed, "oncomplete", "AttackPositionReached"));
    }

    public void MoveBack()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", playerBasePosition, "time", movSpeed, "oncomplete", "BasePositionReached"));
    }

    void AttackPositionReached()
    {
        attackPositionReached = true;
    }

    void BasePositionReached()
    {
        basePositionReached = true;
    }

    public virtual void EndTurn()
    {
        isCharactersTurn = false;

        ManageSpecialEffects(EffectApplyTime.EndCharTurn);
        ApplyStatusEffects(EffectApplyTime.EndCharTurn);

        BattleManager.instance.TurnOver();
    }

    //Function used for managing abnormal status effects
    //callID lets this function know where it was called from
    public void ManageSpecialEffects(EffectApplyTime applyTime)
    {
        if (applyTime == EffectApplyTime.EndCharTurn) //called from endturn
        {
            int stealthIndex = GetStatusIndex(EffectType.Stealth);
            
            if(stealthIndex != -1)
            {
                //stealth only ended by actions, if still active at end of turn extend until next turn
                statusEffects[stealthIndex].numTurns += 1;
            }
        }
    }

    //Called by the BattleManager to allow this character to attack
    public virtual bool TakeTurn()
    {
        ApplyStatusEffects(EffectApplyTime.StartCharTurn); //apply start of turn status effects

        if(this.HasStatusEffect(EffectType.Freeze))
        {
            EndTurn();
            return false; //cannot continue turn
        }

        canUseAction = true;
        isCharactersTurn = true;

        GUIController.instance.UpdateActiveMarkerPosition(this);

        return true; //can continue turn
    }

    public virtual void TimeOut()
    {
        canUseAction = false;
        EndTurn();
    }

    //Adds a status effect to this player
    //If the status is already applied, its duration is extended
    public void AddStatusEffect(StatusEffect effect)
    {
        //Add the status effect to effects list
        if (effect.effectType != EffectType.None)
        {
            if (HasStatusEffect(effect.effectType))
            {
                if (effect.noCompoundTurns)
                {
                    //no compound so set the num turns to the default value
                    statusEffects[GetStatusIndex(effect.effectType)].numTurns = effect.numTurns;
                }
                else
                {
                    //status already applied so just add more turns to it
                    statusEffects[GetStatusIndex(effect.effectType)].numTurns += effect.numTurns;
                }
            }
            else
            {
                //new status effect so add it to the effects list and add the icon
                statusEffects.Add(new StatusEffect(effect));

                //Add the icon if the effect has one
                charStatus.AddStatusIcon(effect.effectType);
            }
        }
    }

    public void ApplyStatusEffects(EffectApplyTime applyTime)
    {
        for(int i=0 ; i<statusEffects.Count ; i++)
        {
            if (statusEffects[i].effectApplyTime == applyTime)
            {
                statusEffects[i].ApplyStatusEffect(this);

                //remove the effect if it is done
                if (statusEffects[i].isFinished())
                {
                    RemoveStatusEffect(statusEffects[i].effectType);
                }
            }
        }
    }

    public void RemoveStatusEffect(EffectType effect)
    {
        for (int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].effectType == effect)
            {
                charStatus.RemoveStatusIcon(statusEffects[i]);
                statusEffects.RemoveAt(i);
            }
        }
    }

    public bool HasStatusEffect(EffectType effect)
    {
        foreach (StatusEffect s in statusEffects)
        {
            if (s.effectType == effect)
            {
                return true;
            }
        }

        return false;
    }

    public int GetStatusIndex(EffectType effect)
    {
        for (int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].effectType == effect)
            {
                return i;
            }
        }
        return -1;
    }

    public StatusEffect GetStatusEffect(EffectType effect)
    {
        for (int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].effectType == effect)
            {
                return statusEffects[i];
            }
        }
        return null;
    }

    //Called by battle manager when this character takes damage
    public void TakeDamage(int damageAmount)
    {
        currentHealth = Mathf.Min(currentHealth -= damageAmount, maxHealth);

        GUIController.instance.SpawnDamageIndicator(gameObject, damageAmount, false);

        if (currentHealth <= 0)
        {
            Die();
        }

        UpdateStatusGUI();
    }

    public void AddEnergy(int energyAmount)
    {
        currentEnergy = Mathf.Min(currentEnergy + energyAmount, maxEnergy);
        UpdateStatusGUI();
    }

    public void PlayMissEffect()
    {
        GUIController.instance.SpawnDamageIndicator(gameObject, 0, true);
    }

    void Die()
    {
        GameObject targetIcon, activeMarker;
        DamageIndicator[] damageIndicators;

        targetIcon = gameObject.FindInChildren("TargetIcon(Clone)");
        activeMarker = gameObject.FindInChildren("ActiveMarker(Clone)");
        damageIndicators = gameObject.GetComponentsInChildren<DamageIndicator>();

        if (targetIcon != null)
        {
            targetIcon.transform.parent = null;
        }

        if (activeMarker != null)
        {
            activeMarker.transform.parent = null;
        }

        if (damageIndicators.Length > 0)
        {
            foreach(DamageIndicator d in damageIndicators)
            {
                d.transform.parent = null;
            }
        }

        BattleManager.instance.CharacterDied(this);
        Destroy(gameObject);
    }

    public int GetQueuePosition()
    {
        return BattleManager.instance.GetQueuePosition(this);
    }

    //Initializes the gui representation of the character's health
    void InitializeStatusGUI()
    {
        charStatus = ((GameObject)Instantiate(Resources.Load("StatusBars"))).GetComponent<CharStatus>();

        charStatus.InitializeStatusBars(GetComponent<Renderer>().sortingLayerID, GetComponent<Renderer>().sortingOrder);

        charStatus.gameObject.transform.SetPosition2D(gameObject.GetComponent<Renderer>().bounds.center.x, gameObject.GetComponent<Renderer>().bounds.min.y - statusBarsOffset);
        charStatus.gameObject.transform.parent = gameObject.transform;

        UpdateStatusGUI();
    }

    //Updates all gui representations of this character's statue
    void UpdateStatusGUI()
    {
        charStatus.UpdateStatusBars(this);
    }

    public virtual void UpdateTargetedCharacter(Character c)
    {
        targetedCharacter = c;
        GUIController.instance.UpdateTargetIconPosition(targetedCharacter);
    }

    public Vector2 GetTargetMarkerLocation(bool onRightSide)
    {
        return charStatus.GetTargetMarkerLocation(onRightSide); ;
    }

    public bool HasRequiredEnergy(Action action)
    {
        if (action.energyCost > currentEnergy)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public IEnumerator CheckForCounterAttack(Character target)
    {
        //Countering
        foreach (Character c in BattleManager.instance.GetCombatantsOfType(target.GetType()))
        {
            if (c.HasStatusEffect(EffectType.Counter))
            {
                yield return StartCoroutine(c.CounterAttack(this));
            }
        }
    }

    public IEnumerator CounterAttack(Character charToCounter)
    {
        int damageAmount = statusEffects[GetStatusIndex(EffectType.Counter)].effectValue;
        Action counterAction = new Action("Counter", damageAmount, BuffType.None, this);

        MoveToAttack();

        //wait until attack position is reached
        while (!attackPositionReached)
        {
            yield return 0;
        }

        counterAction.DoAction(this, charToCounter);

        attackPositionReached = false;

        StartCoroutine(WaitAndDo(movSpeed, MoveBack));  //delay the moveback to simulate the time it would take to play the animation

        //wait until base position is reached
        while (!basePositionReached)
        {
            yield return 0;
        }

        //once base position is reached, end the turn
        basePositionReached = false;
    }


    //Used to change the character's color
    //Either on their turn or when a status effect is active
    IEnumerator ColorShift()
    {
        while (true)
        {
            //pulse between white and less white
            //when it is this characters turn
            if (isCharactersTurn)
            {
                currentColor = Color.Lerp(normalColor, pulseColor, Mathf.PingPong(Time.time * pulseTime, 1.0f));
            }
            else
            {
                currentColor = normalColor;
            }

            if (this.HasStatusEffect(EffectType.Freeze))
            {
                currentColor = frozenColor;
            }

            if (this.HasStatusEffect(EffectType.Stealth))
            {
                currentColor.a = stealthAlpha;
            }

            spriteRenderer.color = currentColor;
            yield return 0;
        }
    }

}
