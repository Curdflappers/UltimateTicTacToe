﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InstructionController : GameController
{
    Instruction[] instructions;
    int instructionIndex;
    Instruction currentInstruction;
    Text infoText;
    int index; // for miscellaneous cycles
    Slider slider;

    private void Start()
    {
        DisableAllButtons();
        InitializeInstructions();

        infoText = GameObject.Find("Info Text").GetComponent<Text>();

        instructionIndex = -1;
        slider = GameObject.Find("Progress Slider").GetComponent<Slider>();
        Next();
    }

    /// <summary>
    /// Advance into the next instruction
    /// </summary>
    public void Next()
    {
        if (instructionIndex == instructions.Length - 1)
        {
            return; // cannot go next if there is no next instruction
        }

        if (currentInstruction != null)
        {
            currentInstruction.AdvanceOut();
        }
        instructionIndex++;
        slider.value = instructionIndex;

        currentInstruction = instructions[instructionIndex];
        currentInstruction.AdvanceIn();
        infoText.text = currentInstruction.Info;

        index = 0;
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

    void InitializeInstructions()
    {
        instructions = new Instruction[12];

        instructions[0] = new Instruction(
            "Ultimate Tic-Tac-Toe is a game with 81 spots...",
            PreviewAll,
            delegate () { },
            delegate () { },
            delegate () { previewTime = 0.111f; },
            delegate () { }
        );

        instructions[1] = new Instruction(
            "...on nine local games.",
            PreviewRelative,
            delegate () { previewTime = 0.333f; },
            delegate () { },
            delegate () { },
            delegate () { }
        );

        instructions[2] = new Instruction(
            "Players take turns playing on any open spot in the global game",
            PlayRandom,
            delegate () { Game.Preview(null); },
            delegate () { },
            delegate () { },
            delegate () { }
        );
    }

    private void Update()
    {
        currentInstruction.Act();
    }

    /// <summary>
    /// Preview a spot based on the current index
    /// Cycle through each spot in order, top to bottom, left to right
    /// </summary>
    void PreviewAll()
    {
        index %= 81;
        int boardRow = index / 27;
        int boardCol = (index / 9) % 3;
        int spotRow = (index / 3) % 3;
        int spotCol = index % 3;

        Game.Preview(boardRow, boardCol, spotRow, spotCol);

        if (previewTimer <= 0)
        {
            index++;
            previewTimer += previewTime;
        }
        else
        {
            previewTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Preview each relative spot of each board in order
    /// Top-left spot of top-left board, top-mid spot of top-mid board, etc.
    /// To be called as instruction action
    /// </summary>
    void PreviewRelative()
    {
        index %= 9;
        int row, col;
        row = index / 3;
        col = index % 3;
        Game.Preview(row, col, row, col);

        if (previewTimer <= 0)
        {
            index++;
            previewTimer += previewTime;
        }
        else
        {
            previewTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Preview and play a random spot, 
    /// just as though two random AIs were playing
    /// </summary>
    void PlayRandom()
    {
        if (Game.HasNextMove)
        {
            if (confirmTimer <= 0)
            {
                Game.Confirm();
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
                // preview random spot
                List<Spot> spots = Game.AvailableSpots;
                if (spots.Count > 0)
                {
                    Game.Preview(spots[Random.Range(0, spots.Count)]);
                }
                else
                {
                    previewTimer = float.MaxValue; // no more checks
                }

                previewTimer += previewTime;
            }
            else
            {
                previewTimer -= Time.deltaTime;
            }
        }
    }
}
