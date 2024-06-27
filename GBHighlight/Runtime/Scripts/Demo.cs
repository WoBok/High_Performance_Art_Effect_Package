using UnityEngine;

public class Demo : MonoBehaviour
{
    public GameObject[] gameObjects;
    public GameObject[] addGameObjects;
    public float duration;
    public int index;
    void OnGUI()
    {
        if (GUILayout.Button("Open"))
        {
            GBHighlight.Open(gameObjects, duration);
        }
        if (GUILayout.Button("Open 2"))
        {
            GBHighlight.Open(gameObjects, gameObjects[Mathf.Min(index, gameObjects.Length - 1)].transform.position, duration);
        }
        if (GUILayout.Button("Add GameObjects"))
        {
            GBHighlight.AddGameObjects(addGameObjects);
        }
        if (GUILayout.Button("Close"))
        {
            GBHighlight.Close();
        }
    }
}