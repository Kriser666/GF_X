using UnityEngine.EventSystems;

public class RawImageZoneChecker : UIItemBase, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsPointerInZone { get; private set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerInZone = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerInZone = false;
    }
}