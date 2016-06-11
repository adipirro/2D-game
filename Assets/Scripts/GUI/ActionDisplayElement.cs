using UnityEngine;
using System.Collections;

public class ActionDisplayElement : MonoBehaviour {

    public enum Location
    {
        Top,
        Bottom,
        Left,
        Right
    };

    public Location location;

    public SpriteRenderer positioningArrow;
    float positioningOffset = 0.1f;

    public SpriteRenderer actionImage;
    public TextMesh actionText;
    public SpriteRenderer healthOrb;
    public SpriteRenderer energyOrb;
    public SpriteRenderer hitChanceOrb;
    public SpriteRenderer numTurnsOrb;
    public TextMesh healthText;
    public TextMesh energyText;
    public TextMesh hitChanceText;
    public TextMesh numTurnsText;

    void Start()
    {
        SetSortingOrders();
    }

    public void SetSortingOrders()
    {
        //put orbs under text
        healthOrb.GetComponent<Renderer>().sortingOrder = actionText.GetComponent<Renderer>().sortingOrder - 1;
        energyOrb.GetComponent<Renderer>().sortingOrder = actionText.GetComponent<Renderer>().sortingOrder - 1;
        hitChanceOrb.GetComponent<Renderer>().sortingOrder = actionText.GetComponent<Renderer>().sortingOrder - 1;
        numTurnsOrb.GetComponent<Renderer>().sortingOrder = actionText.GetComponent<Renderer>().sortingOrder - 1;

        //put image under orbs
        actionImage.GetComponent<Renderer>().sortingOrder = healthOrb.GetComponent<Renderer>().sortingOrder;
    }

    public void UpdateElement(Action action, GUIController guiController)
    {
        RenderStats(action);

        actionText.text = action.actionName;
        healthText.text = Mathf.Abs(action.GetDamageAmount()) + "";
        energyText.text = action.GetEnergyCost() + "";
        hitChanceText.text = (int)(action.GetHitChance() * 100) + "%";

        if (action.HasLastingStatusEffect())
        {
            healthText.text = healthText.text + "+" + action.statusEffect.effectValue;
            numTurnsText.text = "x" + action.statusEffect.numTurns;
        }

        //Set the colors of the text meshes based on their buff values
        SetStatTextColor(healthText, action.DamageBuffValue(), guiController);
        SetStatTextColor(energyText, action.EnergyCostBuffValue(), guiController);
        SetStatTextColor(hitChanceText, action.HitChanceBuffValue(), guiController); 

        if (action.actionImage != null)
        {
            actionImage.sprite = action.actionImage;
        }
        else
        {
            actionImage.sprite = Resources.Load<Sprite>("x");
        }


        UpdatePosition();
    }

    //Sets the color of a textmesh based on a buff value
    //positive buff = green/heal color
    //negative buff = red/hurt color
    //no buff = white/miss color
    void SetStatTextColor(TextMesh textMesh, int buffValue, GUIController guiController)
    {
        if (buffValue < 0)
        {
            textMesh.color = guiController.hurtColor;
        }
        else if (buffValue > 0)
        {
            textMesh.color = guiController.healColor;
        }
        else
        {
            textMesh.color = guiController.missColor;
        }
    }

    //Hides or shows the health/energy/hitchance stats
    void RenderStats(Action action)
    {
        bool enabled = action.showStats;
        bool showNumTurns = enabled && action.HasLastingStatusEffect();

        healthOrb.GetComponent<Renderer>().enabled = enabled;
        energyOrb.GetComponent<Renderer>().enabled = enabled;
        hitChanceOrb.GetComponent<Renderer>().enabled = enabled;
        numTurnsOrb.GetComponent<Renderer>().enabled = showNumTurns;

        StartCoroutine(ShowHideOutlineText(healthText, enabled));
        StartCoroutine(ShowHideOutlineText(energyText, enabled));
        StartCoroutine(ShowHideOutlineText(hitChanceText, enabled));
        StartCoroutine(ShowHideOutlineText(numTurnsText, showNumTurns));
    }

