using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColor : MonoBehaviour
{

    public Dropdown dropdown_size;
    public Dropdown dropdown_color;

    public int check;

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetInt("Size", 0);
        PlayerPrefs.SetInt("Color", 0);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void changeValues()
    {
        PlayerPrefs.SetInt("Size", dropdown_size.value);
        PlayerPrefs.SetInt("Color", dropdown_color.value);
        check = dropdown_color.value;
    }
}
