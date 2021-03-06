﻿using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to the status text game object
/// </summary>
public class StatusText : MonoBehaviour
{
    GlobalGame game;

    public GlobalGame Game
    {
        get
        { return game; }
        set
        {
            if (game != null)
            {
                game.WinnerChanged -= HandleGameStateChanged;
                game.TurnChanged -= HandleGameStateChanged;
            }

            game = value;

            if (game != null)
            {
                game.WinnerChanged += HandleGameStateChanged;
                game.TurnChanged += HandleGameStateChanged;
            }
            UpdateState();
        }
    }

    void UpdateState()
    {
        HandleGameStateChanged(game, null);
    }

    private void Awake()
    {
        UpdateState();
    }

    public void HandleGameStateChanged(object o, GameEventArgs e)
    {
        Text text = GetComponent<Text>();
        if (game == null)
        {
            text.text = "";
            return;
        }

        if (game.GameOver())
        {
            if (game.Winner != null)
            {
                text.text = game.Winner.Name + " wins!";
                return;
            }
            text.text = "Tie game";
            return;
        }

        text.text = game.ActivePlayer().Name + "'s turn";
    }
}
