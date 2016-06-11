using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//Excecute in edit mode can cause itween component buildup
//[ExecuteInEditMode]
public class GUIController : MonoBehaviour
{
    //Here is a private reference only this class can access
    private static GUIController _instance;

    //This is the public reference that other classes will use
    public static GUIController instance
    {
        get
        {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<GUIController>();
            return _instance;
        }
    }

    //Queue Variables
    public List<SpriteRenderer> portraits = new List<SpriteRenderer>();

    //Damage Indicator Variables
    public Color hurtColor;
    public Color healColor;
    public Color missColor;

    //Timer Variables
    public GameObject timerContainer;
    public GameObject timerBar;
    public float playerTime;
    public float timerMoveSpeed;
    GameObject timeoutCallback;
    float baseScale;
    float currentTime;
    bool timerRunning;
    Vector3 timerShowLocation;
    Vector3 timerHideLocation;

    //Targeting Variables
    GameObject targetIcon;
    GameObject smallTargetIcon;

    //ActiveMarker Variables
    GameObject activeIcon;

    //Action Notification Variables
    public GameObject actionNotificationBackground;
    public float ANmoveSpeed;
    public float ANlifetime;
    float ANcurrentTime;
    TextMesh ANtext;
    Vector3 ANshowLocation;
    Vector3 ANhideLocation;

    //Action Display Variables
    public GameObject actionDisplay;
    Vector3 ADonScreenPosition;
    Vector3 ADoffScreenPosition;
    public float ADmoveSpeed;
    float ADScreenOffset = 1.0f;
    ActionDisplayElement[] ADelements = new ActionDisplayElement[4];

    //Status Effect Variables
    public List<EffectIconPair> effectIconPairings = new List<EffectIconPair>();

    /*Stat Widget Variables
    public List<StatWidget> lhsWidgets = new List<StatWidget>();
    public List<StatWidget> rhsWidgets = new List<StatWidget>();
    float widgetBaseScale = 0.5f;
    float widgetSmallScale = 0.3f;
     */

    void Awake()
    {
        PositionGUI();
        InitializeTargetIcon();
        InitializeActiveMarker();
        InitializeTimer();
        InitializeActionDisplay();
    }

    // Use this for initialization
    void Start()
    {
        InitializeActionNotification();
    }

    // Update is called once per frame
    void Update()
    {
        TickActionNotification();
    }

    void FixedUpdate()
    {
        TickTimer();
    }

    void PositionGUI()
    {
        //Set the position of the main queue/timer objects to be at the edge of the camera
        float yPosition = (Camera.main.orthographicSize + Camera.main.transform.position.y) - (portraits[0].GetComponent<Renderer>().bounds.max.y - transform.position.y);
        transform.SetPosition2D(transform.position.x, yPosition);
    }

    /*****************************
    * Queue Functions 
    ****************************/

    public void UpdateQueuePortraits(List<Character> queue)
    {
        if (queue.Count != portraits.Count)
        {
            Debug.Log("UpdateQueuePortraits: List lengths do not match");
        }

        for (int i = 0; i < queue.Count ; i++)
        {
            portraits[i].sprite = queue[i].portrait;
        }
    }


    /*****************************
    * Damage Indicator Functions 
    ****************************/

    public void SpawnDamageIndicator(GameObject affectedCharacter, int damageAmount, bool miss)
    {
        GameObject dIndicator = (GameObject)Instantiate(Resources.Load("DamageIndicator"));
        TextMesh textMesh = dIndicator.GetComponent<TextMesh>();

        dIndicator.transform.parent = affectedCharacter.transform;

        dIndicator.GetComponent<Renderer>().sortingLayerID = affectedCharacter.GetComponent<Renderer>().sortingLayerID;
        dIndicator.GetComponent<Renderer>().sortingOrder = affectedCharacter.GetComponent<Renderer>().sortingOrder + 1;

        dIndicator.transform.SetPosition2D(affectedCharacter.GetComponent<Renderer>().bounds.center.x, affectedCharacter.GetComponent<Renderer>().bounds.max.y);

        if (!miss)
        {
            textMesh.text = Mathf.Abs(damageAmount).ToString();

            if (damageAmount > 0)
            {
                textMesh.color = hurtColor;
            }
            else if (damageAmount < 0)
            {
                textMesh.color = healColor;
            }
            else
            {
                textMesh.color = missColor;
            }
        }
        else
        {
            textMesh.text = "Miss";
            textMesh.color = missColor;
        }

    }

