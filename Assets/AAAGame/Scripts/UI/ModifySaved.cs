using UnityGameFramework.Runtime;

public partial class ModifySaved : UIFormBase
{
    public override void InitLocalization()
    {
        base.InitLocalization();
        varDesc.text = string.Format(varDesc.text, GF.Localization.GetString("MAIN_MENU.HISTORY_MODI"));
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        // 关闭改装页面
        GF.UI.CloseUIForms(UIViews.ModifyGame);
        // GF.UI.RefocusUIForm(menuUIForm);
        // GF.UI.CloseUIForms(UIViews.ModifyGame);
        base.OnClose(isShutdown, userData);
    }
}
