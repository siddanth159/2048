using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public bool merged = false;
    public int Number
    {
        get { return number; }
        set
        {
            number = value;
            if (number == 0) SetEmpty();
            else
            {
                ApplyStyle(number);
                SetVisible();
            }
        }
    }
    private int number;
    public int rowIndex;
    public int colIndex;

    private Text TileText;
    private Image TileImage;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        TileText = GetComponentInChildren<Text>();
        TileImage = transform.Find("TileImage").GetComponent<Image>();
    }

    public void MergedAnimation()
    {
        anim.SetTrigger("Merge");
    }

    public void AppearAnimation()
    {
        anim.SetTrigger("Appear");
    }

    void ApplyStyleFromHolder(int index)
    {
        TileText.text = TileStyleHolder.tileStyleHolder.tileStyles[index].Number.ToString();
        TileText.color = TileStyleHolder.tileStyleHolder.tileStyles[index].TextColor;
        TileImage.color = TileStyleHolder.tileStyleHolder.tileStyles[index].TileColor;
    }

    void ApplyStyle(int number)
    {
        ApplyStyleFromHolder(Array.FindIndex(TileStyleHolder.tileStyleHolder.tileStyles, p => p.Number == number));
    }

    void SetVisible()
    {
        TileImage.enabled = true;
        TileText.enabled = true;
    }

    void SetEmpty()
    {
        TileImage.enabled = false;
        TileText.enabled = false;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
