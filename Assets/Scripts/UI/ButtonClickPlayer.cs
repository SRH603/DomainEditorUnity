using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickPlayer : MonoBehaviour
{
    public AudioClip clickSound;
    public AudioSource audioSource;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlayClickSound);
    }
    
    public  void Refresh()
    {
        GetComponent<Button>().onClick.AddListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}