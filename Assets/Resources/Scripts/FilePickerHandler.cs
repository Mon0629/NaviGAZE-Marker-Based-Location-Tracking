using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class FilePickerHandler : MonoBehaviour
{
    public Image displayImage; // Drag your Image UI element here

    public void OpenFileExplorer()
    {
        if (!NativeFilePicker.CanPickMultipleFiles())
        {
            Debug.Log("Native File Picker is not supported on this device.");
            return;
        }

        NativeFilePicker.PickFile((path) =>
        {
            if (path != null)
            {
                Debug.Log("File picked: " + path);

                // Load the selected image
                byte[] imageData = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);

                // Display the image in the UI
                Sprite imageSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                displayImage.sprite = imageSprite;
            }
            else
            {
                Debug.Log("File picking was canceled.");
            }
        }, new string[] { "image/png", "image/jpeg" });
    }
}
