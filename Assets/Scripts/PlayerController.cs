using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IPointerDownHandler
{
    private LineRenderer _lineRenderer;
    private MatchController _matchController;
    public Material LoseMaterial;
    public GameObject SelectedHighlight;
    public void Configure(PlayerConfig playerConfig, MatchController matchController)
    {
        _matchController = matchController;
        GetComponent<Image>().sprite = playerConfig.PlayerImage;
        // GetComponent<Image>().color = playerConfig.PlayerImage.color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _matchController.SetSelectPlayer(this);
    }

    public void SetSelected(bool selected)
    {
        SelectedHighlight.SetActive(selected);
    }

    public void SetPlayerLost()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        GetComponent<Image>().DOColor(Color.red, 2f);
        _lineRenderer.material = LoseMaterial;
    }
}
