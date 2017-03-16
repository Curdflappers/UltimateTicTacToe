﻿using UnityEngine;
using UnityEngine.UI;

public class BoardSpot : Button {
    /// <summary>
    /// Whether this spot has been clicked
    /// </summary>
    private bool clicked;

    public bool Clicked { get { return clicked; } set { clicked = value; } }

    protected override void Start()
    {
        Clicked = false;
    }

    public void OnClick()
    {
        transform.parent.parent.GetComponent<Game>().FillSpot(gameObject);
    }

    public void Clear()
    {
        Clicked = false;
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/empty");
        GetComponent<Image>().color = Game.enabledColor;

        ColorBlock cb = GetComponent<Button>().colors;
        cb.disabledColor = Game.enabledColor;
        cb.highlightedColor = Game.FirstTurn ? Game.p1Color : Game.p2Color;
        GetComponent<Button>().colors = cb;
    }
}
