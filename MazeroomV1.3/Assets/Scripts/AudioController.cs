using UnityEngine;
public class AudioController : MonoBehaviour
{

    public AudioSource audioSorceBackgroundMusic;
    public AudioClip[] backgroundMusics;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int IndexRandomMusic = Random.Range(0, backgroundMusics.Length);
        AudioClip SceneBackgroundMusics = backgroundMusics[IndexRandomMusic];
        audioSorceBackgroundMusic.clip = SceneBackgroundMusics;
        audioSorceBackgroundMusic.loop = true;
        audioSorceBackgroundMusic.Play();
    }
}
