using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputInterface : MonoBehaviour
{
    public HashSet<KeyCode> currentKeysPressed = new HashSet<KeyCode>();
    public HashSet<KeyCode> prevKeysPressed = new HashSet<KeyCode>();

    public HashSet<string> currentButtonPressed = new HashSet<string>();
    public HashSet<string> prevButtonPressed = new HashSet<string>();

    public Dictionary<string, float> currentAxis_Values = new Dictionary<string, float>();
    public Dictionary<string, float> prevAxis_Values = new Dictionary<string, float>();

    public int playerNumber;
    public bool isAiControlled = false;
    public void Start()
    {
        currentAxis_Values.Add("Horizontal" + playerNumber, 0);
        currentAxis_Values.Add("Vertical" + playerNumber, 0);

        prevAxis_Values.Add("Horizontal" + playerNumber, 0);
        prevAxis_Values.Add("Vertical" + playerNumber, 0);


    }

    public void Clear()
    {
        currentKeysPressed.Clear();
        currentButtonPressed.Clear();
    }

    public bool AddKeyPressed(KeyCode key)
    {
        return currentKeysPressed.Add(key);
    }

    public bool AddButtonPressed(string buttonString)
    {
        return currentButtonPressed.Add(buttonString);
    }


    /*
    public void LateUpdate()
    {
        print("Ich bin LAte update ich sollte ganz unten stehen");
        prevKeysPressed = currentKeysPressed;
        prevButtonPressed = currentButtonPressed;
        prevAxis_Values = currentAxis_Values;
    }*/

    //Diesen fehler hier zu fixen hat mich bestimmt 6 stunden gedauert da die davorige weise falsch war den Inhalt von previes mit dem von current zu füllen durch prevKeysPressed = currentKeysPressed; weil das natürlich eine Referenz auf das Object setz.
    public IEnumerator setPrevs()
     {
         yield return new WaitForEndOfFrame();
        prevKeysPressed = new HashSet<KeyCode>(currentKeysPressed);

        /*
        print("Bin in PREV Methoden diese Sollte ganz unten stehen       ");
        print("");
        print("");
        foreach (var button in currentButtonPressed)
        {
            print("currentButtonPressed: " + button);
        }

        print("priviusButtonPressed: ");
        foreach (var button in prevButtonPressed)
        {
            print("prevButtonPressed: " + button);
        }
        */
        prevButtonPressed = new HashSet<string>(currentButtonPressed);
        prevAxis_Values = new Dictionary<string, float>(currentAxis_Values);
    }


    //Key part////////////////////////////////////////////////////

    //ich brauche keine Key part in meiner game dieses da ich es aber auch gut in der zukunft brauchen kann habe ich es drin gelassen
    public bool GetKey(KeyCode key)
    {
        if (!isAiControlled)
        {
            return Input.GetKey(key);
        } else if (isAiControlled)
        {
            return currentKeysPressed.Contains(key);
        } else
        {
            return false;
        }
    }

    public bool GetKeyDown(KeyCode key)
    {
        if (!isAiControlled)
        {
            return Input.GetKeyDown(key);
        }
        else if (isAiControlled)
        {
            //Bei der zweiten abfrage muss evt prev stat current stehen
            if (currentKeysPressed.Contains(key) && !prevKeysPressed.Contains(key))
            {
                return true;
            } else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public bool GetKeyUp(KeyCode key)
    {
        if (!isAiControlled)
        {
            return Input.GetKeyUp(key);
        }
        else if (isAiControlled)
        {
            //Bei der zweiten abfrage muss evt prev stat current stehen
            if (!currentKeysPressed.Contains(key) && prevKeysPressed.Contains(key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    //Key part ende//////////////////////////////////////////////////////////////


    //Button Part Funktioniert nicht//////////////////////////////////////////////////////////////////
    
    public bool GetButton(string buttonString)
    {
        if (!isAiControlled)
        {
            return Input.GetButton(buttonString);
        }
        else if (isAiControlled)
        {

            //print("Gehe ich in ButtonUp rein ?");
            //print("ButtonString in GetButton " + buttonString);
            //print("GetButton = " + currentButtonPressed.Contains(buttonString));
            return currentButtonPressed.Contains(buttonString);
        }
        else
        {
            return false;
        }
    }

    public bool GetButtonDown(string buttonString)
    {
        if (!isAiControlled)
        {
            return Input.GetButtonDown(buttonString);

            
        }
        else if (isAiControlled)
        {
            //Bei der zweiten abfrage muss evt prev stat current stehen
            //print("Butten get Down");
            
            /*
            foreach (var button in currentButtonPressed)
            {
                print("currentButtonPressed: " + button);
            }

            foreach (var button in prevButtonPressed)
            {
                print("prevButtonPressed: " + button);
            }*/
            
            //print("GetButtonDown = " + (currentButtonPressed.Contains(buttonString) && !prevButtonPressed.Contains(buttonString)));
            if (currentButtonPressed.Contains(buttonString) && !prevButtonPressed.Contains(buttonString))
            {
                //print("Bin in true");
                return true;
            }
            else
            {
                //print("Bin in false");
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public bool GetButtonUp(string buttonString)
    {
        if (!isAiControlled)
        {
            return Input.GetButtonUp(buttonString);
        }
        else if (isAiControlled)
        {
            //Bei der zweiten abfrage muss evt prev stat current stehen
            //print("Get Button up = " + (!currentButtonPressed.Contains(buttonString) && prevButtonPressed.Contains(buttonString)));
            if (!currentButtonPressed.Contains(buttonString) && prevButtonPressed.Contains(buttonString))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    //Button Part Ende//////////////////////////////////////////////////////////////////
    


    //Axis Part////////////////////////////////////////////////////////////////////
    public float GetAxis(string axis)
    {

        if (!isAiControlled)
        {
            return Input.GetAxis(axis);
        }
        else if (isAiControlled)
        {
            //print(axis);
            return currentAxis_Values[axis];
        }
        else
        {
            return currentAxis_Values[axis];
        }
    }
}
