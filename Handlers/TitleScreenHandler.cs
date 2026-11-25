using Photon.Realtime;
using SG.Airlock.UI.TitleScreen;
using UnityEngine;

namespace AirlockClient.Handlers;

public class TitleScreenHandler : MonoBehaviour
{
    private bool loadSplash = true;
    
    void Start()
    {
        
    }

    void Update()
    {
        var checkmenu = FindObjectOfType<SG.Airlock.UI.TitleScreen.CheckInternetConnection>();
        var titlehandler = FindObjectOfType<TitleStartup>();
        if (loadSplash)
        {
            if (checkmenu != null)
            {
                checkmenu.gameObject.SetActive(false);
                if (titlehandler != null)
                {
                    titlehandler._menuManager._menus[27].gameObject.SetActive(true);
                    loadSplash = false;
                }
            }
        }

        if (titlehandler != null)
        {
            if (titlehandler._menuManager._menus[27].gameObject.active && titlehandler._menuManager._menus[28].gameObject.active)
            {
                titlehandler._menuManager._menus[28].gameObject.SetActive(false);
            }
            if (titlehandler._menuManager._menus[0].gameObject.active && titlehandler._menuManager._menus[27].gameObject.active)
            {
                titlehandler._menuManager._menus[27].gameObject.SetActive(false);
            }
        }
    }
}