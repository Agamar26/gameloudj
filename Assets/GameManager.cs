using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<elevatorScript> elevators = new List<elevatorScript>();
    public bool isbegin;
    public bool canAlwaysBegin;
    public bool haveBeginned;
    public int timeForBegin;
    public TextMeshProUGUI textBeginUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        verifyelevators();
    }

    public void verifyelevators()
    {
        if(haveBeginned) {return;}
        if (!isbegin)
        {
            foreach (var item in elevators)
            {
                if (!item.isSoloInElevator)
                {
                    return;
                }
            }
            print("elevators are ok");
            StartCoroutine(BeginCounter(timeForBegin));
            isbegin = true;
        }
        if (canAlwaysBegin)
        {
            foreach (var item in elevators)
            {
                if (!item.isSoloInElevator)
                {
                    isbegin = false;
                    canAlwaysBegin = false;
                    StopAllCoroutines();
                }
            }
        }

    }
    public IEnumerator BeginCounter(int ee)
    {
       
        int zeez = 3;
        canAlwaysBegin = true;

        while (zeez > 0)
        {
            textBeginUI.text = zeez.ToString();
            yield return new WaitForSeconds(1);
            zeez--;
        }

        canAlwaysBegin = false;
        haveBeginned = true;
    } 
}
