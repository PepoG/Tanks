using UnityEngine;

public class CreateHelloWorld : MonoBehaviour
{
    [ContextMenu("Create Hello World")]
    private void CreateHelloWorldObject()
    {
        GameObject helloWorld = new GameObject("HelloWorld");
        Debug.Log("HelloWorld GameObject created!");
    }

    // Start is called before the first frame update
    void Start()
    {
        CreateHelloWorldObject();
    }
}
