using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class HistoryData
{
    public int id;
    public string sentence;
}    

public class PlayCommand
{
    public int id;
    public string content;
    public string target1;
}

public class PlayCommandRelation
{
    public int id;
    public int selectNumber;
    public string currentFile;
    public string nextFile;
}

public class PlayerPrefsData
{
    public int nowCommandIndex;
    public float iconPosX;
    public float iconPosY;
    public float nextIconPosX;
    public float nextIconPosY;
    public string background;
    public string chapter;
    public string historyJsonString;
    public string sentence;
    public TextAsset csvFile;
}

public class Story : MonoBehaviour
{
    private bool allowDisplayingAllCurrentSentence = false;
    private bool allowDisplayingFromContinuation = false;
    private bool allowStoryProceed = true;
    private bool cancelButtonClick = false;
    private bool downMiddle = false;
    private bool downBottom = false;
    private bool displaySentenceControllerfinish = false;
    private bool finishDisplayingAllCurrentSentence = false;

    private float distTwoPoint = 0.0f;
    private float startHistoryDragTime;
    private float oldPositionT1Y;
    private float oldPositionT2Y;
    private float beganTouchTime;
    private float endedTouchTime;
    private Touch t1;
    private Touch t2;
    private float twoPointDist = 0.0f;
    private float v = 1.0f;
    private float vMin = 1.0f;
    private float vMax = 5.0f;
    private float x = 0;
    private float y = 0;

    private GameObject copyHistoryText1;
    private GameObject copyHistoryText2;
    private GameObject copyHistoryText3;
    private GameObject icon;
    private GameObject nextIcon;

    private int currentHistoryNum;
    private int historyId = 0;
    private int nowCommandIndex;
    private int maxTouchCount;
    private int startRow = 0;

    private List <HistoryData> historyData;

    private string activePanel = "";
    private string destroyingHistoryText = "";
    private string displayingHistoryText = "";
    private string historyJsonString = "";
    private string jsonString = "";
    private string nowChapter = "";
    private string nowCommandValue = "";
    private string nowContentName = "";
    private string nowTime = "";

    public GameObject[] menuButtons;
    private Tween[] menuButtonsTween = new Tween[6];
    private Tween loadPanelFade;
    private Tween menuBackgroundFade;
    private Tween operationMethodPanelFade;
    private Tween savePanelFade;
    private Tween settingPanelFade;
    private Tween storyTextFade;

    private Vector2 beganTouchPosition;
    private Vector2 endedTouchPosition;

    public AudioSource sound;

    public GameObject chapterPanel;
    public GameObject historyPanel;
    public GameObject selectPanel;
    public GameObject storyPanel;

    public GameObject historyText;
    public GameObject storyBackground;
    public GameObject storyText;

    public GameObject main;
    public GameObject menu;

    public GameObject defaultPanel;
    public GameObject loadPanel;
    public GameObject operationMethodPanel;
    public GameObject savePanel;
    public GameObject settingPanel;

    public GameObject menuBackground;

    public HistoryData[] historyDatum;

    public PlayCommand[] commandsDatum;

    public PlayCommandRelation[] commandRelationDatum;

    public TextAsset commandFile;
    public TextAsset commandRelationFile;
    public Slider characterSpeedSlider;
    public Slider soundSlider;


