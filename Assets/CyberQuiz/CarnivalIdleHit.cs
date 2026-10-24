using UnityEngine;
using UnityEngine.EventSystems;

namespace CyberQuiz
{
    public class CarnivalIdleHit : MonoBehaviour, IPointerClickHandler
    {
        public CarnivalDisplays displays;
        public void OnPointerClick(PointerEventData eventData)
        {
            if(displays!=null)displays.OpenRegistration();
        }
    }
}
