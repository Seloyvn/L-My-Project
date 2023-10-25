using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChampSelect : MonoBehaviour
{
    public int id;
    public TMP_Dropdown Dropdown;
    public void SelectChamp()
    {
        GameManager.instance.CmdSelectChampion(Dropdown.value, id);
    }
}