    /*****************************
     * Timer Functions 
    ****************************/

    void InitializeTimer()
    {
        timerRunning = false;
        baseScale = timerBar.transform.localScale.x;

        //This is dependent on setting up the scene correctly
        //action notification should be placed in show location to start
        timerShowLocation = timerContainer.transform.localPosition;
        timerHideLocation = new Vector3(timerShowLocation.x, 0.03f, 0);  //Magic Number!

        timerContainer.transform.localPosition = timerHideLocation;
    }

    void TickTimer()
    {
        //Timer managment
        if (timerRunning)
        {
            if (TimeRemaining())
            {
                timerBar.transform.localScale = new Vector3(currentTime * (baseScale / playerTime), timerBar.transform.localScale.y, timerBar.transform.localScale.z);
                currentTime -= Time.fixedDeltaTime;
            }
            else
            {
                print("Ran out of time");
                StopTimer();
                timeoutCallback.SendMessage("TimeOut");
            }
        }
    }

    public void StartTimer(GameObject sender)
    {
        currentTime = playerTime;
        timerRunning = true;
        timeoutCallback = sender;
        ShowTimer();
    }

    //"Hides" the timer by setting scale to 0
    public void StopTimer()
    {
        timerRunning = false;
        timerBar.transform.localScale = new Vector3(0, timerBar.transform.localScale.y, timerBar.transform.localScale.z);
        HideTimer();
    }

    void HideTimer()
    {
        iTween.MoveTo(timerContainer, iTween.Hash("position", timerHideLocation, "islocal", true, "time", timerMoveSpeed));
    }

    void ShowTimer()
    {
        iTween.MoveTo(timerContainer, iTween.Hash("position", timerShowLocation, "islocal", true, "time", timerMoveSpeed));
    }

    //Function for checking if there is time left for the player to act
    bool TimeRemaining()
    {
        if (currentTime >= 0)
        {
            return true;
        }

        return false;
    }

    /*****************************
     * Targeting Functions 
    ****************************/

    void InitializeTargetIcon()
    {
        targetIcon = (GameObject)Instantiate(Resources.Load("TargetIcon"));
        smallTargetIcon = (GameObject)Instantiate(Resources.Load("SmallTargetIcon"));
    }

    public void UpdateTargetIconPosition(Character targetCharacter)
    {
        //target icon
        Vector2 targetLocation;

        //small target icon
        int index = targetCharacter.GetQueuePosition();
        float newX, newY;

        if (targetCharacter.GetType() == typeof(Player))
        {
            targetIcon.GetComponent<SpriteRenderer>().color = healColor;
            smallTargetIcon.GetComponent<SpriteRenderer>().color = healColor;
        }
        else
        {
            targetIcon.GetComponent<SpriteRenderer>().color = hurtColor;
            smallTargetIcon.GetComponent<SpriteRenderer>().color = hurtColor;
        }

        targetIcon.GetComponent<Renderer>().sortingLayerID = targetCharacter.GetComponent<Renderer>().sortingLayerID;
        targetIcon.GetComponent<Renderer>().sortingOrder = targetCharacter.GetComponent<Renderer>().sortingOrder + 1;
        targetIcon.transform.parent = targetCharacter.transform;

        targetLocation = targetCharacter.GetTargetMarkerLocation(targetCharacter.onRightSide);

        //If targeting self put target marker on other side
        if (targetLocation.x == activeIcon.transform.position.x)
        {
            targetLocation = targetCharacter.charStatus.GetTargetMarkerLocation(!targetCharacter.onRightSide);
        }

        targetIcon.transform.SetPosition2D(targetLocation.x, targetLocation.y);

        //small target icon positioning
        newX = portraits[index].GetComponent<Renderer>().bounds.max.x - (smallTargetIcon.GetComponent<Renderer>().bounds.max.x - smallTargetIcon.GetComponent<Renderer>().bounds.center.x);
        newY = portraits[index].GetComponent<Renderer>().bounds.min.y + (smallTargetIcon.GetComponent<Renderer>().bounds.center.y - smallTargetIcon.GetComponent<Renderer>().bounds.min.y); ;

        smallTargetIcon.transform.SetPosition2D(newX, newY);
    }

