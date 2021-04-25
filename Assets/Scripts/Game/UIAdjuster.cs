using CCE.Core;
using UnityEngine;

namespace CCE.Game
{
    internal static class UIAdjuster
    {
        public static void AdjustToResolution()
        {
            float aspectRatio = GlobalState.AspectRatio;
            const float normalAspectRatio = GlobalState.NormalAspectRatio;
            float playAreaWidth = GlobalState.PlayAreaWidth;
            float playAreaHeight = GlobalState.PlayAreaHeight;

            // Adjust for different aspect ratios
            GameObject.Find("PlayAreaBorder").GetComponent<SpriteRenderer>().size =
                new Vector2(playAreaWidth, playAreaHeight);
            
            GameObject.Find("Scanline").GetComponent<SpriteRenderer>().size = new Vector2(playAreaWidth, 0.1f);

            GameObject levelOptionsButton = GameObject.Find("LevelOptionsButton");
            GameObject editorSettingsButton = GameObject.Find("EditorSettingsButton");
            GameObject saveButton = GameObject.Find("SaveButton");

            GameObject.Find("ChartSelectButton").GetComponent<RectTransform>().sizeDelta = 
                new Vector3(200 + 1.5f * (200 * aspectRatio / normalAspectRatio - 200), 70);

            levelOptionsButton.GetComponent<RectTransform>().sizeDelta =
                new Vector3(200 + 1.5f * (200 * aspectRatio / normalAspectRatio - 200), 70);

            editorSettingsButton.GetComponent<RectTransform>().sizeDelta =
                new Vector3(200 + 1.5f * (200 * aspectRatio / normalAspectRatio - 200), 70);
            
            saveButton.GetComponent<RectTransform>().sizeDelta =
                new Vector3(200 + 1.5f * (200 * aspectRatio / normalAspectRatio - 200), 70);
            
            levelOptionsButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                levelOptionsButton.GetComponent<RectTransform>().anchoredPosition.x * aspectRatio / normalAspectRatio,
                levelOptionsButton.GetComponent<RectTransform>().anchoredPosition.y);
            
            editorSettingsButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                editorSettingsButton.GetComponent<RectTransform>().anchoredPosition.x * aspectRatio / normalAspectRatio,
                editorSettingsButton.GetComponent<RectTransform>().anchoredPosition.y);
            
            saveButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                saveButton.GetComponent<RectTransform>().anchoredPosition.x * aspectRatio / normalAspectRatio,
                saveButton.GetComponent<RectTransform>().anchoredPosition.y);

#if UNITY_STANDALONE
            if (Screen.fullScreen)
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            }
#endif

            if (Screen.orientation == ScreenOrientation.LandscapeLeft)
            {
                GameObject.Find("AddClickNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    Screen.safeArea.x,
                    GameObject.Find("AddClickNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                
                GameObject.Find("AddHoldNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    Screen.safeArea.x,
                    GameObject.Find("AddHoldNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                
                GameObject.Find("AddDragNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    Screen.safeArea.x,
                    GameObject.Find("AddDragNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                
                GameObject.Find("AddFlickNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    Screen.safeArea.x,
                    GameObject.Find("AddFlickNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                
                GameObject.Find("AddScanlineNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    Screen.safeArea.x,
                    GameObject.Find("AddScanlineNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
            }
            else if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                GameObject.Find("MoveNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    -Screen.safeArea.x,
                    GameObject.Find("MoveNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                
                GameObject.Find("OtherOptionsScrollView").GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    -Screen.safeArea.x,
                    GameObject.Find("OtherOptionsScrollView").GetComponent<RectTransform>().anchoredPosition.y);
            }
        }
    }
}