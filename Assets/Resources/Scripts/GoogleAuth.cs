using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Google;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class GoogleAuth : MonoBehaviour
{
    public Text emailauth, nameauth;
    private GoogleSignInConfiguration configuration;
    public string webClientId ="Y754613247534-6fs4ueotgooi3u21nlp19frdqv2t3hmh.apps.googleusercontent.com";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        configuration = new GoogleSignInConfiguration {
             WebClientId = webClientId, 
             RequestIdToken = true,
             UseGameSignIn = false,
             RequestEmail = true 
             };
    }

    public void SignInWithGoogle()
    {
        GoogleSignIn.Configuration = configuration;
       GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished, TaskScheduler.Default);  
    }
    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.Log("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    Debug.Log("Got Unexpected Exception: " + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            Debug.Log("Canceled");
        }
        else
        {
            StartCoroutine(UpdateUI(task.Result));
        }

    }


    IEnumerator UpdateUI(GoogleSignInUser user)
    {
        Debug.Log("Welcome" + user.DisplayName);
        nameauth.text = user.DisplayName;
        emailauth.text = user.Email;
        yield return null;
    }


}