    public void HideTargetIcon()
    {
        targetIcon.transform.SetPosition2D(100, 100);  //moves target off-screen
        smallTargetIcon.transform.SetPosition2D(100, 100); 
    }

    /*****************************
     * Active Marker Functions 
    ****************************/

    void InitializeActiveMarker()
    {
        activeIcon = (GameObject)Instantiate(Resources.Load("ActiveMarker"));
    }

    public void UpdateActiveMarkerPosition(Character activeCharacter)
    {
        Vector2 activeLocation;

        activeIcon.GetComponent<Renderer>().sortingLayerID = activeCharacter.GetComponent<Renderer>().sortingLayerID;
        activeIcon.GetComponent<Renderer>().sortingOrder = activeCharacter.GetComponent<Renderer>().sortingOrder + 1;
        activeIcon.transform.parent = activeCharacter.transform;

        activeLocation = activeCharacter.charStatus.GetActiveMarkerLocation(activeCharacter.onRightSide);

        activeIcon.transform.SetPosition2D(activeLocation.x, activeLocation.y);
    }

    public void HideActiveIcon()
    {
        targetIcon.transform.SetPosition2D(100, 100);  //moves icon off-screen
    }

    /*****************************
     * Action Notification Functions 
    ****************************/

    void InitializeActionNotification()
    {
        ANtext = actionNotificationBackground.transform.GetComponentInChildren<TextMesh>();
        ANtext.gameObject.GetComponent<Renderer>().sortingLayerID = actionNotificationBackground.GetComponent<Renderer>().sortingLayerID;
        ANtext.gameObject.GetComponent<Renderer>().sortingOrder = actionNotificationBackground.GetComponent<Renderer>().sortingOrder + 1;

        //This is dependent on setting up the scene correctly
        //action notification should be placed in show location to start
        ANshowLocation = actionNotificationBackground.transform.localPosition;
        ANhideLocation = new Vector3(ANshowLocation.x, 0, 0);

        actionNotificationBackground.transform.localPosition = ANhideLocation;
    }

    void TickActionNotification()
    {
        if (ANcurrentTime > 0)
        {
            ANcurrentTime -= Time.deltaTime;

            if (ANcurrentTime <= 0)
                HideActionNotification();
        }
    }

    void HideActionNotification()
    {
        iTween.MoveTo(actionNotificationBackground, iTween.Hash("position", ANhideLocation, "islocal", true, "time", ANmoveSpeed));
    }

    void ShowActionNotification()
    {
        iTween.MoveTo(actionNotificationBackground, iTween.Hash("position", ANshowLocation, "islocal", true, "time", ANmoveSpeed));
        ANcurrentTime = ANlifetime;
    }

    public void ChangeActionNotification(string actionName)
    {
        ANtext.text = actionName;

        if (actionNotificationBackground.transform.localPosition != ANshowLocation)
        {
            ShowActionNotification();
        }
    }

    /*****************************
     * Action Display Functions 
     ****************************/

    void InitializeActionDisplay()
    {
        //This is dependent on setting up the scene correctly
        //action display should be placed in on screen location to start
        ADonScreenPosition = actionDisplay.transform.position;

        InitializeADElements();

        float offYPosition = (Camera.main.transform.position.y - Camera.main.orthographicSize) - (ADelements[0].healthOrb.GetComponent<Renderer>().bounds.max.y - actionDisplay.transform.position.y) - ADScreenOffset;
        ADoffScreenPosition = new Vector3(ADonScreenPosition.x, offYPosition, ADonScreenPosition.z);

        //Action Display starts off screen
        actionDisplay.transform.position = ADoffScreenPosition;
    }

