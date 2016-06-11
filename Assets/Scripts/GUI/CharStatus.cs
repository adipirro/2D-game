using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharStatus : MonoBehaviour {

    public GameObject background;
    public GameObject healthBar;
    public TextMesh healthText;
    public GameObject energyBar;
    public TextMesh energyText;

    public SpriteRenderer activeMarker;
    public SpriteRenderer targetMarker;

    //Status effect icons
    public List<SpriteRenderer> statusEffectIcons = new List<SpriteRenderer>();

    public void InitializeStatusBars(int layerID, int layerOrder)
    {
        //Text meshes on top
        healthText.GetComponent<Renderer>().sortingLayerID = layerID;
        healthText.GetComponent<Renderer>().sortingOrder = layerOrder;
        energyText.GetComponent<Renderer>().sortingLayerID = layerID;
        energyText.GetComponent<Renderer>().sortingOrder = layerOrder;

        //Foreground
        GetComponent<Renderer>().sortingLayerID = layerID;
        GetComponent<Renderer>().sortingOrder = layerOrder - 1;

        //Bars
        healthBar.GetComponent<Renderer>().sortingLayerID = layerID;
        healthBar.GetComponent<Renderer>().sortingOrder = layerOrder - 2;
        energyBar.GetComponent<Renderer>().sortingLayerID = layerID;
        energyBar.GetComponent<Renderer>().sortingOrder = layerOrder - 2;

        //Background
        background.GetComponent<Renderer>().sortingLayerID = layerID;
        background.GetComponent<Renderer>().sortingOrder = layerOrder - 3;
    }

    public void UpdateStatusBars(Character c)
    {
        float newHealthScale = (float)c.currentHealth / (float)c.maxHealth;
        float newEnergyScale = (float)c.currentEnergy / (float)c.maxEnergy;

        healthBar.transform.SetXScale(newHealthScale);
        energyBar.transform.SetXScale(newEnergyScale);

        healthText.text = c.currentHealth + "";
        energyText.text = c.currentEnergy + "";

        if (newHealthScale == 1)
        {
            healthText.color = Color.white;
        }
        else
        {
            healthText.color = Color.red;
        }
    }

    public void ShowActiveMarker(bool onRightSide)
    {
        float xPosition;

        if (onRightSide)
        {
            xPosition = GetComponent<Renderer>().bounds.min.x;
        }
        else
        {
            xPosition = GetComponent<Renderer>().bounds.max.x;
        }

        activeMarker.transform.SetPosition2D(xPosition, GetComponent<Renderer>().bounds.center.y);
        activeMarker.enabled = true;
    }

    public void HideActiveMarker()
    {
        activeMarker.enabled = false;
    }

    public void ShowTargetMarker(bool onRightSide, Color color)
    {
        float xPosition;

        //if targeting self, put target marker on other side 
        if (activeMarker.enabled)
        {
            onRightSide = !onRightSide;
        }

        if (onRightSide)
        {
            xPosition = GetComponent<Renderer>().bounds.min.x;
        }
        else
        {
            xPosition = GetComponent<Renderer>().bounds.max.x;
        }

        targetMarker.color = color;

        targetMarker.transform.SetPosition2D(xPosition, GetComponent<Renderer>().bounds.center.y);
        targetMarker.enabled = true;
    }

    public void HideTargetMarker()
    {
        targetMarker.enabled = false;
    }

    public Vector2 GetTargetMarkerLocation(bool onRightSide)
    {
        float xPosition;

        //if targeting self, put target marker on other side 
        if (activeMarker.enabled)
        {
            onRightSide = !onRightSide;
        }

        if (onRightSide)
        {
            xPosition = GetComponent<Renderer>().bounds.min.x;
        }
        else
        {
            xPosition = GetComponent<Renderer>().bounds.max.x;
        }

        return new Vector2(xPosition, GetComponent<Renderer>().bounds.center.y);
    }

    public Vector2 GetActiveMarkerLocation(bool onRightSide)
    {
        float xPosition;

        if (onRightSide)
        {
            xPosition = GetComponent<Renderer>().bounds.min.x;
        }
        else
        {
            xPosition = GetComponent<Renderer>().bounds.max.x;
        }

        activeMarker.transform.SetPosition2D(xPosition, GetComponent<Renderer>().bounds.center.y);

        return new Vector2(xPosition, GetComponent<Renderer>().bounds.center.y);
    }

    /**********************
     * Status effect icons
     **********************/

    //Called when a status effect is added
    //If the effect has an icon then it is shown, otherwise nothing happens
    public void AddStatusIcon(EffectType effectType)
    {
        Sprite spriteToAdd = GUIController.instance.GetEffectSprite(effectType);

        if (spriteToAdd != null)
        {
            GameObject newObject = (GameObject)Instantiate(Resources.Load("StatusIcon"));
            newObject.GetComponent<StatusIcon>().myEffectType = effectType;
            newObject.transform.parent = transform.parent;

            SpriteRenderer newIcon = newObject.GetComponent<SpriteRenderer>();
            newIcon.sprite = spriteToAdd;

            statusEffectIcons.Add(newIcon);
            PositionStatusIcons();
        }
    }

    //called when a status effect (with icon) is removed
    public void RemoveStatusIcon(StatusEffect effect)
    {
        for (int i=0 ; i<statusEffectIcons.Count ; i++)
        {
            if (statusEffectIcons[i].GetComponent<StatusIcon>().myEffectType == effect.effectType)
            {
                Destroy(statusEffectIcons[i].gameObject);
                statusEffectIcons.RemoveAt(i);
                break;
            }
        }

        PositionStatusIcons();
    }

    //Called when icons are added or removed
    //uses the list to position the icons correctly
    void PositionStatusIcons()
    {
        float newX = 0;
        float newY = 0;

        if(statusEffectIcons.Count != 0)
        {
            newY = background.GetComponent<Renderer>().bounds.min.y - (statusEffectIcons[0].bounds.max.y - statusEffectIcons[0].transform.position.y);
        }

        for(int i=0 ; i<statusEffectIcons.Count ; i++)
        {
                         //left most position          //extent of the icon to line up the left edge    //amount to shift based on position in array
            newX = background.GetComponent<Renderer>().bounds.min.x + statusEffectIcons[i].GetComponent<Renderer>().bounds.extents.x + (i * statusEffectIcons[i].GetComponent<Renderer>().bounds.size.x);

            statusEffectIcons[i].transform.SetPosition2D(newX, newY);
        }
    }

}
