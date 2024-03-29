using Photon.Pun;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private GameObject[] buttons;
    [SerializeField] public GameObject scrollbar;
    [SerializeField] public TMP_Text timer;

    public static GameUI Instance { get; set; }

    private void Awake()
    {
        Instance = this;

        Application.targetFrameRate = 60;
    }


    public void OnLocalGameButton()
    {
        menuAnimator.SetTrigger("InGameMenu");
    }

    public void OnOnlineGameButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnOnlineHostButton()
    {
        menuAnimator.SetTrigger("HostMenu");
    }

    public void OnOnlineBackButton()
    {
        menuAnimator.SetTrigger("StartMenu");
    }

    public void OnHostBackButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void ChangePieceNumber(int pieceIndex, int Value)
    {
        int currentText = int.Parse(buttons[pieceIndex].GetComponentInChildren<TMP_Text>().text);
        int newText = currentText + Value;
        buttons[pieceIndex].GetComponentInChildren<TMP_Text>().text = newText.ToString();
    }


    public void ChangeTextColor(int pieceIndex, Color newColor)
    {
        buttons[pieceIndex].GetComponentInChildren<TMP_Text>().color = newColor;
    }
}
