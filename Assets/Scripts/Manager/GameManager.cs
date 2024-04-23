using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
    [Header("Game Settings")]
    
    [Tooltip("Default enemy refreshing time")]
    public float EnemyRefreshTime;

    public GameObject playerPrefab;

    [HideInInspector]
    public GameObject player;

    [HideInInspector]
    public CameraController cameraController;
    
    [HideInInspector]
    public CharacterStats playerStats;
    
    [HideInInspector]
    public PlayerController playerController;

    [HideInInspector]
    public MouseManager mouseManager;

    [HideInInspector]
    public SceneController sceneController;

    [HideInInspector]
    public UIManager uiManager;

    public event Action PlayerInstantiated;
    
    

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);
        mouseManager = FindObjectOfType<MouseManager>();
        sceneController = FindObjectOfType<SceneController>();
        uiManager = FindObjectOfType<UIManager>();
        cameraController = FindObjectOfType<CameraController>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
    }
    
    private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name != "StartScene")
        {
            //TODO
            UIManager.Instance.worldCanvas.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            
            //instantiate player, and get reference
            player = Instantiate(playerPrefab);
            playerStats = player.GetComponent<CharacterStats>();
            playerController = player.GetComponent<PlayerController>();
            
            //enable cameraController and mouseManager 
            cameraController.enabled = true;
            cameraController.transform.GetChild(0).GetChild(0).GetComponent<Camera>().enabled = true;
            mouseManager.enabled = true;

            //set player position and rotation
            if (LoadAllData())
            {
                player.transform.position = playerController.playerGameStatsData.previousPosition;
                player.transform.rotation = playerController.playerGameStatsData.previousRotation;
            }
            else if (GameObject.FindWithTag("Born"))
            {
                Transform bornTransform = GameObject.FindWithTag("Born").transform;
                player.transform.position = bornTransform.position;
                player.transform.rotation = bornTransform.rotation;
            }
            else
            {
                player.transform.position = Vector3.zero;
                player.transform.rotation = Quaternion.identity;
            }
            
            //inform other gameObjects to refresh the reference to player
            PlayerInstantiated?.Invoke();
        }
        else
        {
            //TODO
            UIManager.Instance.worldCanvas.GetComponent<RectTransform>().localScale = new Vector3(0.023595f, 0.023595f, 0.023595f);
            
            player = GameObject.FindWithTag("Player");
            playerStats = player.GetComponent<CharacterStats>();
            playerController = player.GetComponent<PlayerController>();
            
            //disable cameraController and mouseManager
            cameraController.enabled = false;
            cameraController.transform.GetChild(0).GetChild(0).GetComponent<Camera>().enabled = false;
            mouseManager.enabled = false;
        }
        uiManager.worldCanvas.worldCamera = Camera.main;
    }

    private void OnApplicationQuit()
    {
        playerController.playerGameStatsData.previousPosition = player.transform.position;
        playerController.playerGameStatsData.previousRotation = player.transform.rotation;
        playerController.playerGameStatsData.previousSceneName = SceneManager.GetActiveScene().name;
        
        SaveAllData();
    }

    #region DataSavingAndLoading
    
    public void SaveData(string key, Object data)
    {
        var jsonData = JsonUtility.ToJson(data , true);
        PlayerPrefs.SetString(key , jsonData);
        PlayerPrefs.Save();
    }

    public bool LoadData(string key, Object data)
    {
        if (PlayerPrefs.HasKey(key))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);
            return true;
        }
        return false;
    }

    public void SavePlayerData()
    {
        if (SceneManager.GetActiveScene().name != "StartScene")
        {
            String key1 = player.name + playerStats.characterBaseStatsData.name;
            SaveData(key1 , playerStats.characterBaseStatsData);
            
            String key2 = player.name + playerController.playerGameStatsData.name;
            SaveData(key2 , playerController.playerGameStatsData);
        }
    }

    public bool LoadPlayerData()
    {
        String key1 = player.name + playerStats.characterBaseStatsData.name;
        bool temp1 = LoadData(key1 , playerStats.characterBaseStatsData);
        String key2 = player.name + playerController.playerGameStatsData.name;
        bool temp2 = LoadData(key2 , playerController.playerGameStatsData);
        return (temp1 && temp2);
    }

    public void SaveEnemyData()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            String key = SceneManager.GetActiveScene().name + enemy.name + 
                         enemy.GetComponent<CharacterStats>().characterBaseStatsData.name;
            SaveData(key , enemy.GetComponent<CharacterStats>().characterBaseStatsData);
        }
    }

    public bool LoadEnemyData()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies == null)
            return false;
        foreach (var enemy in enemies)
        {
            String key = SceneManager.GetActiveScene().name + enemy.name + 
                         enemy.GetComponent<CharacterStats>().characterBaseStatsData.name;
            LoadData(key , enemy.GetComponent<CharacterStats>().characterBaseStatsData);
        }
        return true;
    }

    public void SaveAllData()
    {
        SavePlayerData();
        SaveEnemyData();
    }

    public bool LoadAllData()
    {
        bool temp1 = LoadPlayerData();
        bool temp2 = LoadEnemyData();
        return (temp1 && temp2);
    }

    
    
    
    
    
    //for game testing only
    //for game testing only
    //for game testing only
    //for game testing only
    //for game testing only
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            CharacterStats.TakeDamage(true , playerStats.CurrentHealth , playerStats);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("Started" , 1);
            
            StartCoroutine(SceneController.Instance.LoadScene_FadeInAndOut("MainScene" , 
                UIManager.FadeInAndOutColor.White , UIManager.FadeInAndOutColor.White , 
                1f , 1f));
        }
    }
    //for game testing only
    //for game testing only
    //for game testing only
    //for game testing only
    //for game testing only

    #endregion
}
