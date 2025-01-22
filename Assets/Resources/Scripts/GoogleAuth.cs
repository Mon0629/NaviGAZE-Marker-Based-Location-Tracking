using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Google;
using System.Threading.Tasks;
using Firebase.Extensions;
using System;

public class GoogleAuth : MonoBehaviour
{
    public Text emailauth, nameauth;
    private GoogleSignInConfiguration configuration;
    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    public string webClientId = "754613247534-gcommev8bu0htsj1r8pafnsj2ud56pu5.apps.googleusercontent.com";

    void Awake()
    {
        // Configure Google Sign-In
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true
        };

        // Initialize Firebase
        FirebaseInit();
    }

    private void FirebaseInit()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    }

    // Method to be called when the button is clicked
    public void SignInWithGoogle()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestEmail = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.LogError("Google Sign-In Failed");
        }
        else if (task.IsCanceled)
        {
            Debug.LogWarning("Google Sign-In Canceled");
        }
        else
        {
            Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
            {
                if (authTask.IsCanceled)
                {
                    Debug.LogWarning("Firebase Sign-In Canceled");
                }
                else if (authTask.IsFaulted)
                {
                    Debug.LogError("Firebase Sign-In Error: " + authTask.Exception);
                }
                else
                {
                    user = auth.CurrentUser;
                    emailauth.text = user.Email;
                    nameauth.text = user.DisplayName;
                    Debug.Log($"Firebase Sign-In Success: {user.DisplayName} ({user.Email})");
                }
            });
        }
    }
}
