﻿using UnityEngine;
using UnityEngine.UI;

public class Board : Spot
{
    int[,] spots;
    bool active;
    /// <summary>
    /// The 8 combos that win the game
    /// </summary>
    static Location[,] winLines;
    /// <summary>
    /// Difference between board and pieces when enabled
    /// </summary>
    static Color offset = Color.gray * 1.2f; // very light offset

    /// <summary>
    /// Change in board when enabled/disabled
    /// </summary>
    static Color enabledOffset = Color.gray / 2; // darker offset

    Game game;

    public bool Active
    {
        get { return active; }
        set
        {
            if (active != value && !IsFull) // changing active status
            {
                Color color = GetComponent<Image>().color;

                if (GameOver) // local game over: simply augment color by an offset
                {
                    if (value) { color += enabledOffset; } // enabling makes lighter
                    else { color -= enabledOffset; } // disabling makes darker
                }
                else // game not over
                {
                    color = value ? game.EnabledColor : game.DisabledColor;
                }

                GetComponent<Image>().color = color;
                active = value;
            }
        }
    }

    /// <summary>
    /// The possible ownerships of spots and winner of the game
    /// </summary>
    public const int EMPTY = 0;

    /// <summary>
    /// The spots on this board
    /// </summary>
    public int[,] Spots { get { return spots; } }

    public Spot[] BoardSpots
    {
        get
        {
            return transform.GetComponentsInChildren<Spot>();
        }
    }

    /// <summary>
    /// Whether this game is over
    /// </summary>
    public bool GameOver { get { return IsFull || Owner != null; } }

    /// <summary>
    /// Returns whether this board is full
    /// </summary>
    public bool IsFull
    {
        get
        {
            foreach (int spot in spots)
            {
                if (spot == 0) { return false; }
            }
            return true;
        }
    }
    
    /// <summary>
    /// The outline image child of this board
    /// </summary>
    public Image Outline {
        get {
            return transform.GetChild(0).GetComponent<Image>();
        }
    }

    // Use this for initialization
    void Start()
    {
        spots = new int[3, 3];
        PopulateWinLines();
        active = true;
        game = GameObject.Find("Global Board").GetComponent<Game>();
    }

    public void Reset()
    {
        Start();
        GetComponent<Image>().color = Color.white;
    }

    /// <summary>
    /// Overrides the value at spots[loc.Row, loc.Col] with player
    /// </summary>
    /// <param name="loc"></param>
    /// <param name="player"></param>
    void FillSpot(Location loc, int player)
    {
        bool gameOverState = GameOver;
        spots[loc.Row, loc.Col] = player;

        if ((!GameOver && player != EMPTY) // game may have just ended
            || (GameOver && player == EMPTY)) // or game could have been re-opened
        {
            CheckWinner(); // update game over status
        }

        if (gameOverState != GameOver) // game over state changed
        {
            UpdateColor();
            Board parentBoard = transform.parent.GetComponent<Board>();
            if (parentBoard)
            {
                parentBoard.FillSpot(Loc, Owner == null ? EMPTY : Owner.Turn);
            }
        }
    }

    /// <summary>
    /// Update the color of the board to reflect the winner of the game
    /// </summary>
    internal void UpdateColor()
    {
        Image image = GetComponent<Image>();
        if(Owner != null)
        {
            image.color = Owner.Color + offset;
            if(active) { image.color += enabledOffset; }
        }
        else if(IsFull)
        { image.color = game.DisabledColor; } // tie game
        else
        {
            image.color = active ? game.EnabledColor : game.DisabledColor;
        }
    }

    public void FillSpot(string name, int player)
    {
        FillSpot(Spot.StringToLoc(name), player);
    }

    int Get(Location loc)
    {
        return spots[loc.Row, loc.Col];
    }

    /// <summary>
    /// Updates winner if someone has won the game
    /// </summary>
    void CheckWinner()
    {
        bool emptySpotExists = false;
        bool firstSpot = false;

        for (int line = 0; line < 8; line++)
        {
            int p = 0; // the player that has a chance of winning on this line
            firstSpot = true;
            for (int spot = 0; spot < 3; spot++)
            {
                int player = Get(winLines[line, spot]);

                if (player == EMPTY)
                {
                    emptySpotExists = true;
                    break;
                }

                if (firstSpot)
                {
                    p = player;
                    firstSpot = false;
                }
                else if (p != player) { break; }

                if (spot == 2) // p has matched player all 3 spots
                {
                    owner = player == 1 ? game.P1 : game.P2;
                    return;
                }
            }
        }
        if (emptySpotExists) { owner = null; }
    }

    internal void PopulateWinLines()
    {
        if (winLines != null) { return; }
        winLines = new Location[8, 3]; // 8 lines of length 3

        // horizontal and vertical lines
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                winLines[i, j] = new Location(i, j);
                winLines[i + 3, j] = new Location(j, i);
            }
        }

        // diagonals
        for (int i = 0; i < 3; i++)
        {
            winLines[6, i] = new Location(i, i); // top-right to bottom-left
            winLines[7, i] = new Location(2 - i, i); // bottom-left to top-right
        }
    }
}