    // Start is called before the first frame update
    void Start()
    {  
        this.storyText.GetComponent<Text>().text = null;

        if(PlayerPrefs.GetString("currentSlot") != "")
        {
            StartCoroutine(ReadSaveData());    
        }
        else
        {
            StartStory(0, this.commandFile);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // UUID作成
        // Debug.Log(Guid.NewGuid().ToString());
        if(Input.GetKeyDown(KeyCode.D))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("セーブ消去");
        }

        DateTime dt = DateTime.Now;
        this.nowTime = dt.ToString("yyyy/MM/dd HH:mm:ss");


        if(Input.touchCount == 2)
        {   
            this.maxTouchCount = 2;
        }

        if(Input.touchCount == 1)
        {
            if(this.maxTouchCount < 2)
            {
                this.maxTouchCount = 1;
                Touch t = Input.touches[0];

                if(t.phase == TouchPhase.Began)
                {
                    this.beganTouchPosition = t.position;
                    this.beganTouchTime = Time.time;
                }

                if(t.phase == TouchPhase.Ended)
                {
                   this.endedTouchPosition = t.position;
                   this.endedTouchTime = Time.time;
                }
            }
        }
            
        if(Input.touchCount == 0)
        {
            switch (this.maxTouchCount)
            {
                case 2:
                    if(menu.activeSelf != true)
                    {
                        MenuController();
                    }

                    if(historyPanel.activeSelf == true)
                    {
                        Destroy(this.copyHistoryText1);
                        Destroy(this.copyHistoryText2);
                        Destroy(this.copyHistoryText3);

                        this.destroyingHistoryText = "";
                        this.displayingHistoryText = "";

                        this.historyPanel.SetActive(false);

                        this.storyText.GetComponent<CanvasGroup>().DOFade(1.0f,0.5f);
                    }

                    this.maxTouchCount = 0;
                    break;

                case 1:
                    Vector2 distance = this.endedTouchPosition - this.beganTouchPosition;
                    float differenceTime = this.endedTouchTime - this.beganTouchTime;

                    if(chapterPanel.activeSelf != true && menu.activeSelf != true)
                    {
                        if(distance.y < -10 && differenceTime < 0.5f && distance.x < - distance.y)
                        {
                            DropHistoryControl();
                        }
                        else if(distance.y > 10 && differenceTime < 0.5f && distance.x < distance.y)
                        {
                            UpHistoryControl();
                        }     
                        else
                        {
                            if(selectPanel.activeSelf != true && historyPanel.activeSelf != true)
                            {
                                if(this.cancelButtonClick != true)
                                {
                                    AdvanceSentence();
                                }
                            }
                        }
                    }
                    this.cancelButtonClick = false;
                    this.maxTouchCount = 0;
                    break;
            }
        }

        // Debug.Log(Input.touchCount);
        // Debug.Log(t.phase);
        // Debug.Log(t.position);
        if (Input.GetKeyDown(KeyCode.K))
        {
            MenuController();
        }

    }

    private void StartStory(int startRow, TextAsset commandFile)
    {
        CreateJson(commandFile);
        this.commandsDatum = JsonConvert.DeserializeObject<PlayCommand[]>(this.jsonString);

        this.icon = this.storyText.transform.Find("Icon").gameObject;
        this.nextIcon = this.storyText.transform.Find("NextIcon").gameObject;

        PlayerPrefs.SetString("currentSlot", "");
        StartCoroutine(CommandLoop(startRow));
    }

    private IEnumerator CommandLoop(int startRow)
    {
        for(this.nowCommandIndex = startRow; this.nowCommandIndex < this.commandsDatum.Length; this.nowCommandIndex++)
        {
            this.nowCommandValue = this.commandsDatum[this.nowCommandIndex].target1;
            this.nowContentName = this.commandsDatum[this.nowCommandIndex].content;
            yield return StartCoroutine(this.commandsDatum[this.nowCommandIndex].content);
        }
    }

    private IEnumerator Background()
    {
        this.storyBackground.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Yakutsujima/StoryScene/Images/Chapters/" + this.nowCommandValue + ".png");

        yield break;
    }

    private IEnumerator Chapter()
    {
        this.chapterPanel.SetActive(true);

        GameObject chapterBackground = this.chapterPanel.transform.Find("ChapterBackground").gameObject;
        chapterBackground.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Yakutsujima/StoryScene/Images/Chapters/" + this.nowCommandValue + ".png");

        string[] chapterSplit = this.nowCommandValue.Split('/');
        this.nowChapter = chapterSplit[0];

        yield return new WaitUntil(()=> this.chapterPanel.activeSelf == false);
    }

