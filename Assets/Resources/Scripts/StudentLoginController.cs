using UnityEngine;

public class StudentLoginController : MonoBehaviour
{
    public SceneManagerScript sceneManager;

    void Start()
    {

    }

    public void OnRegisterButtonClicked()
    {
        sceneManager.LoadSceneByName("RegistrationPage");
    }
    
}
