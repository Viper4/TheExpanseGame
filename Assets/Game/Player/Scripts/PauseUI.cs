using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    VRControl control;
    [SerializeField] LayerMask pausedCullingMask;
    [SerializeField] LayerMask normalCullingMask;
    [SerializeField] Camera UICamera;
    [SerializeField] GameObject pauseMenu;
    Animator animator;
    bool settings = false;

    [SerializeField] Transform settingsParent;

    void Awake()
    {
        control = GetComponent<VRControl>();
        animator = pauseMenu.GetComponent<Animator>();
    }

    void Update()
    {
        if (VRControl.instance.inputActions.UI.Pause.WasPressedThisFrame())
        {
            if (pauseMenu.activeSelf)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        UICamera.cullingMask = normalCullingMask;
        control.Paused = false;
        pauseMenu.SetActive(false);
    }

    public void Pause()
    {
        UICamera.cullingMask = pausedCullingMask;
        control.Paused = true;
        pauseMenu.transform.SetPositionAndRotation(UICamera.transform.position + UICamera.transform.forward, FlatCamera.instance.transform.rotation);
        pauseMenu.SetActive(true);
        animator.SetTrigger("PauseIn");
        settings = false;
    }

    public void Settings()
    {
        animator.SetTrigger("SettingsIn");
        settings = true;
        foreach(Transform child in settingsParent)
        {
            switch (child.name)
            {
                case "Grab Toggle":
                    break;
            }
        }
    }

    public void Back()
    {
        if (settings)
        {
            animator.SetTrigger("SettingsOut");
        }
        settings = false;
    }

    public void Exit()
    {
        Application.Quit();
    }
}
