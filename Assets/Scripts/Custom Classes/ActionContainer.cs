using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ActionContainer : MonoBehaviour {

    public string containerName;
    public Sprite containerImage;
    public Action[] actions = new Action[3];

}
