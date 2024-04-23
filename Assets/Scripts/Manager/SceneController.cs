using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : Singleton<SceneController>
{
    private GameObject player;

    private bool isPortal;
    private bool startIsCalled;
    private Portal.TransitionType transitionType;
    private Portal.TransitionID transitionID;
    private Portal portal;
    

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        player = GameManager.Instance.player;
        GameManager.Instance.PlayerInstantiated += SetPlayerTransform;
        startIsCalled = true;
    }

    private void OnEnable()
    {
        if (startIsCalled)
            GameManager.Instance.PlayerInstantiated += SetPlayerTransform;
    }

    private void OnDisable()
    {
        if (GameManager.IsInitialized)
            GameManager.Instance.PlayerInstantiated -= SetPlayerTransform;
    }
    
    public void Transfer(Portal portal)
    {
        transitionType = portal.transitionType;
        this.portal = portal;
        transitionID = portal.transitionID;
        switch (transitionType)
        {
            case Portal.TransitionType.SameScene:
                switch (portal.portalType)
                {
                    case Portal.PortalType.From:
                        Transform temp1;
                        if (GetPortalTransform(transitionType , transitionID , portal, out temp1))
                        {
                            player.GetComponent<NavMeshAgent>().ResetPath();
                            player.transform.SetPositionAndRotation(temp1.position , player.transform.rotation);
                        }
                        break;
                    case Portal.PortalType.ToAndFrom:
                        Transform temp2;
                        if (GetPortalTransform(transitionType , transitionID , portal, out temp2))
                        {
                            player.GetComponent<NavMeshAgent>().ResetPath();
                            player.transform.SetPositionAndRotation(temp2.position , player.transform.rotation);
                        }
                        break;
                    case Portal.PortalType.To:
                        break;
                }
                break;
            case Portal.TransitionType.DifferentScene:
                switch (portal.portalType)
                {
                    case Portal.PortalType.From:
                        isPortal = true;
                        StartCoroutine(LoadScene_ProgressBar(portal.destinationScene));
                        break;
                    case Portal.PortalType.ToAndFrom:
                        isPortal = true;
                        StartCoroutine(LoadScene_ProgressBar(portal.destinationScene));
                        break;
                    case Portal.PortalType.To:
                        break;
                }
                break;
        }
    }

    private bool GetPortalTransform(Portal.TransitionType transitionType , Portal.TransitionID transitionID 
        , Portal portal , out Transform transformRef)
    {
        var portals = FindObjectsOfType<Portal>();
        switch (transitionType)
        {
            case Portal.TransitionType.SameScene:
                foreach (var item in portals)
                {
                    if (portal != null)
                    {
                        if ((item.transitionID == transitionID) && (item != portal))
                        {
                            if ((item.portalType == Portal.PortalType.To) || (item.portalType == Portal.PortalType.ToAndFrom))
                            {
                                transformRef = item.portalPos.transform;
                                return true;
                            }
                        }
                    }
                }
                break;
            case Portal.TransitionType.DifferentScene:
                foreach (var item in portals)
                {
                    if (item.transitionID == transitionID)
                    {
                        if ((item.portalType == Portal.PortalType.To) || (item.portalType == Portal.PortalType.ToAndFrom))
                        {
                            transformRef = item.portalPos.transform;
                            return true;
                        }
                    }
                }
                break;
        }
        transformRef = null;
        return false;
    }

    public IEnumerator LoadScene_ProgressBar(String sceneName)
    {
        if((sceneName == SceneManager.GetActiveScene().name) || (sceneName == null))
            yield break;
        GameManager.Instance.SaveAllData();

        GameObject loadSceneUI;
        GameObject progressBar;
        Image currentProgress;
        UIManager.Instance.GetReference_LoadSceneUI(out loadSceneUI);
        progressBar = loadSceneUI.transform.GetChild(0).gameObject;
        currentProgress = progressBar.transform.GetChild(0).GetComponent<Image>();
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (true)
        {
            if (operation.isDone)
            {
                UIManager.Instance.BreakReference_LoadSceneUI(ref loadSceneUI);
                break;
            }
            else
            {
                currentProgress.fillAmount = operation.progress;
                yield return null;
            }
        }
    }

    public IEnumerator LoadScene_FadeInAndOut(String sceneName, UIManager.FadeInAndOutColor fadeInColor, 
        UIManager.FadeInAndOutColor fadeOutColor , float fadeInDudation, float fadeOutDuration ,
        bool fadeInActive = true , bool fadeOutActive = true)
    {
        if((sceneName == SceneManager.GetActiveScene().name) || (sceneName == null))
            yield break;
        GameManager.Instance.SaveAllData();

        if(fadeInActive)
            yield return StartCoroutine(UIManager.Instance.FadeIn(fadeInColor , fadeInDudation));
        //TODO fix this problem
        //yield return SceneManager.LoadSceneAsync(sceneName);
        SceneManager.LoadSceneAsync(sceneName);
        if(fadeOutActive)
            yield return StartCoroutine(UIManager.Instance.FadeOut(fadeOutColor, fadeOutDuration));
    }

    private void SetPlayerTransform()
    {
        player = GameManager.Instance.player;
        if (isPortal)
        {
            Transform temp;
            if (GetPortalTransform(transitionType, transitionID, portal, out temp))
            {
                player.transform.position = temp.position;
                player.transform.rotation = temp.rotation;
                isPortal = false;
            }
        }
    }
}
