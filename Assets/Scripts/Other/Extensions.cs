using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Extensions
{
    //Used for setting 2d vectors
    public static void SetPosition2D(this Transform t, float newX, float newY)
    {
        t.position = new Vector3(newX, newY, 0);
    }

    public static void SetScale2D(this Transform t, float newX, float newY)
    {
        t.localScale = new Vector3(newX, newY, 0);
    }

    public static void SetXScale(this Transform t, float newX)
    {
        t.localScale = new Vector3(newX, t.localScale.y, t.localScale.z);
    }

    public static void Flip(this Transform t)
    {
        t.SetXScale(-t.localScale.x);
    }

    //Searches through a character list for a character with matching tag
    public static bool FindWithTag(this List<Character> list, string tag)
    {
        foreach (Character c in list)
        {
            if (c.gameObject.tag.Equals(tag))
            {
                return true;
            }
        }

        return false;
    }

    public static GameObject FindInChildren(this GameObject gameObject, string name)
    {
        Transform[] children = gameObject.transform.GetComponentsInChildren<Transform>();

        foreach (Transform t in children)
        {
            if (t.gameObject.name.Equals(name))
            {
                return t.gameObject;
            }
        }

        //Not found
        return null;
    }

    public static int IndexOfCharacter(this List<StatWidget> list, Character c)
    {
        for(int i=0 ; i<list.Count ; i++)
        {
            if(list[i].myCharacter.Equals(c))
            {
                return i;
            }
        }

        return -1;
    }
}