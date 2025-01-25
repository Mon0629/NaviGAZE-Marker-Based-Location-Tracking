using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class ShopController : MonoBehaviour
{

    public GameObject itemPrefab; // Assign the prefab in the Inspector
    public Transform contentParent; // Assign the Content object of the Scroll View

    private DatabaseReference dbReference;

    void Start()
    {
        itemPrefab.SetActive(false);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                LoadItemsFromDatabase(); 
            }
            else
            {
                Debug.LogError("Could not resolve Firebase dependencies.");
            }
        });
    }

    void LoadItemsFromDatabase()
    {
        dbReference.Child("items").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot item in snapshot.Children)
                {
                    string itemName = item.Child("itemName").Value.ToString();
                    string itemPrice = item.Child("itemPrice").Value.ToString();

                    CreateUIItem(itemName, itemPrice);
                }
            }
            else
            {
                Debug.LogError("Failed to fetch items from Firebase.");
            }
        });
    }

    void CreateUIItem(string name, string price)
    {
        GameObject newItem = Instantiate(itemPrefab, contentParent);

        newItem.SetActive(true);

        Text[] texts = newItem.GetComponentsInChildren<Text>();
        foreach (Text text in texts)
        {
            if (text.name == "ItemName")
            {
                text.text = name; 
            }
            else if (text.name == "ItemPrice")
            {
                text.text = "$" + price; 
            }
        }

        Sprite itemImage = Resources.Load<Sprite>($"Assets/{name}");  


        Debug.Log($"Attempting to load image for {name} from path: {name}");


        if (itemImage != null)
        {
            Image itemImageComponent = newItem.transform.Find("ItemImage").GetComponent<Image>();
            itemImageComponent.sprite = itemImage; 
        }
        else
        {
            Debug.LogWarning($"Image for {name} not found. Using placeholder.");
            // Optionally, set a placeholder image if the item image is not found
            Sprite placeholderImage = Resources.Load<Sprite>("placeholder"); // Assuming you have a placeholder image
            if (placeholderImage != null)
            {
                Image itemImageComponent = newItem.transform.Find("ItemImage").GetComponent<Image>();
                itemImageComponent.sprite = placeholderImage;
            }
        }

    }

}
