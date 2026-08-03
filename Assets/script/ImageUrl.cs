using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class ImageURL : MonoBehaviour
{
    private string imageUrl =
        "https://drive.google.com/uc?export=download&id=19B5wwAu1dj4GsLdI8B9klEy_vjvfI6Z";

    public Image targetImage;


    void Start()
    {
        StartCoroutine(LoadImage());
    }

    IEnumerator LoadImage()
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Image load error: " + request.error);
            }
            else
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(request);

                targetImage.sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
        }
    }
}