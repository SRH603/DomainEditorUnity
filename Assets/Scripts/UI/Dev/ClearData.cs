using UnityEngine;
using static GameUtilities;

public class ClearData : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Clear()
    {
        GameUtilities.Archive.ClearArchive();
    }
}
