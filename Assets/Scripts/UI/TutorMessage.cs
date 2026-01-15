using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class tutorMessages
{
    public String TutorMessage;
    public List<KeyCode> TutorMessageKeys;
    public bool IsAnyKeyPressed()
    {
        foreach (KeyCode key in TutorMessageKeys)
        {
            if (Input.GetKeyDown(key))
                return true;
        }
        return false;
    }
}