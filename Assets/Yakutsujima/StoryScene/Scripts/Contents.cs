﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;


public class Contents : MonoBehaviour
{
    private bool allowDisplayingAllCurrentSentence = false;
    private bool finishDisplayingAllCurrentSentence = false;
    private bool DownMiddle = false;
    private bool DownBottom = false;
    private GameObject icon;
    private GameObject nextIcon;
    private GameObject storyBackground1;
    private GameObject storyBackground2;

    private float x = 0;
    private float y = 0;
    private float characterSpeed = 0.03f;

    private int historyId = 0;
    private int startRow = 0;
    private int nowCommandIndex;
    private int currentHistoryNum;

    private List <HistoryData> historyData;

    private string historyJsonString = "";
    private string jsonString = "";
    private string nowCommandValue = "";
    private string lastTriggerName1 = "";
    private string lastTriggerName2 = "";
    private string lastTriggerName3 = "";
    private string slotName = "";
    private Vector3 StartPosition;

    public class PlayCommand
    {
        public int id;
        public string content;
        public string target1;
    } 
    public class PlayCommandRelation
    {
        public int id;
        public string currentFile;
        public int selectNumber;
        public string nextFile;
    }
    public  class HistoryData
    {
        public int id;
        public string sentence;
    }    

    public GameObject chapterPanel;
    public GameObject historyPanel;
    public GameObject selectPanel;
    public GameObject storyPanel;
    public GameObject menu;
 
    public GameObject storyText;
    public GameObject historyText1;
    private GameObject historyText2;
    private GameObject historyText3;

    public PlayCommand[] commandsDatum;
    public PlayCommandRelation[] commandRelationDatum;
    public HistoryData[] historyDatum;

    public TextAsset commandFile;
    public TextAsset commandRelationFile;


    // Start is called before the first frame update
    void Start()
    {
        this.historyData = new List<HistoryData>();
        this.slotName = PlayerPrefs.GetString("currentSlot");
        if(this.slotName != "")
        {
            LoadData();    
        }
        else
        {
            StartStory(0, this.commandFile);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("セーブ消去");
        }
    }

    private void StartStory(int startRow, TextAsset commandFile)
    {
        CreateJson(commandFile);
        this.commandsDatum = JsonConvert.DeserializeObject<PlayCommand[]>(this.jsonString);

        this.icon = this.storyText.transform.Find("Icon").gameObject;
        this.nextIcon = this.storyText.transform.Find("NextIcon").gameObject;
        this.storyBackground1 = storyPanel.transform.Find("StoryBackground1").gameObject;
        this.storyBackground2 = storyPanel.transform.Find("StoryBackground2").gameObject;
        this.storyText.GetComponent<Text>().text = null;
        
        StartCoroutine(CommandLoop(startRow));
    }

    private IEnumerator CommandLoop(int startRow)
    {
        for(this.nowCommandIndex = startRow; this.nowCommandIndex < this.commandsDatum.Length; this.nowCommandIndex++)
        {   
            this.nowCommandValue = this.commandsDatum[this.nowCommandIndex].target1;
            // Debug.Log("コマンド名" + this.commandsDatum[this.nowCommandIndex].content);           
            yield return StartCoroutine(this.commandsDatum[this.nowCommandIndex].content);  
        } 
    }

    private IEnumerator Background()
    {
        this.storyBackground1.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Yakutsujima/StoryScene/Images/Chapters/" + this.nowCommandValue + ".png");
        yield break;
    }

