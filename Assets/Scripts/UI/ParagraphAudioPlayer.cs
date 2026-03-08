using UnityEngine;

public class ParagraphAudioPlayer : MonoBehaviour
{
    public AudioClip audioClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = FindObjectOfType<AudioSource>();
    }

public void PlayAudio()
{
    Debug.Log("Boton presionado");

    if (audioSource == null || audioClip == null)
    {
        Debug.Log("AudioSource o AudioClip faltante");
        return;
    }

    if (audioSource.isPlaying)
    {
        audioSource.Stop();
    }

    audioSource.clip = audioClip;
    audioSource.Play();
}
}