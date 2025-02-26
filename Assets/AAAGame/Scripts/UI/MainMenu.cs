using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public partial class MainMenu : UIFormBase
{
    MenuProcedure procedure;
    bool lvReady;

    public TMP_Dropdown.DropdownEvent DropdownChangedEvent { get; private set; }

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        DropdownChangedEvent = new();
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        ChangeScreenOrientation(false);
        procedure = GF.Procedure.CurrentProcedure as MenuProcedure;
        lvReady = false;
        varGameStart.enabled = false;
        varGameStart.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GF.Localization.GetString("MAIN_MENU.LOADING");
        DropdownChangedEvent.AddListener(DropDownChanged);
        varDropdown.onValueChanged = DropdownChangedEvent;
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        DropdownChangedEvent.RemoveListener(DropDownChanged);
        ChangeScreenOrientation(true);
        base.OnClose(isShutdown, userData);
    }

    private void DropDownChanged(int arg0)
    {
        Log.Debug($"{GetType()} /DropDownChanged=> {arg0}");
    }

    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        if (!lvReady)
        {
            if (procedure.LevelEntity != null && procedure.LevelEntity.IsAllReady)
            {
                varGameStart.enabled = true;
                lvReady = true;
                varGameStart.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GF.Localization.GetString("MAIN_MENU.GAME_START");
            }
        }
    }

    protected override void OnButtonClick(object sender, string btId)
    {
        base.OnButtonClick(sender, btId);
        switch(btId)
        {
            case "HistoryModification":
                OpenSubUIForm(UIViews.SettingDialog);
                break;
        }
    }

    protected override void OnButtonClick(object sender, Button btSelf)
    {
        base.OnButtonClick(sender, btSelf);
        if (btSelf == varGameStart)
        {
            procedure.EnterGame();
        }
    }

    public void ChangeScreenOrientation(bool isLandscape)
    {
        if (isLandscape)
        {
            // 切换到横屏
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
        }
        else
        {
            // 切换到竖屏
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }
    }
}
