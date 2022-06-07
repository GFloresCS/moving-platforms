using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region
    public delegate void BuildPlatformClicked(PlatformConfigurationData pcd);
    public static event BuildPlatformClicked BuildPlatformOnClicked;

    public delegate void NodeProgramChanged(Slider s);
    public static event NodeProgramChanged OnNodeProgramChanged;

    public delegate void UpdateCameraPosition(PlatformConfigurationData pcd);
    public static event UpdateCameraPosition OnUpdateCameraPosition;

    public delegate void WriteProgramData();
    public static event WriteProgramData OnWriteProgramData;
    #endregion

    #region
    public Dropdown dropDownColorSelection;
    public InputField inputPlatformMDimension, inputPlatformNDimension;
    public Slider sliderDeltaSpacing, sliderYAxisRange, sliderProgramHeight;
    #endregion

    #region
    public Text textPlatformDeltaSpacing, textPlatformDimensions, textPlatformYAxisRange, textSelectedNodeName, textSelectedNodePosition;
    #endregion

    int colorIndex = 0;

    //events that this class is listening for
    private void OnEnable()
    {
        PlatformManager.OnPlatformManagerChanged += PlatformManager_OnPlatformManagerChanged;
        PlatformManager.OnPlatformManagerUpdateUI += PlatformManager_OnPlatformManagerUpdateUI;
        PlatformDataNode.OnUpdatePlatformDataNodeUI += PlatformDataNode_OnUpdatePlatformDataNodeUI;
    }

    private void OnDisable()
    {
        PlatformManager.OnPlatformManagerChanged -= PlatformManager_OnPlatformManagerChanged;
        PlatformManager.OnPlatformManagerUpdateUI -= PlatformManager_OnPlatformManagerUpdateUI;
        PlatformDataNode.OnUpdatePlatformDataNodeUI -= PlatformDataNode_OnUpdatePlatformDataNodeUI;
    }

    //if platform manager has changed any data then update the values for the Setup Scene UI as well
    private void PlatformManager_OnPlatformManagerChanged(PlatformConfigurationData pcd)
    {
        if(pcd == null){}
        else
        {
            if(OnUpdateCameraPosition != null)
            {
                OnUpdateCameraPosition(pcd);
            }
            if (inputPlatformMDimension != null)
            {
                inputPlatformMDimension.text = pcd.M.ToString();
            }
            if (inputPlatformNDimension != null)
            {
                inputPlatformNDimension.text = pcd.N.ToString();
            }

            textPlatformDimensions.text = string.Format("{0}x{1}", pcd.M, pcd.N);

            if(sliderDeltaSpacing != null)
            {
                sliderDeltaSpacing.value = pcd.deltaSpace;
            }

            if(sliderYAxisRange != null)
            {
                sliderYAxisRange.value = pcd.RandomHeight;
            }

            if(textPlatformDeltaSpacing != null)
            {
                textPlatformDeltaSpacing.text = string.Format("{0}f", (float)Math.Round(pcd.deltaSpace, 2));
            }
        }
    }
    
    //Updates the UI for the programming scene
    private void PlatformManager_OnPlatformManagerUpdateUI()
    {
        if (!PlatformManager.Instance.Program)
        {
            if (sliderYAxisRange != null)
            {
                sliderYAxisRange.value = PlatformManager.Instance.configurationData.RandomHeight;
            }
        } 
        else
        {
            if (sliderYAxisRange != null)
            {
                sliderYAxisRange.value = 0.0f;
            }
        }
        if (textSelectedNodeName != null)
        {
            textSelectedNodeName.text = "";
        }  
        if (textSelectedNodePosition != null)
        {
            textSelectedNodePosition.text = ("Height:");
        }     
    }
 
    //updates the slider max and min for the programming scene and the name and position of the node
    private void PlatformDataNode_OnUpdatePlatformDataNodeUI(PlatformDataNode pdn)
    {
        sliderProgramHeight.value = pdn.height;
        sliderProgramHeight.minValue = 0;
        sliderProgramHeight.maxValue = PlatformManager.Instance.configurationData.RandomHeight;
        textPlatformDeltaSpacing.text = string.Format("{0}f", (float)Math.Round(PlatformManager.Instance.configurationData.deltaSpace, 2));
        textSelectedNodePosition.text = string.Format("Height: {0}f", (float)Math.Round(pdn.height, 2));
        textSelectedNodeName.text = string.Format("Selected Node: [{0}, {1}]", pdn.i, pdn.j);
    }

    public void ButtonClicked(Button s)
    {
        switch (s.name)
        {
            case "SetupButton":
                SceneManager.LoadScene("SetupScene");
                break;
            case "ProgramButton":
                SceneManager.LoadScene("ProgrammingScene");
                break;
            case "SimulateButton":
                //SceneManager.LoadScene("SimulationScene");
                //SceneManager.LoadScene("SetupScene");
                //SceneManager.LoadScene("ProgrammingScene");
                PlatformManager.Instance.StartSimulationButtonClick();
                break;
            case "MainMenuButton":
                SceneManager.LoadScene("StartScene");
                break;
            case "BuildPlatformButton":
                if (BuildPlatformOnClicked != null)
                {
                    try
                    {
                        PlatformConfigurationData pcd = new PlatformConfigurationData();

                        //check that theyre both integers if not then display an error message
                        var isMInt = int.TryParse(inputPlatformMDimension.text, out int tempM);
                        var isNInt = int.TryParse(inputPlatformNDimension.text, out int tempN);

                        if (isMInt && isNInt)
                        {
                            //check that theyre both greater than or equal to 0, if so then set the all values
                            if (tempM >= 0 && tempN >= 0)
                            {
                                pcd.M = tempM;
                                pcd.N = tempN;
                                pcd.deltaSpace = sliderDeltaSpacing.value;
                                pcd.RandomHeight = sliderYAxisRange.value;
                                //0 = grayscale, 1 = red, 2 = green, 3 = blue, 4 = rgb
                                switch (dropDownColorSelection.value)
                                {
                                    case 0:
                                        //gray
                                        pcd.colorIndex = 0;
                                        break;
                                    case 1:
                                        //red
                                        pcd.colorIndex = 1;
                                        break;
                                    case 2:
                                        //green
                                        pcd.colorIndex = 2;
                                        break;
                                    case 3:
                                        //blue
                                        pcd.colorIndex = 3;
                                        break;
                                    case 4:
                                        //rgb
                                        pcd.colorIndex = 4;
                                        break;
                                }
                                BuildPlatformOnClicked(pcd);
                            }
                            else
                            {
                                Debug.Log("One or more of the values entered for M and N are negative!");
                            }
                        }
                        else
                        {
                            Debug.Log("One or more of the values entered for M and N aren't integers!");
                        }
                    }
                    catch (FormatException)
                    {
                        Debug.Log("The values entered aren't numbers!");
                    }
                }            
                break;
            case "PlatformProgramButton":
                if (OnWriteProgramData != null)
                {
                    OnWriteProgramData();
                }
                break;
            case "ExitButton":
                //UnityEditor.EditorApplication.isPlaying = false;
                Application.Quit();
                break;
        }
    }

    public void SliderChange(Slider s)
    {
        switch (s.name)
        {
            case "DeltaSpacingSlider":
                {
                    textPlatformDeltaSpacing.text = string.Format("{0}f", (float)Math.Round(s.value, 2));
                    break;
                }
            case "YAxisRangeSlider":
                {
                    textPlatformYAxisRange.text = string.Format("{0}", (float)Math.Round(s.value));
                    break;
                }
            case "NodeHeightSlider":
                {
                    if (OnNodeProgramChanged != null)
                    {
                        OnNodeProgramChanged(s);
                    }    
                    break;
                }
        }
    }

    public void colorSelectionDropDown(Dropdown colorSelection)
    {
        //0 = grayscale, 1 = red, 2 = green, 3 = blue, 4 = rgb
        colorIndex = colorSelection.value;
    }
}