    //ensures all outline elements are created before enabling/disabling them all
    IEnumerator ShowHideOutlineText(TextMesh t, bool enabled)
    {
        Renderer[] renderers = t.GetComponentsInChildren<Renderer>();

        while (renderers.Length < 9)
        {
            yield return 0;
            renderers = t.GetComponentsInChildren<Renderer>();
        }

        foreach (Renderer r in renderers)
        {
            r.enabled = enabled;
        }
    }

    /*******************
    * Positioning Functions
    * *****************/

    void UpdatePosition()
    {
        float newX, newY;

        newX = 0f;
        newY = 0f;

        switch (location)
        {
            case Location.Top:
                newX = positioningArrow.bounds.center.x + GetOffsetX();
                newY = positioningArrow.bounds.max.y + positioningOffset + (transform.position.y - GetMinY());
                break;
            case Location.Bottom:
                newX = positioningArrow.bounds.center.x + GetOffsetX();
                newY = positioningArrow.bounds.min.y - positioningOffset - (GetMaxY() - transform.position.y);
                break;
            case Location.Left:
                newX = positioningArrow.bounds.min.x - positioningOffset - (GetMaxX() - transform.position.x);
                newY = positioningArrow.bounds.center.y + GetOffsetY();
                break;
            case Location.Right:
                newX = positioningArrow.bounds.max.x + positioningOffset + (transform.position.x - GetMinX());
                newY = positioningArrow.bounds.center.y + GetOffsetY();
                break;
            default:
                break;
        }

        transform.SetPosition2D(newX, newY);
    }

    float GetMaxX()
    {
        float returnValue;

        if (healthOrb.enabled)
        {
            returnValue = Mathf.Max(hitChanceOrb.bounds.max.x, hitChanceText.GetComponent<Renderer>().bounds.max.x, actionText.GetComponent<Renderer>().bounds.max.x);
        }
        else
        {
            returnValue = actionText.GetComponent<Renderer>().bounds.max.x;
        }

        return returnValue;
    }

    float GetMaxY()
    {
        float returnValue;

        if (healthOrb.enabled)
        {
            returnValue = healthOrb.bounds.max.y;
        }
        else
        {
            returnValue = actionImage.bounds.max.y;
            returnValue -= healthOrb.bounds.extents.y;  //lots of whitespace in the current images
        }
        return returnValue;
    }

    float GetMinX()
    {
        float returnValue;

        if (healthOrb.enabled)
        {
            returnValue = Mathf.Min(energyOrb.bounds.min.x, energyText.GetComponent<Renderer>().bounds.min.x, actionText.GetComponent<Renderer>().bounds.min.x);
        }
        else
        {
            returnValue = actionText.GetComponent<Renderer>().bounds.min.x;
        }

        return returnValue;
    }

    float GetMinY()
    {
        return Mathf.Min(actionImage.bounds.min.y, actionText.GetComponent<Renderer>().bounds.min.y);
    }

    float GetOffsetY()
    {
        if (healthOrb.enabled)
        {
            return transform.position.y - MidPoint(healthOrb.GetComponent<Renderer>().bounds.center, actionText.GetComponent<Renderer>().bounds.center).y;
        }
        else
        {
            return transform.position.y - MidPoint(actionImage.GetComponent<Renderer>().bounds.center, actionText.GetComponent<Renderer>().bounds.center).y;
        }
    }

    float GetOffsetX()
    {
        return transform.position.x - MidPoint(actionImage.GetComponent<Renderer>().bounds.center, actionText.GetComponent<Renderer>().bounds.center).x;
    }

    Vector3 MidPoint(Vector3 a, Vector3 b)
    {
        Vector3 returnVector;

        returnVector.x = (a.x / 2) + (b.x / 2);
        returnVector.y = (a.y / 2) + (b.y / 2);
        returnVector.z = (a.z / 2) + (b.z / 2);

        return returnVector;
    }

}
