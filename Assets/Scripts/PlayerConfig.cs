using UnityEngine;
using UnityEngine.UI;

// Cria um item no menu "Create" do Unity
[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Game/Player Config")]
public class PlayerConfig : ScriptableObject
{
    public Sprite PlayerImage;
    public AudioClip PlayerWinAudio;
    public AudioClip PlayerLoseAudio;
}