    private IEnumerator Music()
    {
        this.sound.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Yakutsujima/StoryScene/Musics/Chapters/" + this.nowCommandValue + ".mp3");
        this.sound.Play();

        yield break;
    }

    private IEnumerator NextPage()
    {
        CreateHistoryData();
        this.storyText.GetComponent<Text>().text = null;

        yield break;
    }

    private IEnumerator Select()
    {
        if(this.storyText.GetComponent<Text>().text != "")
        {
            CreateHistoryData();
        }

        this.selectPanel.SetActive(true);
        this.storyText.GetComponent<Text>().text = null;

        string[] selectrow = this.nowCommandValue.Split('/');

        switch (selectrow.Length)
        {
            case 3:
                GameObject.Find("SelectButtonText1").GetComponentInChildren<Text>().text = "1." + selectrow[0];
                GameObject.Find("SelectButtonText2").GetComponentInChildren<Text>().text = "2." + selectrow[1];
                GameObject.Find("SelectButtonText3").GetComponentInChildren<Text>().text = "3." + selectrow[2];
                this.storyText.GetComponent<Text>().text = "\n\n\n\n" + "1." + selectrow[0] + "\n\n\n" + "2." + selectrow[1] + "\n\n\n" + "3." + selectrow[2];
                break;
            case 2:
                GameObject.Find("SelectButtonText1").GetComponentInChildren<Text>().text = "1." + selectrow[0];
                GameObject.Find("SelectButtonText2").GetComponentInChildren<Text>().text = "2." + selectrow[1];
                GameObject.Find("SelectButtonText3").SetActive(false);
                this.storyText.GetComponent<Text>().text = "\n\n\n\n" + "1." + selectrow[0] + "\n\n\n" + "2." + selectrow[1];
                break;
            case 1:
                GameObject.Find("SelectButtonText1").GetComponentInChildren<Text>().text = "1." + selectrow[0];
                GameObject.Find("SelectButtonText2").SetActive(false);
                GameObject.Find("SelectButtonText3").SetActive(false);
                this.storyText.GetComponent<Text>().text = "\n\n\n\n" + "1." + selectrow[0];
                break;
        }

        this.storyText.SetActive(false);

        yield break;

    }
    private IEnumerator Sentence()
    {    
        this.allowDisplayingAllCurrentSentence = false;
        this.finishDisplayingAllCurrentSentence = false;

        string currentSentence = this.nowCommandValue;
        string[] sentenceSplit = currentSentence.Split('$');
        
        if(sentenceSplit.Length > 1)
        {
            for (int i = 0; i < sentenceSplit.Length; i++)
            {
                if(i == 0)
                {
                    currentSentence = sentenceSplit[i] + "\n";
                }
                else
                {
                    currentSentence += sentenceSplit[i] + "\n";
                }
            }
        }

        DisplaySentenceController(currentSentence);
        
        yield return new WaitUntil(()=> displaySentenceControllerfinish);
        this.displaySentenceControllerfinish = false;

        this.storyText.GetComponent<Text>().text += "\n\n";
        this.icon.GetComponent<RectTransform>().anchoredPosition = new Vector3(x,y);
        this.nextIcon.GetComponent<RectTransform>().anchoredPosition = new Vector3(x,y);

        if(this.commandsDatum[this.nowCommandIndex + 1].content != "NextPage" && this.commandsDatum[this.nowCommandIndex + 1].content != "")
        {
            icon.SetActive(true);
        }
        else
        {
            nextIcon.SetActive(true);
            this.y = 0;
        }

        yield return new WaitUntil(()=> finishDisplayingAllCurrentSentence);
    }

