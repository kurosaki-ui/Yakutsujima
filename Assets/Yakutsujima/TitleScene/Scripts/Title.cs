using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Title : MonoBehaviour
{
    private Tween startButtonFade;
    private Tween loadButtonFade;
    private Tween loadPanelFade;
    public GameObject defaultPanel;
    public GameObject loadPanel;
    public GameObject operationMethodPanel;
    public GameObject startButton;
    public GameObject loadButton;

    // Start is called before the first frame update
    void Start()
    {
        this.loadPanel.SetActive(false);
        this.loadPanel.GetComponent<CanvasGroup>().alpha = 0;
        this.operationMethodPanel.SetActive(false);
        this.operationMethodPanel.GetComponent<CanvasGroup>().alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("セーブ消去");
        }

        if(this.operationMethodPanel.GetComponent<CanvasGroup>().alpha == 1)
        {
            PlayerPrefs.SetString("currentSlot", "");
            SceneManager.LoadScene("StoryScene");
        }
    }

    public void Newgame()
    {
        this.operationMethodPanel.SetActive(true);
        this.operationMethodPanel.GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f);
    }

    public void OpenLoadPanel()
    {
        this.loadPanel.SetActive(true);

        for(int i = 1; i <= 4; i++)
        {
            if(PlayerPrefs.GetString("Slot" + i + "time") == "")
            {
                GameObject.Find("LoadSlot" + i).GetComponent<Text>().text = " ---- / -- / --   -- : -- : --";
            }else
            {
                GameObject.Find("LoadSlot" + i).GetComponent<Text>().text = PlayerPrefs.GetString("Slot" + i + "time");             
            }
        }
        this.startButtonFade.Kill();
        this.loadButtonFade.Kill();
        this.startButton.GetComponent<CanvasGroup>().alpha = 0;
        this.loadButton.GetComponent<CanvasGroup>().alpha = 0;
        this.loadPanelFade = this.loadPanel.GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f);        
    }

    public void CloseLoadPanel()
    {
        this.startButtonFade = this.startButton.GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f);
        this.loadButtonFade = this.loadButton.GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f);
        this.loadPanel.SetActive(false);
        this.loadPanelFade.Kill();
        this.loadPanel.GetComponent<CanvasGroup>().alpha = 0;        
    }

    public void Load(string slotName)
    {
        if(PlayerPrefs.GetString(slotName + "time") != "")
        {
            PlayerPrefs.SetString("currentSlot",slotName);
            SceneManager.LoadScene("StoryScene");
        }
    }
}
