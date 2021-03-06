﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InstructionController : GameController
{
    Instruction[] instructions;
    List<int[]> scriptedMoves;
    int[] milestones;
    int miscIndex, instructionIndex, moveIndex, milestoneIndex;
    Instruction currentInstruction;
    Text infoText;
    Slider slider;

    private void Start()
    {
        DisableAllButtons();
        InitializeScriptedMoves();
        InitializeInstructions();

        infoText = GameObject.Find("Info Text").GetComponent<Text>();
        slider = GameObject.Find("Progress Slider").GetComponent<Slider>();

        instructionIndex = -1;
        miscIndex = 0;
        Next();
    }

    /// <summary>
    /// Advance into the next instruction
    /// </summary>
    public void Next()
    {
        if (instructionIndex >= instructions.Length - 1)
        {
            return; // cannot go next if there is no next instruction
        }

        instructionIndex++;
        slider.value = instructionIndex;

        currentInstruction = instructions[instructionIndex];
        currentInstruction.AdvanceIn();
        infoText.text = currentInstruction.Info;

        previewTimer = previewTime;
        confirmTimer = confirmTime;
    }

    public void Previous()
    {
        if (instructionIndex <= 0)
        {
            return; // cannot go previous if there is no previous instruction
        }

        instructionIndex--;
        slider.value = instructionIndex;

        currentInstruction = instructions[instructionIndex];
        currentInstruction.BackIn();
        infoText.text = currentInstruction.Info;

        previewTimer = previewTime;
        confirmTimer = confirmTime;
    }

    void DisableAllButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.enabled = false;
        }
    }

    void InitializeScriptedMoves()
    {
        scriptedMoves = new List<int[]>();
        // X plays 1 1 0 0 for first move
        scriptedMoves.Add(new int[] { 0, 1 });
        scriptedMoves.Add(new int[] { 0, 0 });
        scriptedMoves.Add(new int[] { 1, 1 });
        scriptedMoves.Add(new int[] { 1, 0 }); // X two in a row C
        scriptedMoves.Add(new int[] { 0, 0 });
        scriptedMoves.Add(new int[] { 2, 1 }); // X blocks TL
        scriptedMoves.Add(new int[] { 0, 0 });
        scriptedMoves.Add(new int[] { 1, 0 });
        scriptedMoves.Add(new int[] { 1, 0 }); // O two in a row ML
        scriptedMoves.Add(new int[] { 2, 0 }); // X blocks ML
        scriptedMoves.Add(new int[] { 0, 0 });
        scriptedMoves.Add(new int[] { 0, 2 });
        scriptedMoves.Add(new int[] { 0, 0 });
        scriptedMoves.Add(new int[] { 0, 0 }); // X two in a row TL
        scriptedMoves.Add(new int[] { 2, 0 }); // O blocks TL
        scriptedMoves.Add(new int[] { 2, 0 });
        scriptedMoves.Add(new int[] { 1, 2 });
        scriptedMoves.Add(new int[] { 0, 0 });
        scriptedMoves.Add(new int[] { 2, 2 });
        scriptedMoves.Add(new int[] { 0, 0 });
        scriptedMoves.Add(new int[] { 1, 2 }); // 20: O end TL in tie
        scriptedMoves.Add(new int[] { 1, 2 });
        scriptedMoves.Add(new int[] { 1, 1 });
        scriptedMoves.Add(new int[] { 2, 0 }); // 23: X win center
        scriptedMoves.Add(new int[] { 1, 1 }); // 24: O send X to completed board
        scriptedMoves.Add(new int[] { 1, 0, 1, 1 });
        scriptedMoves.Add(new int[] { 2, 0, 1, 0 });
        scriptedMoves.Add(new int[] { 0, 2 });
        scriptedMoves.Add(new int[] { 1, 0 });
        scriptedMoves.Add(new int[] { 1, 2, 1, 0 });
        scriptedMoves.Add(new int[] { 0, 2, 2, 0 });
        scriptedMoves.Add(new int[] { 1, 2, 2, 0 }); // 31: X wins
        moveIndex = 0;

        milestones = new int[] { 20, 23, 24, 31 };
        milestoneIndex = 0;
    }

    void InitializeInstructions()
    {
        instructions = new Instruction[11];

        instructions[0] = new Instruction(
            "Ultimate Tic-Tac-Toe is a game with 81 spots...",
            PreviewAll,
            delegate () { previewTime = 0.2f; },
            delegate () { previewTime = 0.2f; }
        );

        instructions[1] = new Instruction(
            "...on nine local games.",
            PreviewRelative,
            delegate () { previewTime = 0.5f; },
            delegate () { }
        );

        instructions[2] = new Instruction(
            "Playing a given spot sends the next player to the " +
            "relative local game, outlined in that player's color...",
            BlinkPreviewTopLeftSpots,
            delegate () { Game.Preview(null); },
            delegate () { }
            );

        instructions[3] = new Instruction(
            "For example, X playing in the top-left spot of " +
            "any local game sends O to the top-left local game.",
            BlinkPreviewTopLeftSpots,
            delegate () { },
            delegate () { Reset(); }
        );

        instructions[4] = new Instruction(
            "Now O must play in the top-left local game, " +
            "sending X to a new game",
            PreviewTopLeftSpots,
            delegate ()
            {
                Game.Preview(null);
                Game.Play(1, 1, 0, 0);
            },
            delegate ()
            {
                Reset();
                Game.Play(1, 1, 0, 0);
            }
        );

        instructions[5] = new Instruction(
            "Players take turns playing on any open spot.",
            PlayToMilestone,
            delegate () { Reset(); Game.Play(1, 1, 0, 0); },
            PreviousMilestone
        );

        instructions[6] = new Instruction(
            "Local games are completed with three in a row or a tie, " +
            "just like tic-tac-toe",
            PlayToMilestone,
            NextMilestone,
            PreviousMilestone
        );

        instructions[7] = new Instruction(
            "Trying to send a player to a completed board instead " +
            "opens all incomplete boards",
            PlayToMilestone,
            NextMilestone,
            PreviousMilestone
        );

        instructions[8] = new Instruction(
            "X can now play in any open spot on any incomplete board",
            PreviewAllOpen,
            NextMilestone,
            delegate ()
            {
                PreviousMilestone();
                NextMilestone();
            }
        );

        instructions[9] = new Instruction(
            "Play continues until one player wins three boards in a row " +
            "or all boards are completed",
            PlayToMilestone,
            delegate () { Game.Preview(null); },
            PreviousMilestone
        );

        instructions[10] = new Instruction(
            "X wins! Now try your own game!",
            delegate () { },
            NextMilestone,
            delegate () { }
        );
    }

    private void Update()
    {
        if (currentInstruction != null)
        {
            currentInstruction.Act();
        }
    }

    /// <summary>
    /// Preview a spot based on the current index
    /// Cycle through each spot in order, top to bottom, left to right
    /// </summary>
    void PreviewAll()
    {
        miscIndex %= 81;
        int boardRow = miscIndex / 27;
        int boardCol = (miscIndex / 9) % 3;
        int spotRow = (miscIndex / 3) % 3;
        int spotCol = miscIndex % 3;

        Game.Preview(boardRow, boardCol, spotRow, spotCol);

        IncrementIndex();
    }

    /// <summary>
    /// Cycle through previewing all open spots
    /// </summary>
    void PreviewAllOpen()
    {
        miscIndex %= Game.AvailableSpots.Count;
        Game.Preview(Game.AvailableSpots[miscIndex]);
        IncrementIndex();
    }

    /// <summary>
    /// Preview each relative spot of each board in order
    /// Top-left spot of top-left board, top-mid spot of top-mid board, etc.
    /// To be called as instruction action
    /// </summary>
    void PreviewRelative()
    {
        miscIndex %= 9;
        int row = miscIndex / 3;
        int col = miscIndex % 3;
        Game.Preview(row, col, row, col);

        IncrementIndex();
    }

    /// <summary>
    /// Preview and play a random spot, 
    /// just as though two random AIs were playing
    /// </summary>
    void PlayToMilestone()
    {
        // do not play past given milestone
        if (moveIndex > milestones[milestoneIndex]) { return; }

        if (Game.HasNextMove)
        {
            if (confirmTimer <= 0)
            {
                Game.Confirm();
                moveIndex++;
                confirmTimer += confirmTime;
            }
            else
            {
                confirmTimer -= Time.deltaTime;
            }
        }
        else
        {
            if (previewTimer <= 0)
            {
                int[] move = scriptedMoves[moveIndex];
                if (CanPlayActiveGame(Game))
                {
                    Location loc = Game.ActiveGame.Loc;
                    Game.Preview(loc.Row, loc.Col, move[0], move[1]);
                }
                else // multiple boards are open, move specifies which to play
                {
                    Game.Preview(move[0], move[1], move[2], move[3]);
                }

                previewTimer += previewTime;
            }
            else
            {
                previewTimer -= Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Whether the active game of the global game can be played
    /// False if active game is null or active game is complete
    /// True if active game exists and is not complete
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    bool CanPlayActiveGame(GlobalGame game)
    {
        return game.ActiveGame != null
            && !game.ActiveGame.GameOver();
    }

    /// <summary>
    /// Instantly advance game state to next milestone and increment milestone index
    /// </summary>
    void NextMilestone()
    {
        if (milestoneIndex < 0)
        {
            milestoneIndex = 0; // reset game and do nothing more
            return;
        }
        else if (milestoneIndex >= milestones.Length)
        {
            return;
        }

        Game.Preview(null);

        // play through next milestone
        while (moveIndex <= milestones[milestoneIndex])
        {
            int[] move = scriptedMoves[moveIndex];
            if (CanPlayActiveGame(Game))
            {
                Location loc = Game.ActiveGame.Loc;
                Game.Play(loc.Row, loc.Col, move[0], move[1]);
            }
            else // multiple boards are open, move specifies which to play
            {
                Game.Play(move[0], move[1], move[2], move[3]);
            }
            moveIndex++;
        }

        // only increment if milestone wasn't reached before
        milestoneIndex++;
    }

    public override void Reset()
    {
        moveIndex = 0;
        base.Reset();
    }

    void PreviousMilestone()
    {
        Debug.Log(milestoneIndex + "before");
        milestoneIndex -= 2; // go back twice
        Reset(); // cannot undo repeatedly
        Game.Play(1, 1, 0, 0); // first move

        NextMilestone();
        Debug.Log(milestoneIndex + "after");
    }

    /// <summary>
    /// Preview top left spot of each board
    /// </summary>
    void BlinkPreviewTopLeftSpots()
    {
        miscIndex %= 18;

        if (miscIndex % 2 == 1)
        {
            Game.Preview(null); // preview nothing to show disappearing outline
        }
        else
        {
            int row = miscIndex / 6;
            int col = (miscIndex / 2) % 3;
            Game.Preview(row, col, 0, 0); // preview top-left of each board
        }

        IncrementIndex();
    }

    void PreviewTopLeftSpots()
    {
        miscIndex %= 9;
        int row = miscIndex / 3;
        int col = miscIndex % 3;
        Game.Preview(0, 0, row, col);

        IncrementIndex();
    }

    void IncrementIndex()
    {
        if (previewTimer <= 0)
        {
            miscIndex++;
            previewTimer += previewTime;
        }
        else
        {
            previewTimer -= Time.deltaTime;
        }
    }
}