    private async void DisplaySentenceController(string currentSentence)
    {
        for(int i = 0; i < currentSentence.Length; i++)
        {
            if (this.allowDisplayingAllCurrentSentence)
            {
                this.storyText.GetComponent<Text>().text += currentSentence.Substring(i, currentSentence.Length - i);

                break;
            }
            else
            {
                await Task.Delay((int)Mathf.Floor(this.characterSpeedSlider.value));
                this.storyText.GetComponent<Text>().text += currentSentence.Substring(i,1);       
            }
        } 
        displaySentenceControllerfinish = true;
    }


    private void CreateHistoryData()
    {
        if(this.historyId == 0)
        {
            this.historyData = new List<HistoryData>();
        }

        this.historyData.Add(new HistoryData{id = this.historyId, sentence = this.storyText.GetComponent<Text>().text});
        this.historyJsonString = JsonConvert.SerializeObject(historyData);
        this.historyDatum = JsonConvert.DeserializeObject<HistoryData[]>(this.historyJsonString);
        this.historyId++;
    }

    private string FileExchange(int selectButtonNum)
    {
        CreateJson(this.commandRelationFile);
        this.commandRelationDatum = JsonConvert.DeserializeObject<PlayCommandRelation[]>(this.jsonString);

        int[] currentId = null;

        for (int i = 0; i < this.commandRelationDatum.Length; i++)
        {
            if(this.commandFile.name == this.commandRelationDatum[i].currentFile)
            {
                if (currentId == null)
                {
                    currentId = new int[this.commandRelationDatum[i].selectNumber];
                    currentId[this.commandRelationDatum[i].selectNumber - 1] = commandRelationDatum[i].id;
                }
                else
                {
                    Array.Resize(ref  currentId, currentId.Length + 1);
                    currentId[this.commandRelationDatum[i].selectNumber - 1] = commandRelationDatum[i].id;         
                }
            }
        }
        Debug.Log(currentId[selectButtonNum - 1]);
        return commandRelationDatum[currentId[selectButtonNum - 1]].nextFile;
    }

    // スワイプ操作
    private void DropHistoryControl()
    {
        if(this.historyPanel.activeSelf == false)
        {
            if(selectPanel.activeSelf == true)
            {
                this.selectPanel.GetComponent<CanvasGroup>().DOFade(0.0f, 0.5f);           
            }

            if(this.historyDatum != null && this.historyDatum.Length > 0)
            {  
                this.storyText.GetComponent<CanvasGroup>().DOFade(0.0f, 0.5f);

                this.historyText.GetComponent<CanvasGroup>().alpha = 0;
                this.historyText.gameObject.SetActive(true);

                this.historyPanel.SetActive(true);

                this.currentHistoryNum = this.historyDatum.Length - 1;
                this.copyHistoryText1 = GameObject.Instantiate(this.historyText, this.historyPanel.transform) as GameObject;
                this.copyHistoryText1.GetComponent<Animator>().SetTrigger("DownMiddleHistoryText");
                this.copyHistoryText1.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;

                this.displayingHistoryText = "copyHistoryText1";
            }
        }
        else if(this.historyPanel.activeSelf == true)
        {
            switch (this.destroyingHistoryText)
            {
                case "copyHistoryText1":
                    Destroy(this.copyHistoryText1);
                    break;
                case "copyHistoryText2":
                    Destroy(this.copyHistoryText2);
                    break;
                case "copyHistoryText3":
                    Destroy(this.copyHistoryText3);
                    break;                                    
            }

            if(currentHistoryNum != 0)
            {
                if (this.displayingHistoryText == "copyHistoryText1")
                {
                    this.copyHistoryText1.GetComponent<Animator>().SetTrigger("DownBottomHistoryText");
                    this.destroyingHistoryText = "copyHistoryText1";
                    // Destroy(this.copyHistoryText1);

                    this.currentHistoryNum--;

                    this.copyHistoryText2 = GameObject.Instantiate(this.historyText, this.historyPanel.transform) as GameObject;
                    this.copyHistoryText2.GetComponent<Animator>().SetTrigger("DownMiddleHistoryText");
                    this.copyHistoryText2.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;

                    this.displayingHistoryText = "copyHistoryText2";
                }
                else if(this.displayingHistoryText == "copyHistoryText2")
                {
                    this.copyHistoryText2.GetComponent<Animator>().SetTrigger("DownBottomHistoryText");
                    this.destroyingHistoryText = "copyHistoryText2";
                    // Destroy(this.copyHistoryText2);

                    this.currentHistoryNum--;

                    this.copyHistoryText3 = GameObject.Instantiate(this.historyText, this.historyPanel.transform) as GameObject;
                    this.copyHistoryText3.GetComponent<Animator>().SetTrigger("DownMiddleHistoryText");
                    this.copyHistoryText3.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;

                    this.displayingHistoryText = "copyHistoryText3";
                }
                else if(this.displayingHistoryText == "copyHistoryText3")
                {
                    this.copyHistoryText3.GetComponent<Animator>().SetTrigger("DownBottomHistoryText");
                    this.destroyingHistoryText = "copyHistoryText3";                    
                    // Destroy(this.copyHistoryText3);

                    this.currentHistoryNum--;

                    this.copyHistoryText1 = GameObject.Instantiate(this.historyText, this.historyPanel.transform) as GameObject;
                    this.copyHistoryText1.GetComponent<Animator>().SetTrigger("DownMiddleHistoryText");
                    this.copyHistoryText1.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;

                    this.displayingHistoryText = "copyHistoryText1";
                }
            }
        }
    }