    private IEnumerator Chapter()
    {
        this.chapterPanel.SetActive(true);
        GameObject chapterBackground = this.chapterPanel.transform.Find("ChapterBackground").gameObject;
        chapterBackground.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Yakutsujima/StoryScene/Images/Chapters/" + this.nowCommandValue + ".png");

        yield return new WaitUntil(()=> this.chapterPanel.activeSelf == false);
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
        
        for(int i = 0; i < currentSentence.Length; i++)
        {
            if (this.allowDisplayingAllCurrentSentence)
            {
                this.storyText.GetComponent<Text>().text += currentSentence.Substring(i, currentSentence.Length - i);
                break;
            }
            else
            {
                yield return new WaitForSeconds(this.characterSpeed);         
                this.storyText.GetComponent<Text>().text += currentSentence.Substring(i,1);            
            }
        } 

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
    
    public IEnumerator Select()
    {
        this.selectPanel.SetActive(true);
        this.storyText.GetComponent<Text>().text = null;
        CreateHistoryData();
        string[] selectrow = this.nowCommandValue.Split('/');
        switch (selectrow.Length)
        {
            case 3:
                GameObject.Find("SelectButtonText1").GetComponentInChildren<Text>().text = selectrow[0];
                GameObject.Find("SelectButtonText2").GetComponentInChildren<Text>().text = selectrow[1];
                GameObject.Find("SelectButtonText3").GetComponentInChildren<Text>().text = selectrow[2];
                break;
            case 2:
                GameObject.Find("SelectButtonText1").GetComponentInChildren<Text>().text = selectrow[0];
                GameObject.Find("SelectButtonText2").GetComponentInChildren<Text>().text = selectrow[1];
                GameObject.Find("SelectButtonText3").SetActive(false);
                break;
            case 1:
                GameObject.Find("SelectButtonText1").GetComponentInChildren<Text>().text = selectrow[0];
                GameObject.Find("SelectButtonText2").SetActive(false);
                GameObject.Find("SelectButtonText3").SetActive(false);
                break;
        } 
        yield break; 
    }

    private IEnumerator NextPage()
    {      
        CreateHistoryData();
        this.storyText.GetComponent<Text>().text = null;
        yield break;
    }

    private IEnumerator Music()
    {
        GameObject.Find("EventSystem").GetComponent<AudioSource>().clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Yakutsujima/StoryScene/Musics/Chapters/" + this.nowCommandValue + ".mp3");
        GameObject.Find("EventSystem").GetComponent<AudioSource>().Play();
        yield break;
    }
    private void CreateHistoryData()
    {
        this.historyData.Add(new HistoryData{id = this.historyId, sentence = this.storyText.GetComponent<Text>().text});
        this.historyJsonString = JsonConvert.SerializeObject(historyData);
        this.historyDatum = JsonConvert.DeserializeObject<HistoryData[]>(this.historyJsonString);
        this.historyId++;   
    }



    private void LoadData(){
        this.historyDatum = JsonConvert.DeserializeObject<HistoryData[]>(PlayerPrefs.GetString("historyJsonString"));

        this.storyPanel.transform.Find("StoryBackground1").gameObject.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Yakutsujima/StoryScene/Images/Chapters/" + PlayerPrefs.GetString("nowBackground") + ".png");
        this.storyPanel.transform.Find("StoryText").gameObject.GetComponent<Text>().text = PlayerPrefs.GetString("displayTextScreen");
        // ログ PlayerPrefs.GetString("nowCommandFile")はChapter1まで保存
        commandFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Yakutsujima/StoryScene/CsvFiles/Chapters/" + PlayerPrefs.GetString("nowCommandFile") + ".csv");
        StartStory(PlayerPrefs.GetInt("nowCommandIndex+1"), commandFile);
    }

    private string FileExchange(int selectButtonNum)
    {
        CreateJson(this.commandRelationFile);
        this.commandRelationDatum = JsonConvert.DeserializeObject<PlayCommandRelation[]>(this.jsonString);
        int[] currentId = null;
        for (int i = 0; i < this.commandRelationDatum.Length ; i++)
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
        return commandRelationDatum[currentId[selectButtonNum - 1]].nextFile;
    }
    

    private void DropHisotryControll()
    {
        if(this.historyPanel.activeSelf == false)
        {
            if(this.historyDatum != null && this.historyDatum.Length > 0)
            {  
                this.storyText.GetComponent<CanvasGroup>().DOFade(0.0f,1f);
                this.historyText1.GetComponent<CanvasGroup>().alpha = 0;
                // this.historyText2.GetComponent<CanvasGroup>().alpha = 0;
                // this.historyText3.GetComponent<CanvasGroup>().alpha = 0;

                this.currentHistoryNum = this.historyDatum.Length-1;

                this.historyText1.gameObject.SetActive(true);

                this.historyPanel.SetActive(true);

                this.lastTriggerName1 = "";
                this.lastTriggerName2 = ""; 

                if(this.lastTriggerName1 == "" || this.lastTriggerName2 == "")
                {
                    this.historyText2 = GameObject.Instantiate(this.historyText1 ,this.historyPanel.transform) as GameObject;

                    this.historyText1.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;
                    this.historyText1.GetComponent<Animator>().SetTrigger("DownMiddleHistoryText");
                    this.historyText1.GetComponent<CanvasGroup>().DOFade(1.0f,0.0f);
                    this.lastTriggerName1 = "DownMiddleHistoryText";                                                                     
                  
                }
            }     
        }
        else if(this.historyPanel.activeSelf == true)
        {
            if(currentHistoryNum != 0)
            {
                // this.historyText1.GetComponent<Animator>().ResetTrigger("DownMiddleHistoryText");
                // this.historyText1.GetComponent<Animator>().ResetTrigger("DownBottomHistoryText");                
                // this.historyText2.GetComponent<Animator>().ResetTrigger("DownMiddleHistoryText");
                // this.historyText2.GetComponent<Animator>().ResetTrigger("DownBottomHistoryText");
                // this.historyText3.GetComponent<Animator>().ResetTrigger("DownMiddleHistoryText");
                // this.historyText3.GetComponent<Animator>().ResetTrigger("DownBottomHistoryText");

                if(this.lastTriggerName1 == "DownMiddleHistoryText" && this.lastTriggerName2 == "" || this.lastTriggerName2 == "DownBottomHistoryText" && this.lastTriggerName3 == ""||this.lastTriggerName3 == "DownBottomHistoryText")
                {
                    Debug.Log(1);
                    this.currentHistoryNum--;
                    this.historyText3 = GameObject.Instantiate(this.historyText2 ,this.historyPanel.transform) as GameObject;

                    this.historyText1.GetComponent<Animator>().SetTrigger("DownBottomHistoryText");
                    this.historyText1.GetComponent<CanvasGroup>().DOFade(0.0f,1f);
                    this.lastTriggerName1 = "DownBottomHistoryText";

                    this.historyText2.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;
                    this.historyText2.GetComponent<Animator>().SetTrigger("DownMiddleHistoryText");
                    this.historyText2.GetComponent<CanvasGroup>().DOFade(1.0f,0.0f);
                    this.lastTriggerName2 = "DownMiddleHistoryText";

                }
                else if(this.lastTriggerName1 == "DownBottomHistoryText" && this.lastTriggerName2 == "DownMiddleHistoryText" && this.lastTriggerName3 ==""||this.lastTriggerName3 == "DownBottomHistoryText")
                {
                    Debug.Log(2);
    
                    this.currentHistoryNum--;
                    Destroy(this.historyText1);
                    this.historyText1 = GameObject.Instantiate(this.historyText3 ,this.historyPanel.transform) as GameObject;

                    this.historyText2.GetComponent<Animator>().SetTrigger("DownBottomHistoryText");
                    this.historyText2.GetComponent<CanvasGroup>().DOFade(0.0f,0.0f);
                    this.lastTriggerName2 = "DownBottomHistoryText";

                    this.historyText3.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;
                    this.historyText3.GetComponent<Animator>().SetTrigger("DownMiddleHistoryText");
                    this.historyText3.GetComponent<CanvasGroup>().DOFade(1.0f,1.0f);
                    this.lastTriggerName3 = "DownMiddleHistoryText";
                    
                }
                else if(this.lastTriggerName1 == "DownBottomHistoryText" && this.lastTriggerName2 == "DownBottomHistoryText" && this.lastTriggerName3 == "DownMiddleHistoryText")
                {
                    Debug.Log(3);
                    this.currentHistoryNum--;
                    Destroy(this.historyText2);
                    this.historyText2 = GameObject.Instantiate(this.historyText1 ,this.historyPanel.transform) as GameObject;

                    this.historyText3.GetComponent<Animator>().SetTrigger("DownBottomHistoryText");
                    this.historyText3.GetComponent<CanvasGroup>().DOFade(0.0f,0.0f);
                    this.lastTriggerName3 = "DownBottomHistoryText";

                    this.historyText1.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;
                    this.historyText1.GetComponent<Animator>().SetTrigger("DownMiddleHistoryText");
                    this.historyText1.GetComponent<CanvasGroup>().DOFade(1.0f,1.0f);
                    this.lastTriggerName1 = "DownMiddleHistoryText"; 

                }

                // if(this.lastTriggerName1 == "DownMiddleHistoryText")
                // {
                //     this.currentHistoryNum--;
                //     this.historyText2.gameObject.SetActive(true);
                //     this.historyText2.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;      

                //     TextAnimation("down", this.historyText2, this.historyText1);

                //     this.historyText1.gameObject.SetActive(false);   
                // }
                // else if(this.historyText2.gameObject.activeSelf == true)
                // {
                //     this.currentHistoryNum--;
                //     this.historyText1.gameObject.SetActive(true); 
                //     this.historyText1.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence; 

                //     TextAnimation("down", this.historyText1, this.historyText2);

                //     this.historyText2.gameObject.SetActive(false);                                                                                        
                // }
            }
        }
    }
    private  void UpHistoryControll()
    {
        if(this.historyPanel.activeSelf == true)
        {
            if(this.historyDatum.Length == this.currentHistoryNum+1)
            {
                this.historyText1.gameObject.SetActive(false);                                
                this.historyText2.gameObject.SetActive(false);                                
                this.historyPanel.SetActive(false);
                this.storyText.GetComponent<CanvasGroup>().DOFade(1.0f,2f);
            }
            else if(this.historyText1.gameObject.activeSelf == true)
            {
                this.currentHistoryNum++;
                this.historyText2.gameObject.SetActive(true);
                this.historyText2.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;  

                // TextAnimation("up", this.historyText2, this.historyText1);

                this.historyText1.gameObject.SetActive(false);
            }
            else if(this.historyText2.gameObject.activeSelf == true)
            {
                this.currentHistoryNum++;
                this.historyText1.gameObject.SetActive(true);
                this.historyText1.GetComponent<Text>().text = this.historyDatum[this.currentHistoryNum].sentence;                                            

                // TextAnimation("up", this.historyText1, this.historyText2);

                this.historyText2.gameObject.SetActive(false); 
            }
        }
    }
    private void TextAnimation(String direction, UnityEngine.UI.Text text1 = null, UnityEngine.UI.Text text2 = null)
    {
        if(direction == "down")
        {            
            if (text1 != null) {
                text1.GetComponent<Animator>().SetTrigger("DownMiddleHistoryText");
                this.lastTriggerName1 = "DownMiddleHistoryText";
                text1.GetComponent<CanvasGroup>().DOFade(1.0f,2.0f);
            }

            if (text2 != null) {
                text2.GetComponent<Animator>().SetTrigger("DownBottomHistoryText");
                this.lastTriggerName2 = "DownBottomHistoryText";
                text2.GetComponent<CanvasGroup>().DOFade(0.0f,2.0f);
            }
        }
        else if(direction == "up")
        {
            if (text1 != null) {
                text1.GetComponent<Animator>().SetTrigger("UpMiddleHistoryText");
                text1.GetComponent<CanvasGroup>().DOFade(1.0f,2.0f);
            }

            if (text2 != null) {
                text2.GetComponent<Animator>().SetTrigger("UpTopHistoryText");
                text2.GetComponent<CanvasGroup>().DOFade(0.0f,2.0f);
            }
        }
    }
    
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

    public void ReStartStory(int selectButtonNum)
    {
        FileExchange(selectButtonNum);
        this.commandFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Yakutsujima/StoryScene/CsvFiles/Chapters/" + FileExchange(selectButtonNum) + ".csv");
        StartStory(0, this.commandFile);
    }
    public void ClickSentence() 
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
    public void BeginDrag()
    {
        this.StartPosition = Input.mousePosition;        
    }
    public void EndDrag()
    {
        Vector2 Distance = Input.mousePosition - this.StartPosition;

        if(Distance.y < -300)
        {
            DropHisotryControll();
        }
        else if (Distance.y > 100)
        {
            UpHistoryControll();           
        }
    }
}