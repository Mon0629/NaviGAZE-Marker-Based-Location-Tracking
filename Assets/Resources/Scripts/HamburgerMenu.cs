using UnityEngine;

public class HamburgerMenu : MonoBehaviour
{
    public GameObject menuPanel; 
    private bool isMenuVisible = false;

   
    public void ToggleMenu()
    {
        isMenuVisible = !isMenuVisible;  
        menuPanel.SetActive(isMenuVisible);  
    }
}