    private void UpHistoryControl()
    {
        if(this.historyPanel.activeSelf == true)
        {
            switch (this.destroyingHistoryText)
            {
                case "copyHistoryText1":
                    Destroy(this.copyHistoryText1);
                    break;
                case "copyHistoryText2":
                    Destroy(this.copyHistoryText2);
                    break;
                case "copyHistoryText3":
                    Destroy(this.copyHistoryText3);
                    break;                                    
            }

            if(this.historyDatum.Length == this.currentHistoryNum + 1)
            {
                if (selectPanel.activeSelf == true)
                {
                    selectPanel.GetComponent<CanvasGroup>().DOFade(1.0f, 0.5f);
                }

                Destroy(this.copyHistoryText1);
                Destroy(this.copyHistoryText2);
                Destroy(this.copyHistoryText3);

                this.destroyingHistoryText = "";
                this.displayingHistoryText = "";

                this.historyPanel.SetActive(false);

                this.storyText.GetComponent<CanvasGroup>().DOFade(1.0f,0.5f);
            }
            else if(this.displayingHistoryText == "copyHistoryText1")
            {
                this.copyHistoryText1.GetComponent<Animator>().SetTrigger("UpTopHistoryText");
                this.destroyingHistoryText = "copyHistoryText1";
                // Destroy(copyHistoryText1);

                this.currentHistoryNum++;

                this.copyHistoryText2 = GameObject.Instantiate(this.historyText, this.historyPanel.transform) as GameObject;
                this.copyHistoryText2.GetComponent<Animator>().SetTrigger("UpMiddleHistoryText");
                this.copyHistoryText2.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;

                this.displayingHistoryText = "copyHistoryText2";
            }
            else if(this.displayingHistoryText == "copyHistoryText2")
            {
                this.copyHistoryText2.GetComponent<Animator>().SetTrigger("UpTopHistoryText");
                this.destroyingHistoryText = "copyHistoryText2";
                // Destroy(copyHistoryText2);

                this.currentHistoryNum++;

                this.copyHistoryText3 = GameObject.Instantiate(this.historyText, this.historyPanel.transform) as GameObject;
                this.copyHistoryText3.GetComponent<Animator>().SetTrigger("UpMiddleHistoryText");
                this.copyHistoryText3.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;

                this.displayingHistoryText = "copyHistoryText3";
            }
            else if(this.displayingHistoryText == "copyHistoryText3")
            {
                this.copyHistoryText3.GetComponent<Animator>().SetTrigger("UpTopHistoryText");
                this.destroyingHistoryText = "copyHistoryText3";
                // Destroy(copyHistoryText3);

                this.currentHistoryNum++;

                this.copyHistoryText1 = GameObject.Instantiate(this.historyText, this.historyPanel.transform) as GameObject;
                this.copyHistoryText1.GetComponent<Animator>().SetTrigger("UpMiddleHistoryText");
                this.copyHistoryText1.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;

                this.displayingHistoryText = "copyHistoryText1";
            }
        }
    }

