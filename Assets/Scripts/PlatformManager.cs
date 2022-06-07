using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlatformManager : PlatformGenericSinglton<PlatformManager>
{
    #region
    public delegate void PlatformManagerChanged(PlatformConfigurationData pcd);
    public static event PlatformManagerChanged OnPlatformManagerChanged;

    public delegate void PlatformManagerUpdateUI();
    public static event PlatformManagerUpdateUI OnPlatformManagerUpdateUI;

    public delegate void ToggleSimulation();
    public static event ToggleSimulation OnToggleSimulation;
    #endregion

    #region
    public PlatformConfigurationData configurationData = new PlatformConfigurationData();
    public GameObject currentSelection=null, PlatformBasePref;
    public GameObject[,] platformNode;
    public bool Program=false, SimulateTest=false;
    int oldM, oldN;
    float xOffset = 0.0f, zOffset = 0.0f;
    public enum Shades
    {
        Red,
        Green,
        Blue,
        Gray,
        RGB
    }
    #endregion
    private void OnEnable()
    {
        UIManager.BuildPlatformOnClicked += UIManager_BuildPlatformOnClicked;
        UIManager.OnWriteProgramData += UIManager_OnWriteProgramData;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void OnDisable()
    {
        UIManager.BuildPlatformOnClicked -= UIManager_BuildPlatformOnClicked;
        UIManager.OnWriteProgramData -= UIManager_OnWriteProgramData;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    //get the platform configuration data and save it locally to be able to build the platform
    private void UIManager_BuildPlatformOnClicked(PlatformConfigurationData pcd)
    {
        configurationData = pcd;
        BuildPlatform();
        oldM = pcd.M;
        oldN = pcd.N;
    }

    private void UIManager_OnWriteProgramData()
    {
        Debug.Log("Writing a file to WriteLines.txt...");
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(Application.dataPath, "WriteLines.txt")))
        {
            outputFile.WriteLine(configurationData.ToString());
            for (int i = 0; i < oldM; i++)
            {
                for (int j = 0; j < oldN; j++)
                {
                    //Debug.Log(platformNode[i, j].GetComponent<PlatformDataNodeVer2>().ToString());
                    outputFile.WriteLine(platformNode[i, j].GetComponent<PlatformDataNode>().ToString());
                }
            }
        }
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (platformNode != null)
        {
            if (SceneManager.GetActiveScene().name == "ProgrammingScene")
            {
                Program = true;
            }
            else
            {
                Program = false;
            }      
            BuildPlatform();
            if (OnPlatformManagerUpdateUI != null)
            {
                OnPlatformManagerUpdateUI();
            }
            if (SceneManager.GetActiveScene().name != "SimulationScene")
            {
                SimulateTest = false;
            }
        }
        //Debug.Log(SceneManager.GetActiveScene().name);
    }

    //everytime we build a platform, destroy the old one
    public void BuildPlatform()
    {
        DestroyPlatform();
        var mDimension = configurationData.M;
        var nDimension = configurationData.N;
        platformNode = new GameObject[mDimension, nDimension];
        xOffset = 0.0f;
        zOffset = 0.0f;

        //translate colorIndex to a color shade
        var simulationShade = Shades.Gray;
        switch(configurationData.colorIndex)
        {
            case 0:
                simulationShade = Shades.Gray;
                break;
            case 1:
                simulationShade = Shades.Red;
                break;
            case 2:
                simulationShade = Shades.Green;
                break;
            case 3:
                simulationShade = Shades.Blue;
                break;
            case 4:
                simulationShade = Shades.RGB;
                break;
            default:
                simulationShade = Shades.Gray;
                break;
        }

        for (int i = 0; i < mDimension; i++)
        {
            //change z value to zero whenever we move to a new row, to set it back at the begining
            zOffset = 0.0f;
            for (int j = 0; j < nDimension; j++)
            {
                //create each platform pref square, name it, and add it to the array containing the whole platform
                var currentNode = Instantiate(PlatformBasePref, new Vector3((i + xOffset), (0), (j + zOffset)), Quaternion.identity);
                currentNode.name = string.Format("Node[" + i + "]" + "[" + j + "]");
                //Debug.Log(currentNode.name + " Initialized.");

                //each pref has its own data node with all of its own information
                currentNode.AddComponent<PlatformDataNode>();
                platformNode[i, j] = currentNode;
                PlatformDataNode pdn = currentNode.transform.GetComponent<PlatformDataNode>();
                pdn.Program = Program;
                pdn.i = i;
                pdn.j = j;
                pdn.shade = (PlatformDataNode.Shades)simulationShade;

                zOffset += configurationData.deltaSpace;
            }
            xOffset += configurationData.deltaSpace;
        }
        if (OnPlatformManagerChanged != null)
        {
            OnPlatformManagerChanged(configurationData);
        }   
    }

    //Destroys the platform (if there is one) so a new one can be formed when changing settings
    public void DestroyPlatform()
    {
        if (platformNode != null)
        {
            for (int i = 0; i < oldM; i++)
            {
                for (int j = 0; j < oldN; j++)
                {
                    Destroy(platformNode[i, j]);
                }
            }
        }
        platformNode = null;
    }

    public void StartSimulationButtonClick()
    {
        SimulateTest = !SimulateTest;

        //if there is no platform then we will have to create one using the file
        if (platformNode == null)
        {         
            Debug.Log("No platform detected, searching for configuration file.");
            PlatformConfigurationData pcd = new PlatformConfigurationData();
            //read the info from the file
            Debug.Log(Path.Combine(Application.dataPath, "WriteLines.txt"));
            using (System.IO.StreamReader sr = new System.IO.StreamReader(Path.Combine(Application.dataPath, "WriteLines.txt")))
            {
                String line;
                bool firstLine = true;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] tempData = line.Split(',');
                    //first line has general information to build the platform, the rest has the max height of each node
                    if (firstLine)
                    {
                        pcd.M = int.Parse(tempData[0]);
                        pcd.N = int.Parse(tempData[1]);
                        pcd.deltaSpace = float.Parse(tempData[2]);
                        pcd.RandomHeight = float.Parse(tempData[3]);
                        pcd.colorIndex = int.Parse(tempData[4]);
                        Program = true;
                        UIManager_BuildPlatformOnClicked(pcd);
                        firstLine = false;
                    }
                    else
                    {
                        PlatformDataNode pdn = platformNode[int.Parse(tempData[0]), int.Parse(tempData[1])].transform.GetComponent<PlatformDataNode>();
                        pdn.nextPosition = pdn.height = float.Parse(tempData[2]);
                    }
                    //Debug.Log(fileConfigHeight[i, j]+"\n");
                }
            }
            SimulateTest = false;
        }
        var mDimension = configurationData.M;
        var nDimension = configurationData.N;

        //if simulation is clicked on and there is a platformnode, then make each platform move and change color
        if (SimulateTest)
        {
            Debug.Log("Beginning simulation.");
            if (platformNode != null)
            {
                //store the height of the previous row to move onto the next row 
                float[] tempPosition = new float[nDimension];

                for (int k = 0; k < mDimension+1; k++)
                {
                    //simulate once and then move to the next row
                    if (OnToggleSimulation != null)
                    {
                        OnToggleSimulation();
                    }
                    
                    for (int i = 0; i < mDimension; i++)
                    {
                        for(int j = 0; j < nDimension; j++)
                        {
                            //if its the first one then store their height so the next row can use them and set this height and position to the last rows (to loop)
                            if (i == 0)
                            {
                                PlatformDataNode pdn = platformNode[i,j].transform.GetComponent<PlatformDataNode>();
                                PlatformDataNode pdnLast = platformNode[mDimension-1, j].transform.GetComponent<PlatformDataNode>();
                                tempPosition[j] = pdn.height;
                                pdn.nextPosition = pdn.height = pdnLast.height;
                            }
                            //if its the last one dont need to store its height
                            else if(i == mDimension - 1)
                            {
                                PlatformDataNode pdn = platformNode[i, j].transform.GetComponent<PlatformDataNode>();
                                pdn.nextPosition = pdn.height = tempPosition[j];
                            }
                            //else get the height from previous row and store it, then make this nodes height the next one in the array, finally set the previous row height to this row
                            else
                            {
                                PlatformDataNode pdn = platformNode[i, j].transform.GetComponent<PlatformDataNode>();
                                var nextPosition = tempPosition[j];
                                tempPosition[j] = pdn.height;
                                pdn.nextPosition = pdn.height = nextPosition;
                            }
                        }
                    }
                }
                
            }
        }
        else
        {           
            //if we're not simulating then return to the y=0
            for (int i = 0; i < mDimension; i++)
            {
                for (int j = 0; j < nDimension; j++)
                {
                    PlatformDataNode pdn = platformNode[i, j].transform.GetComponent<PlatformDataNode>();
                    pdn.Simulation = false;
                    pdn.Program = pdn.Simulate = true;
                    pdn.transform.position = new Vector3(pdn.transform.position.x, 0, pdn.transform.position.z);
                    pdn.transform.gameObject.GetComponent<Renderer>().material.color = Color.white;
                }
            }
            
        }
    }

    void Start()
    {

    }
    void Update()
    {
        //if the platform is not built then do not continue
        if (platformNode == null)
        {
            return;
        }

        //Only let the user select a node when in the programming phase
        if (Program)
        {
            if (Input.GetMouseButtonUp(0))
            {
                //if they click on a UI object then don't do anything with the nodes
                if (IsPointerOverUIObject())
                {
                    return;
                }            
                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
                if (hit)
                {
                    if (currentSelection != null)
                    {
                        PlatformDataNode pdn = currentSelection.transform.GetComponent<PlatformDataNode>();
                        pdn.ResetDataNode();
                    }
                    currentSelection = hitInfo.transform.gameObject;
                    PlatformDataNode newPdn = currentSelection.transform.GetComponent<PlatformDataNode>();
                    newPdn.SelectNode();
                }
                else
                {
                    Debug.Log("No hit");
                }
            }
        }
    }

    //determines if user is over a UI object if so then dont select a node
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
