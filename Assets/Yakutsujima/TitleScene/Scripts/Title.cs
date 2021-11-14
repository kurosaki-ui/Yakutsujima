using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    public GameObject loadPanel;

    // Start is called before the first frame update
    void Start()
    {

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

    public void Newgame()
    {
        PlayerPrefs.SetString("currentSlot", "");
        SceneManager.LoadScene("StoryScene");
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

        this.loadPanel.GetComponent<CanvasGroup>().DOFade(1.0f,1.0f);        
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