    // 文章を進める
    public void AdvanceSentence()
    {
        if(this.allowStoryProceed == true)
        {
            if(this.commandsDatum[this.nowCommandIndex].content == "Sentence")
            {
                if(this.icon.activeSelf)
                {
                    this.icon.SetActive(false);
                    this.finishDisplayingAllCurrentSentence = true;
                }
                else if(this.nextIcon.activeSelf)
                {
                    this.nextIcon.SetActive(false);
                    this.finishDisplayingAllCurrentSentence = true;
                }
                else
                {
                    this.allowDisplayingAllCurrentSentence = true;
                }
            }
        }
        else if(this.allowStoryProceed == false)
        {
            this.allowDisplayingFromContinuation = true;
        }
    }

    //Jsonの作成
    private void CreateJson(TextAsset csv)
    {
        StringReader reader = new StringReader(csv.text);
        List<string[]> sentenseList = new List<string[]>();
        string[] properties = null;
        int lineCount = 0;

        //文字列から1行ずつ読み込む
        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();

            if (lineCount == 0)
            {
                properties = line.Split(',');
            }

            sentenseList.Add(line.Split(','));
            lineCount += 1;
        }

        List<Dictionary<string, string>> listObjResult = new List<Dictionary<string, string>>();

        for (int i = 1; i < lineCount; i += 1)
        {
            Dictionary<string, string> objResult = new Dictionary<string, string>();

            for (int j = 0; j < properties.Length; j += 1)
            {
                objResult.Add(properties[j], sentenseList[i][j]);
            }

            listObjResult.Add(objResult);
        }

