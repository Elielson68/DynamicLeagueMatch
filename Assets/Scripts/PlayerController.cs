using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public void Configure(PlayerConfig playerConfig)
    {
        GetComponent<Image>().sprite = playerConfig.PlayerImage;
        // GetComponent<Image>().color = playerConfig.PlayerImage.color;
    }
}