    void InitializeADElements()
    {
        GameObject[] gameObjects = new GameObject[4];

        //Spawn the action display elements as gameobjects
        for (int i = 0; i < gameObjects.Length; i++)
        {
            gameObjects[i] = (GameObject)Instantiate(Resources.Load("ActionDisplayElement"));
        }

        //transfer the references to ADelements
        for (int i = 0; i < gameObjects.Length; i++)
        {
            ADelements[i] = gameObjects[i].GetComponent<ActionDisplayElement>();
            ADelements[i].transform.parent = actionDisplay.transform;
        }

        //Set up the locations and positioning arrows of each element
        ADelements[0].location = ActionDisplayElement.Location.Top;
        ADelements[0].positioningArrow = actionDisplay.FindInChildren("Up Arrow").GetComponent<SpriteRenderer>();
        ADelements[1].location = ActionDisplayElement.Location.Right;
        ADelements[1].positioningArrow = actionDisplay.FindInChildren("Right Arrow").GetComponent<SpriteRenderer>();
        ADelements[2].location = ActionDisplayElement.Location.Bottom;
        ADelements[2].positioningArrow = actionDisplay.FindInChildren("Down Arrow").GetComponent<SpriteRenderer>();
        ADelements[3].location = ActionDisplayElement.Location.Left;
        ADelements[3].positioningArrow = actionDisplay.FindInChildren("Left Arrow").GetComponent<SpriteRenderer>();
    }

    public void MoveADOnScreen()
    {
        //The itween for move offscreen could still be going
        //This ensures the move on screen can be done
        iTween.Stop(gameObject);

        iTween.MoveTo(actionDisplay, iTween.Hash("position", ADonScreenPosition, "time", ADmoveSpeed, "easetype", iTween.EaseType.easeOutBounce));
    }

    public void MoveADOffScreen()
    {
        iTween.MoveTo(actionDisplay, iTween.Hash("position", ADoffScreenPosition, "time", ADmoveSpeed, "easetype", iTween.EaseType.easeInElastic));
    }

    public void UpdateActionDiplay(List<Action> actions)
    {
        if (actions.Count != ADelements.Length)
        {
            Debug.Log("Action Display Error: Non-Matching array lengths");
            return;
        }

        for (int i = 0; i < actions.Count; i++)
        {
            ADelements[i].UpdateElement(actions[i], this);
        }

    }

    /*****************************
    * Action Display Functions 
     ****************************/

    public Sprite GetEffectSprite(EffectType effectType)
    {
        foreach (EffectIconPair pair in effectIconPairings)
        {
            if (pair.effectType == effectType)
            {
                return pair.icon;
            }
        }

        return null;
    }