        // 材料を文字列化する => "[{"sentence": "Aaa", "isLast": "0"}, {"sentence": "Bbb", "isLast": "1"}, ...]"
        this.jsonString = JsonConvert.SerializeObject(listObjResult);
    }

    //次のCsvFileへ
    public void NextCommandFile(int selectButtonNum)
    {
        CreateHistoryData();
        this.storyText.SetActive(true);
        this.storyText.GetComponent<Text>().text = null;
        FileExchange(selectButtonNum);

        this.commandFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Yakutsujima/StoryScene/CsvFiles/Chapters/" + FileExchange(selectButtonNum) + ".csv");

        StartStory(0, this.commandFile);
    }

    // Menuのフェードを管理
    private void MenuController()
    {
        if(this.menu.activeSelf == false)
        {
            if(this.historyPanel.activeSelf == true)
            {
                Destroy(this.copyHistoryText1, 1.0f);
                Destroy(this.copyHistoryText2, 1.0f);
                Destroy(this.copyHistoryText3, 1.0f);
                this.historyPanel.SetActive(false);
                this.storyText.GetComponent<CanvasGroup>().DOFade(1.0f,0.5f);

                if(selectPanel.activeSelf == true)
                {
                    selectPanel.GetComponent<CanvasGroup>().DOFade(1.0f, 0.5f);
                }
            }
            else
            {
                this.storyTextFade.Kill();
                OpenMenu();
            }
        }
        else
        {
            CloseMenu();
        }
    }
    private void OpenMenu()
    {
        this.storyText.GetComponent<CanvasGroup>().alpha = 0;
        this.menu.SetActive(true);
        // this.menuBackgroundFade.Kill();
        this.menuBackground.GetComponent<CanvasGroup>().alpha = 1;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            this.menuButtonsTween[i] = this.menuButtons[i].GetComponent<CanvasGroup>().DOFade(1.0f,1.0f);
        }

    }
    public void CloseMenu()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            this.menuButtonsTween[i].Kill();
        }

        for (int i = 0; i < menuButtons.Length; i++)
        {
            this.menuButtons[i].GetComponent<CanvasGroup>().alpha = 0;
        }

        this.storyTextFade = this.storyText.GetComponent<CanvasGroup>().DOFade(1.0f,1.0f);
        this.storyBackground.GetComponent<CanvasGroup>().alpha = 1;
        
        this.loadPanel.SetActive(false);
        this.operationMethodPanel.SetActive(false);
        this.savePanel.SetActive(false);
        this.settingPanel.SetActive(false);
        this.cancelButtonClick = true;
        this.menu.SetActive(false);
    }

    //Menu内で管理しているPanelsのフェード 

    public void CloseMenuPanels()
    {
        switch (this.activePanel)
        {
            case "operationMethodPanel":
                this.operationMethodPanelFade.Kill();
                this.operationMethodPanel.GetComponent<CanvasGroup>().alpha = 0;       
                this.operationMethodPanel.SetActive(false);
                break;

            case "loadPanel":
                this.loadPanelFade.Kill();
                this.loadPanel.GetComponent<CanvasGroup>().alpha = 0;       
                this.loadPanel.SetActive(false);
                break;

            case "savePanel":
                this.savePanelFade.Kill();
                this.savePanel.GetComponent<CanvasGroup>().alpha = 0;       
                this.savePanel.SetActive(false);
                break;

            case "settingPanel":
                this.settingPanelFade.Kill();
                this.settingPanel.GetComponent<CanvasGroup>().alpha = 0;       
                this.settingPanel.SetActive(false);
                break;
            default:
                break;
        }
        this.activePanel = "";
    }
    
    public void OpenSaveLoadPanel(GameObject Panel)
    {
        Panel.SetActive(true);

        if(Panel.name == "SavePanel")
        {
            SetSlot("SaveSlot");
            this.savePanelFade = this.savePanel.GetComponent<CanvasGroup>().DOFade(1.0f,1.0f);
            this.activePanel = "savePanel";
        }
        else if(Panel.name == "LoadPanel")
        {
            SetSlot("LoadSlot");
            this.loadPanelFade = this.loadPanel.GetComponent<CanvasGroup>().DOFade(1.0f,1.0f);
            this.activePanel = "loadPanel";
        }
    }
    private void SetSlot(string slotName)
    {
        for(int i = 1; i <= 4; i++)
        {
            if(PlayerPrefs.GetString("Slot" + i + "time") == "")
            {
                GameObject.Find(slotName + i).GetComponent<Text>().text = " ---- / -- / --   -- : -- : --";
            }
            else
            {
                GameObject.Find(slotName + i).GetComponent<Text>().text = PlayerPrefs.GetString("Slot" + i + "time");
            }
        }
    }

    public void OpenSettingPanel()
    {
        this.settingPanel.SetActive(true);
        this.settingPanelFade = this.settingPanel.GetComponent<CanvasGroup>().DOFade(1.0f,1.0f);
        this.activePanel = "settingPanel";
    }
    
    public void OpenOperationMethodPanel()
    {
        this.operationMethodPanel.SetActive(true);
        this.operationMethodPanelFade = this.operationMethodPanel.GetComponent<CanvasGroup>().DOFade(1.0f,1.0f);
        this.activePanel = "operationMethodPanel";        
    }
    

    //設定のリセット 
    public void RestoreDefault()
    {
        this.characterSpeedSlider.value = 3.0f;
        this.soundSlider.value = 0.5f;
    }


    // タイトルに戻るボタン
    public void ReturnTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }


