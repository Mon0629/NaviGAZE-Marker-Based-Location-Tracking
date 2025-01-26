using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class AccessoriesController : MonoBehaviour
{

    public GameObject itemPrefab; // Assign the prefab in the Inspector
    public Transform contentParent; // Assign the Content object of the Scroll View

    private DatabaseReference dbReference;
    private int userCoins; // Track the user's current coins
    private string userId; // Dynamically fetched user ID

    void Start()
    {

        userId = UserSession.UserId; // Fetch the current user ID from the session
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set! Ensure the user is logged in or registered.");
            return;
        }

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

        // Automatically set up the OnClick event for the button
        Button buyButton = newItem.transform.Find("BuyItem").GetComponent<Button>();
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();  // Clear existing listeners

            // Add a listener to the button's OnClick event, passing the item name automatically
            buyButton.onClick.AddListener(() => BuyItem(name));
        }

    }

    public void BuyItem(string itemName)
    {
        // Define paths in the Firebase database
        string itemsPath = "items";
        string userCoinsPath = $"users/{userId}/userCoins"; // Correct path to the user's coin balance
        string userInventoryPath = $"users/{userId}/inventory"; // Correct path to the user's inventory

        // Fetch item details and user coins
        dbReference.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snapshot = task.Result;

                // Find the item in the database based on itemName
                DataSnapshot itemSnapshot = null;
                foreach (var item in snapshot.Child(itemsPath).Children)
                {
                    if (item.Child("itemName").Value.ToString() == itemName)
                    {
                        itemSnapshot = item;
                        break;
                    }
                }

                if (itemSnapshot == null)
                {
                    Debug.LogError($"Item with name '{itemName}' not found in the database.");
                    return;
                }

                int itemPrice = int.Parse(itemSnapshot.Child("itemPrice").Value.ToString());

                // Retrieve the user's coins from the correct path
                DataSnapshot userCoinsSnapshot = snapshot.Child("users").Child(userId).Child("userCoins");
                if (!userCoinsSnapshot.Exists)
                {
                    Debug.LogError("User's coin balance not found in the database.");
                    return;
                }
                int userCoins = int.Parse(userCoinsSnapshot.Value.ToString());

                // Check if the user has enough coins
                if (userCoins >= itemPrice)
                {
                    // Deduct the item's price from the user's coins
                    int newCoinBalance = userCoins - itemPrice;
                    dbReference.Child(userCoinsPath).SetValueAsync(newCoinBalance);

                    // Check if the inventory exists
                    if (!snapshot.Child(userInventoryPath).Exists)
                    {
                        // If inventory doesn't exist, create a new inventory with the item as "Item 1: itemName"
                        Dictionary<string, string> newInventory = new Dictionary<string, string> {
                        { "Item 1", itemName }
                        };
                        dbReference.Child(userInventoryPath).SetValueAsync(newInventory).ContinueWith(createTask =>
                        {
                            if (createTask.IsCompleted)
                            {
                                Debug.Log($"Inventory created and item '{itemName}' added successfully!");
                            }
                            else
                            {
                                Debug.LogError("Failed to create inventory: " + createTask.Exception);
                            }
                        });
                    }
                    else
                    {
                        // If inventory exists, add the new item to the inventory with the next available index
                        Dictionary<string, string> inventory = new Dictionary<string, string>();

                        // Fetch the current inventory items and add the new item
                        int index = 1;
                        foreach (var item in snapshot.Child(userInventoryPath).Children)
                        {
                            inventory.Add($"Item {index}", item.Value.ToString());
                            index++;
                        }

                        // Add the new item with the next index
                        inventory.Add($"Item {index}", itemName);

                        // Update the inventory in Firebase
                        dbReference.Child(userInventoryPath).SetValueAsync(inventory).ContinueWith(updateTask =>
                        {
                            if (updateTask.IsCompleted)
                            {
                                Debug.Log($"Item '{itemName}' purchased and added to inventory successfully!");
                            }
                            else
                            {
                                Debug.LogError("Failed to update inventory: " + updateTask.Exception);
                            }
                        });
                    }
                }
                else
                {
                    Debug.Log("Insufficient Coins!");
                }
            }
            else
            {
                Debug.LogError("Error fetching data from Firebase: " + task.Exception);
            }
        });
    }



}
