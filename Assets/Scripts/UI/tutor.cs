using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tutor : MonoBehaviour
{
    public List<tutorMessages> tutorMessagesList = new();

    public TextMeshProUGUI tutorText;
    private int currentMessageIndex = 0;

    private void Start()
    {
        ShowCurrentMessage();
    }
    private void Update()
    {
        if (currentMessageIndex < tutorMessagesList.Count)
        {
            if (tutorMessagesList[currentMessageIndex].IsAnyKeyPressed())
            {
                currentMessageIndex++;
                ShowCurrentMessage();
            }
        }
        else
        {
            tutorText.gameObject.SetActive(false);
        }
    }
    private void ShowCurrentMessage()
    {
        if (currentMessageIndex < tutorMessagesList.Count)
        {
            tutorText.text = tutorMessagesList[currentMessageIndex].TutorMessage;
        }
    }
}