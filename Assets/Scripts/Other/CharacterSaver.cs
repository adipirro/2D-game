using UnityEngine;
using System.Collections;

public static class CharacterSaver {

    public static void SavePlayerCharacter(Player playerCharacter)
    {
        PlayerPrefs.SetFloat(playerCharacter.savePrefix + "_health", playerCharacter.maxHealth);
        PlayerPrefs.Save();
    }

    public static string ReadPlayerCharacter(Player playerCharacter)
    {
        return playerCharacter.savePrefix + "_health"+PlayerPrefs.GetFloat(playerCharacter.savePrefix + "_health", -1f);
    }

}
