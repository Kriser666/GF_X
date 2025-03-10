public partial class ModifySaved : UIFormBase
{
    public override void InitLocalization()
    {
        base.InitLocalization();
        varDesc.text = string.Format(varDesc.text, GF.Localization.GetString("MAIN_MENU.HISTORY_MODI"));
    }
}