　  //環境設定
    // スピード等を変更した際、保存する
    public void PlayerPrefsCharacterSpeed()
    {
        PlayerPrefs.SetFloat("characterSpeed", this.characterSpeedSlider.value);                
    }
    public void PlayerPrefsVolume()
    {
        PlayerPrefs.SetFloat("volume", this.soundSlider.value);        
    }


    // セーブロード
    public void Save(string slotName)
    {      
        PlayerPrefs.SetInt(slotName + "historyId", this.historyId);
        PlayerPrefs.SetInt(slotName + "nowCommandIndex", this.nowCommandIndex);
        PlayerPrefs.SetString(slotName + "nowContentName", this.nowContentName);
        PlayerPrefs.SetString(slotName + "background", this.storyBackground.GetComponent<Image>().sprite.name);
        PlayerPrefs.SetString(slotName + "commandFileName", this.commandFile.name);
        PlayerPrefs.SetString(slotName + "chapter", this.nowChapter);
        PlayerPrefs.SetString(slotName + "historySentence", this.historyJsonString);
        PlayerPrefs.SetString(slotName + "sound", this.sound.clip.name);
        PlayerPrefs.SetString(slotName + "sentence", this.storyText.GetComponent<Text>().text);
        PlayerPrefs.SetString(slotName + "time", this.nowTime);
        PlayerPrefs.Save();

        GameObject.Find("Save" + slotName).GetComponent<Text>().text = PlayerPrefs.GetString(slotName + "time");
    }

    public void Load(string slotName)
    {
        if(PlayerPrefs.GetString(slotName + "time") != "")
        {
            PlayerPrefs.SetString("currentSlot",　slotName);
            SceneManager.LoadScene("StoryScene");
        }
    }
  
    private IEnumerator ReadSaveData()
    {
        this.nowChapter = PlayerPrefs.GetString(PlayerPrefs.GetString("currentSlot") + "chapter");

        if(this.characterSpeedSlider.value != 3.0f)
        {
            this.characterSpeedSlider.value = PlayerPrefs.GetFloat("characterSpeed");
        }

        this.commandFile.name = PlayerPrefs.GetString(PlayerPrefs.GetString("currentSlot") + "commandFileName");
        this.commandFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Yakutsujima/StoryScene/CsvFiles/Chapters/" + this.nowChapter + "/" + this.commandFile.name + ".csv");
        this.historyId = PlayerPrefs.GetInt(PlayerPrefs.GetString("currentSlot") + "historyId");
        this.historyJsonString = PlayerPrefs.GetString(PlayerPrefs.GetString("currentSlot") + "historySentence");
        this.historyDatum = JsonConvert.DeserializeObject<HistoryData[]>(this.historyJsonString);
        this.historyData = JsonConvert.DeserializeObject<List<HistoryData>>(this.historyJsonString);
        this.sound.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Yakutsujima/StoryScene/Musics/Chapters/" + this.nowChapter + "/" + PlayerPrefs.GetString(PlayerPrefs.GetString("currentSlot") + "sound") + ".mp3");
        
        if(this.soundSlider.value != 0.5f)
        {
            this.soundSlider.value = PlayerPrefs.GetFloat("volume");
        }
        this.sound.Play();        

        if(PlayerPrefs.GetString(PlayerPrefs.GetString("currentSlot") + "nowContentName") == "Select")
        {
            StartStory(PlayerPrefs.GetInt(PlayerPrefs.GetString("currentSlot") + "nowCommandIndex") - 1, this.commandFile);          
        }
        else
        {
            this.storyText.GetComponent<Text>().text = PlayerPrefs.GetString(PlayerPrefs.GetString("currentSlot") + "sentence");
            this.allowStoryProceed = false;

            yield return new WaitUntil(()=> this.allowDisplayingFromContinuation);

            this.allowStoryProceed = true;
            this.allowDisplayingFromContinuation = false;

            StartStory(PlayerPrefs.GetInt(PlayerPrefs.GetString("currentSlot") + "nowCommandIndex") + 1, this.commandFile);    
        }
        
    }

}
