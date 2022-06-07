using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformDataNode : MonoBehaviour
{
    public int i, j;
    public Color nextColor;
    public Shades shade = Shades.Gray;
    public float nextPosition = 0.0f, height;
    public bool Program, Selected, Simulate, Simulation = false;

    public Text textSelectedNodeName, textSelectedNodePosition;
    public Image imgSelectedNodeColor, imgSelectedProgramNodeColor;
    public Slider sliderSelectedProgramNodeHeight;

    public delegate void UpdatePlatformDataNodeUI(PlatformDataNode pdn);
    public static event UpdatePlatformDataNodeUI OnUpdatePlatformDataNodeUI;

    public enum Shades
    {
        Red,
        Green,
        Blue,
        Gray,
        RGB
    }

    private void OnDisable()
    {
        UIManager.OnNodeProgramChanged -= UIManager_OnNodeProgramChanged;
        PlatformManager.OnToggleSimulation -= PlatformManager_OnToggleSimulation;
    }

    private void OnEnable()
    {
        UIManager.OnNodeProgramChanged += UIManager_OnNodeProgramChanged;
        PlatformManager.OnToggleSimulation += PlatformManager_OnToggleSimulation;
    }

    void Start()
    {
        nextPosition = transform.position.y;
        ResetDataNode();
    }

    //if there is a change in the UI then update the next position
    private void UIManager_OnNodeProgramChanged(Slider s)
    {
        if (Program)
        {
            if (Selected)
            {
                transform.position = new Vector3(transform.position.x, s.value, transform.position.z);    
                nextPosition = s.value;
            }
        }
    }

    private void UIManagerWithEvents_OnToggleProgram(Toggle t)
    {
        Program = !Program;
    }

    
    private void PlatformManager_OnToggleSimulation()
    {
        //if its simulating then stop, if not then start and stop the programming phase
        if(Simulation == false)
        {
            //depending on the shade, we will get a random color on that spectrum
            switch (shade)
            {
                case Shades.Gray:
                    nextColor = ReturnColor(Color.gray);
                    break;
                case Shades.Red:
                    nextColor = ReturnColor(Color.red);
                    break;
                case Shades.Green:
                    nextColor = ReturnColor(Color.green);
                    break;
                case Shades.Blue:
                    nextColor = ReturnColor(Color.blue);
                    break;
                case Shades.RGB:
                    nextColor = ReturnColor(Color.black);
                    break;
            }
            //move them to the start, make them white and then begin
            //transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            transform.gameObject.GetComponent<Renderer>().material.color = Color.white;

            //Program = false;
            //Simulate = true;
            Selected = Simulate = Program = false;
            Simulation = true;
        }
        else
        {
            Simulation = false;
            //Simulate = Program = true;
        }
        //Debug.Log("Node: ["+i+"]["+j+"] Next Position: "+nextPosition+" Height: "+height+" Shade: "+shade);
    }

    // Update is called once per frame
    void Update()
    {
        if (Program)
        {    
            if (Selected)
            {
                transform.gameObject.GetComponent<Renderer>().material.color = Color.blue;
                height = transform.position.y;
                UpdateUI();
            }
            return;
        }
        if (!Program)
        {
            if (Simulate)
            {
                {
                    //lerp to next color
                    transform.gameObject.GetComponent<Renderer>().material.color = Color.Lerp(transform.gameObject.GetComponent<Renderer>().material.color, nextColor, Time.deltaTime);

                    //lerp to next position
                    transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, nextPosition, transform.position.z), Time.deltaTime);             
                    if (NearlyEquals(transform.position.y, nextPosition))
                    {
                        Simulate = false;
                        nextPosition = 0;
                        nextColor = Color.white;
                    }
                }
            }
            else{}
        }
        
        //if its simulating to a height of not 0 then move it to that position
        if(Simulation && height !=0)
        {
            //lerp to next color
            transform.gameObject.GetComponent<Renderer>().material.color = Color.Lerp(transform.gameObject.GetComponent<Renderer>().material.color, nextColor, Time.deltaTime);

            //lerp to next position
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, nextPosition, transform.position.z), Time.deltaTime);

            //if were getting close to destination which is 0 then we need to simulate back up to the height
            if (NearlyEquals(transform.position.y, nextPosition) && nextPosition == 0)
            {
                //lerp to next color
                transform.gameObject.GetComponent<Renderer>().material.color = Color.Lerp(transform.gameObject.GetComponent<Renderer>().material.color, nextColor, Time.deltaTime);
                Simulation = false;
            }
            else if(NearlyEquals(transform.position.y, nextPosition) && nextPosition != 0)
            {
                nextPosition = 0;
                //change it up and get a new color on same spectrum
                switch (shade)
                {
                    case Shades.Gray:
                        nextColor = ReturnColor(Color.gray);
                        break;
                    case Shades.Red:
                        nextColor = ReturnColor(Color.red);
                        break;
                    case Shades.Green:
                        nextColor = ReturnColor(Color.green);
                        break;
                    case Shades.Blue:
                        nextColor = ReturnColor(Color.blue);
                        break;
                    case Shades.RGB:
                        nextColor = ReturnColor(Color.black);
                        break;
                }
                //lerp to next color
                transform.gameObject.GetComponent<Renderer>().material.color = Color.Lerp(transform.gameObject.GetComponent<Renderer>().material.color, nextColor, Time.deltaTime);
            }
            else
            {

            }
        }
        else if (Simulation && height == 0)
        {
            Simulation = false;
            transform.gameObject.GetComponent<Renderer>().material.color = Color.Lerp(transform.gameObject.GetComponent<Renderer>().material.color, Color.white, Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, 0.0f, transform.position.z), Time.deltaTime);
        }
        //if its not simulating then turn it white and make sure its at 0, 0, 0
        else
        {
            transform.gameObject.GetComponent<Renderer>().material.color = Color.Lerp(transform.gameObject.GetComponent<Renderer>().material.color, Color.white, Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, 0.0f, transform.position.z), Time.deltaTime);
        }
        
    }
    public void UpdateUI()
    {
        if (OnUpdatePlatformDataNodeUI != null)
        {
            OnUpdatePlatformDataNodeUI(this);
        }
    }

    //resets our node to default color and position if the program is not runnning
    public void ResetDataNode()
    {
        if (!Program)
        {
            Simulate = true;
            Selected = false;
            nextColor = Color.white;
            nextPosition = 0.0f;
        }
        else
        {
            Simulate = true;
            Selected = false;
        }
    }
    //if we're selecting a node turn it blue and mark it as selected
    public void SelectNode()
    {
        nextColor = Color.blue;
        Selected = true;
        Simulate = true;
        Program = true;

        if (OnUpdatePlatformDataNodeUI != null)
        {
            OnUpdatePlatformDataNodeUI(this);
        }
    }
    //if the values are close enough (less than 0.01 diff) then think of them as equal
    public static bool NearlyEquals(float? value1, float? value2, float unimportantDifference = 0.01f)
    {
        if (value1 != value2)
        {
            if (value1 == null || value2 == null)
                return false;

            return Math.Abs(value1.Value - value2.Value) < unimportantDifference;
        }

        return true;
    }

    //Returns a random variation of a color
    public Color ReturnColor(Color nameOfColor)
    {
        if (nameOfColor == Color.gray)
        {
            float randomValue = UnityEngine.Random.Range(0.0f, 1.0f);
            Color randomGray = new Color(randomValue, randomValue, randomValue, 1);
            return randomGray;
        }
        else if (nameOfColor == Color.red)
        {
            float randomValue = UnityEngine.Random.Range(0.0f, 1.0f);
            Color randomRed = new Color(randomValue, 0, 0, 1);
            return randomRed;
        }
        else if (nameOfColor == Color.green)
        {
            float randomValue = UnityEngine.Random.Range(0.0f, 1.0f);
            Color randomGreen = new Color(0, randomValue, 0, 1);
            return randomGreen;
        }
        else if (nameOfColor == Color.blue)
        {
            float randomValue = UnityEngine.Random.Range(0.0f, 1.0f);
            Color randomBlue = new Color(0, 0, randomValue, 1);
            return randomBlue;
        }
        else
        {
            float randomValueR = UnityEngine.Random.Range(0.0f, 1.0f);
            float randomValueG = UnityEngine.Random.Range(0.0f, 1.0f);
            float randomValueB = UnityEngine.Random.Range(0.0f, 1.0f);
            Color randomRGB = new Color(randomValueR, randomValueG, randomValueB, 1);
            return randomRGB;
        }
    }

    public override string ToString()
    {
        return string.Format("{0},{1},{2}", i, j, nextPosition);
    }
}
