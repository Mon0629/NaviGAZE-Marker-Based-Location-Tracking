using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;

public class AddScheduleController : MonoBehaviour
{
    private DatabaseReference dbReference;

    public InputField subjectCode;    
    public InputField subjectName;    
    public InputField room;          
    public Dropdown dayOfTheWeek; 
    public Dropdown campus;       

    public GameObject rowTemplate;     
    public Transform tableContainer; 

    private List<ScheduleData> scheduleList = new List<ScheduleData>();
    string userId = UserSession.UserId;
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                string databaseUrl = "https://navigaze-29230-default-rtdb.asia-southeast1.firebasedatabase.app/"; 
                dbReference = FirebaseDatabase.GetInstance(app, databaseUrl).RootReference;
                Debug.Log("Firebase Initialized Successfully");
                
                Debug.Log("User ID: " + userId);
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });
    }

 
    public void OnAddScheduleButtonClicked()
    {
    
        if (string.IsNullOrEmpty(subjectCode.text) || 
            string.IsNullOrEmpty(subjectName.text) || 
            string.IsNullOrEmpty(room.text))
        {
            Debug.LogWarning("Please fill in all input fields!");
            return;
        }

        GameObject newRow = Instantiate(rowTemplate, tableContainer);
        newRow.SetActive(true); 

      
        TextMeshProUGUI[] rowColumns = newRow.GetComponentsInChildren<TextMeshProUGUI>();

        if (rowColumns.Length >= 5)
        {
            rowColumns[0].text = subjectCode.text;                                 // Subject Code
            rowColumns[1].text = subjectName.text;                                // Subject Name
            rowColumns[2].text = room.text;                                       // Room
            rowColumns[3].text = dayOfTheWeek.options[dayOfTheWeek.value].text;   // Day of the Week
            rowColumns[4].text = campus.options[campus.value].text;               // Campus
        }
        else
        {
            Debug.LogError("Row template does not have enough columns to populate data.");
        }

        ScheduleData schedule = new ScheduleData(
            subjectCode.text,
            subjectName.text,
            room.text,
            dayOfTheWeek.options[dayOfTheWeek.value].text,
            campus.options[campus.value].text
        );

        scheduleList.Add(schedule); 
        ClearInputFields();
    }

    public void OnSaveButtonClicked()
    {
        if (scheduleList.Count == 0)
        {
            Debug.LogWarning("No schedule data to save!");
            return;
        }
        
        foreach (var schedule in scheduleList)
        {
            SaveToDatabase(schedule);
        }
        scheduleList.Clear();
        ClearTable();
    }

private void SaveToDatabase(ScheduleData schedule)
{
    if (dbReference == null)
    {
        Debug.LogError("Database reference is not initialized.");
        return;
    }
    
    if (string.IsNullOrEmpty(userId))
    {
        Debug.LogError("No user is logged in. Cannot associate schedule.");
        return;
    }

    Dictionary<string, object> scheduleData = new Dictionary<string, object>
    {
        { "subjectCode", schedule.subjectCode },
        { "subjectName", schedule.subjectName },
        { "room", schedule.room },
        { "dayOfTheWeek", schedule.dayOfTheWeek },
        { "campus", schedule.campus }
    };

    // Save the schedule under the user's ID
        dbReference.Child("users").Child(userId).Child("schedules").Push().SetValueAsync(scheduleData).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
     {
         Debug.Log("Schedule data saved to Firebase under user: " + userId);
     }
        else
        {
        Debug.LogError("Failed to save schedule data: " + task.Exception);
        }
    });
}

    private void ClearInputFields()
    {
        subjectCode.text = "";
        subjectName.text = "";
        room.text = "";
        dayOfTheWeek.value = 0;
        campus.value = 0;
    }

    private void ClearTable()
    {
        foreach (Transform child in tableContainer)
        {
            Destroy(child.gameObject);
        }
    }
}

// Class to hold schedule data (structure)
[System.Serializable]
public class ScheduleData
{
    public string subjectCode;
    public string subjectName;
    public string room;
    public string dayOfTheWeek;
    public string campus;

    public ScheduleData(string subjectCode, string subjectName, string room, string dayOfTheWeek, string campus)
    {
        this.subjectCode = subjectCode;
        this.subjectName = subjectName;
        this.room = room;
        this.dayOfTheWeek = dayOfTheWeek;
        this.campus = campus;
    }
}
