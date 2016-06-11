using UnityEngine;
using System.Collections;

[System.Serializable]
public class StatWidget {

    public GameObject gameObject;
    public Character myCharacter;

    SpriteRenderer portrait;

    GameObject healthBar;
    GameObject energyBar;

    TextMesh healthTxt;
    TextMesh energyTxt;

    bool flipped;

    public StatWidget()
    {
        gameObject = null;
        portrait = null;
        healthBar = null;
        energyBar = null;
        healthTxt = null;
        energyTxt = null;
    }

    public StatWidget(GameObject gameObject, Character c, float scale, bool flipped)
    {
        this.gameObject = gameObject;
        this.gameObject.transform.SetScale2D(scale, scale);

        myCharacter = c;

        portrait = gameObject.FindInChildren("Portrait").GetComponent<SpriteRenderer>();

        healthBar = gameObject.FindInChildren("HealthBar");
        energyBar = gameObject.FindInChildren("EnergyBar");

        healthTxt = gameObject.FindInChildren("HealthText").GetComponent<TextMesh>();
        energyTxt = gameObject.FindInChildren("EnergyText").GetComponent<TextMesh>();

        healthTxt.GetComponent<Renderer>().sortingLayerID = gameObject.GetComponent<Renderer>().sortingLayerID;
        healthTxt.GetComponent<Renderer>().sortingOrder = gameObject.GetComponent<Renderer>().sortingOrder;

        energyTxt.GetComponent<Renderer>().sortingLayerID = gameObject.GetComponent<Renderer>().sortingLayerID;
        energyTxt.GetComponent<Renderer>().sortingOrder = gameObject.GetComponent<Renderer>().sortingOrder;

        this.flipped = flipped;

        if (flipped)
        {
            //flip everything around
            this.gameObject.transform.Flip();

            //Portraits are already facing the correct way
            portrait.transform.Flip();

            //Flip the text so it faces the right direction
            healthTxt.anchor = TextAnchor.MiddleLeft;
            energyTxt.anchor = TextAnchor.MiddleLeft;

            healthTxt.alignment = TextAlignment.Left;
            energyTxt.alignment = TextAlignment.Left;

            healthTxt.transform.Flip();
            energyTxt.transform.Flip();
        }

        portrait.sprite = c.portrait;

        healthBar.transform.SetXScale((float)c.currentHealth / (float)c.maxHealth);
        healthTxt.text = c.currentHealth + "/" + c.maxHealth;
    }

    public void AlignTo(Vector2 alignPosition)
    {
        float newX, newY;

        if (flipped)
        {
            newX = alignPosition.x + (gameObject.transform.position.x - gameObject.GetComponent<Renderer>().bounds.min.x);
        }
        else
        {
            newX = alignPosition.x - (gameObject.GetComponent<Renderer>().bounds.max.x - gameObject.transform.position.x);
        }

        newY = alignPosition.y - (gameObject.GetComponent<Renderer>().bounds.max.y - gameObject.transform.position.y);

        gameObject.transform.SetPosition2D(newX, newY);
    }

    public Vector2 GetAlignmentPosition()
    {
        if (flipped)
        {
            return new Vector2(gameObject.GetComponent<Renderer>().bounds.min.x, gameObject.GetComponent<Renderer>().bounds.min.y);
        }
        else
        {
            return new Vector2(gameObject.GetComponent<Renderer>().bounds.max.x, gameObject.GetComponent<Renderer>().bounds.min.y);
        }
    }

    public void UpdateStats()
    {
        healthBar.transform.SetXScale((float)myCharacter.currentHealth / (float)myCharacter.maxHealth);
        healthTxt.text = myCharacter.currentHealth + "/" + myCharacter.maxHealth;

        energyBar.transform.SetXScale((float)myCharacter.currentEnergy / (float)myCharacter.maxEnergy);
        energyTxt.text = myCharacter.currentEnergy + "/" + myCharacter.maxEnergy;
    }

    //I may want to come back to this
    //Swaps the character and portrait of two widgets
    //public static void Swap(StatWidget a, StatWidget b)
    //{
    //    Character tempC ;
    //    Sprite tempS = new Sprite();

    //    tempC = a.myCharacter;
    //    tempS = a.portrait.sprite;

    //    a.portrait.sprite = b.portrait.sprite;
    //    a.myCharacter = b.myCharacter;
    //    a.UpdateStats();

    //    b.portrait.sprite = tempS;
    //    b.myCharacter = tempC;
    //    b.UpdateStats();
    //}

}