    /*****************************
    * Stat Widget Functions 
     ****************************

    void InitializeStatWidgets()
    {
        List<Character> combatants = GameObject.Find("Battle Manager").GetComponent<BattleManager>().GetCombatants();
        
        GameObject rhsParent = (GameObject)Instantiate(new GameObject());
        GameObject lhsParent = (GameObject)Instantiate(new GameObject());

        StatWidget newStatWidget;
        GameObject newWidget;

        rhsParent.name = "RHS Stats";
        lhsParent.name = "LHS Stats";
        rhsParent.transform.parent = transform;
        lhsParent.transform.parent = transform;

        //Make a stat widget for each combatant
        foreach (Character c in combatants)
        {
            newWidget = (GameObject)Instantiate(Resources.Load("StatWidget"));

            //Players get flipped widgets on the left hand side of the screen
            if (c.GetType() == typeof(Player))
            {
                newWidget.transform.parent = lhsParent.transform;

                if (lhsWidgets.Count != 0)
                {
                    newStatWidget = new StatWidget(newWidget, c, widgetSmallScale, true);
                }
                else
                {
                    newStatWidget = new StatWidget(newWidget, c, widgetBaseScale, true);
                }

                lhsWidgets.Add(newStatWidget);
            }
            else  //Enemies get non-flipped widgets on the right hand side of the screen
            {
                newWidget.transform.parent = rhsParent.transform;

                if (rhsWidgets.Count != 0)
                {
                    newStatWidget = new StatWidget(newWidget, c, widgetSmallScale, false);
                }
                else
                {
                    newStatWidget = new StatWidget(newWidget, c, widgetBaseScale, false);
                }

                rhsWidgets.Add(newStatWidget);
            }
        }

        PositionWidgets();
    }

    //Positions the widgets in the correct spot on the screen
    void PositionWidgets()
    {
        //Position lhs widgets
        for (int i = 0; i < lhsWidgets.Count; i++)
        {
            if (i == 0)  //position based on screen
            {
                lhsWidgets[i].AlignTo(GetScreenTopLeft());
            }
            else  //Position based on previous
            {
                lhsWidgets[i].AlignTo(lhsWidgets[i - 1].GetAlignmentPosition());
            }
        }

        //Position rhs widgets
        for (int i = 0; i < rhsWidgets.Count; i++)
        {
            if (i == 0)  //position based on screen
            {
                rhsWidgets[i].AlignTo(GetScreenTopRight());
            }
            else  //Position based on previous
            {
                rhsWidgets[i].AlignTo(rhsWidgets[i - 1].GetAlignmentPosition());
            }
        }
    }

    //I may want to come back to this
    //Reorders the characters/portraits to match order in the queue
    //void UpdateWidgetOrder(List<Character> queue)
    //{
    //    int count = 0;

    //    print("Updating");

    //    //First order the lhs widgets
    //    for (int i = 0; i < queue.Count; i++)
    //    {
    //        if (queue[i].GetType() == typeof(Player))
    //        {
    //            if (count < lhsWidgets.Count - 1)
    //            {
    //                print(count +" swapping " + lhsWidgets.IndexOfCharacter(queue[i]));
    //                StatWidget.Swap(lhsWidgets[count], lhsWidgets[lhsWidgets.IndexOfCharacter(queue[i])]);
    //            }
    //            else
    //            {
    //                break;
    //            }
    //            count++;
    //        }
    //    }

    //    count = 0;

    //    //Then order the rhs widgets
    //    for (int i = 0; i < queue.Count; i++)
    //    {
    //        if (queue[i].GetType() == typeof(Enemy))
    //        {
    //            if (count < lhsWidgets.Count - 1)
    //            {
    //                print(count + " swapping " + rhsWidgets.IndexOfCharacter(queue[i]));
    //                StatWidget.Swap(rhsWidgets[count], rhsWidgets[rhsWidgets.IndexOfCharacter(queue[i])]);
    //            }
    //            else
    //            {
    //                break;
    //            }
    //            count++;
    //        }
    //    }
    //}

    //Updates the corresponding widget for a character

    public void UpdateWidgetStats(Character c)
    {
        List<StatWidget> widgetList;

        if (c.GetType() == typeof(Player))
        {
            widgetList = lhsWidgets;
        }
        else
        {
            widgetList = rhsWidgets;
        }

        foreach (StatWidget s in widgetList)
        {
            if (s.myCharacter.Equals(c))
            {
                s.UpdateStats();
                return;
            }
        }
    }

    Vector2 GetScreenTopLeft()
    {
        float vertExtent = Camera.main.orthographicSize;
        float horzExtent = vertExtent * ((float)Screen.width/(float)Screen.height);

        return new Vector2(-horzExtent + Camera.main.transform.position.x, vertExtent + Camera.main.transform.position.y);
    }

    Vector2 GetScreenTopRight()
    {
        float vertExtent = Camera.main.orthographicSize;
        float horzExtent = vertExtent * ((float)Screen.width / (float)Screen.height);

        return new Vector2(horzExtent + Camera.main.transform.position.x, vertExtent + Camera.main.transform.position.y);
    }
     */ 

}
