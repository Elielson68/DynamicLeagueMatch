using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MatchController : MonoBehaviour
{
    public bool CanSelectPlayers = true;
    public bool InitializeMatch;
    public List<int> MatchPlayersIndex = new();
    public int IndexWinner;
    public List<PlayerConfig> Players;
    public PlayerController PlayerPrefab;
    public MatchController MatchPrefab;
    public float Distance = 2;
    public Vector3 FinalMatchPoint;
    public Vector3[] ChildsPositions;
    public AudioSource WinMusic;

    private LineRenderer[] _linesRenderers;
    private bool _animationFinish;
    public List<PlayerController> _playersCreated;
    public List<PlayerController> PlayersSelected;
    public GameObject FogoArtificio;
    private CompetitionController _competitionController;

    IEnumerator Start()
    {
        _competitionController = FindAnyObjectByType<CompetitionController>();

        if (IsValidMatch() is false)
        {
            yield break;
        }

        float currentY = transform.position.y;
        transform.localScale = Vector3.zero;
        transform.DOMoveY(0f, 0f);

        yield return new WaitForEndOfFrame();

        CreatePlayers();
        transform.DOScale(1f, 2f);
        transform.DOMoveY(currentY, 2f);

        yield return transform.DOMoveY(currentY, 2f).WaitForKill();
        yield return new WaitForEndOfFrame();

        if (Players.Count == 1)
        {
            WinMusic.Play();
            StartCoroutine(WinnerAnimationFogos());
            yield return WinnerAnimation();
        }

        UpdatePositions();
    }
    bool winnerAnim = true;
    public IEnumerator WinnerAnimation()
    {
        while (winnerAnim)
        {
            FogoArtificio.SetActive(true);
            transform.DORotate(Vector3.forward * 3, 2f);
            yield return transform.DOScale(2f, 2f).WaitForKill();
            transform.DORotate(Vector3.back * 3, 2f);
            yield return transform.DOScale(1.5f, 2f).WaitForKill();
            transform.DORotate(Vector3.up * 3, 2f);
            yield return transform.DOScale(2.3f, 2f).WaitForKill();
            transform.DORotate(Vector3.down * 3, 2f);
            yield return transform.DOScale(1f, 2f).WaitForKill();
        }
    }

    public IEnumerator WinnerAnimationFogos()
    {
        while (winnerAnim)
        {
            GameObject fogo = Instantiate(FogoArtificio);
            var pos = fogo.transform.position;
            fogo.transform.position = new Vector3(pos.x + Random.Range(0, 2.5f), pos.y + Random.Range(0, 2.5f), pos.z);
            yield return new WaitForSeconds(Random.Range(0, 1f));
        }
    }
    public void StartMove()
    {
        StartCoroutine(StartMoveRoutine());
    }

    private IEnumerator StartMoveRoutine()
    {
        if (_playersCreated.Count == 3)
        {
            yield return new WaitUntil(() => PlayersSelected.Count == 2);
            CanSelectPlayers = false;
            MatchPlayersIndex[0] = _playersCreated.IndexOf(PlayersSelected[0]);
            MatchPlayersIndex[1] = _playersCreated.IndexOf(PlayersSelected[1]);
            MatchPlayersIndex.Sort();
            Debug.Log($"{string.Join(",", MatchPlayersIndex)}");

            yield return new WaitForEndOfFrame();

            CreateLines();

            yield return new WaitUntil(() => _animationFinish);

            CreateNewMatch();
        }
        else if (_playersCreated.Count == 2)
        {
            yield return new WaitUntil(() => IndexWinner > -1);
            CanSelectPlayers = false;
            yield return new WaitForEndOfFrame();

            CreateLines();

            yield return new WaitUntil(() => _animationFinish);

            CreateNewMatch();
        }

    }

    private PlayerController _lastPlayerSelected;

    public void SetSelectPlayer(PlayerController player)
    {
        if (CanSelectPlayers is false)
        {
            return;
        }

        if (_playersCreated.Count == 2)
        {
            IndexWinner = _playersCreated.IndexOf(player);
            _lastPlayerSelected?.SetSelected(false);
            player.SetSelected(true);
            _lastPlayerSelected = player;
            return;
        }

        if (PlayersSelected.Contains(player))
        {
            PlayersSelected.Remove(player);
            player.SetSelected(false);
            return;
        }

        if (PlayersSelected.Count < 2)
        {
            PlayersSelected.Add(player);
            player.SetSelected(true);
        }
    }

    public void SetPlayers(List<PlayerConfig> players)
    {
        Players = players;
    }

    public void CreatePlayers()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (PlayerConfig pc in Players)
        {
            var p = Instantiate(PlayerPrefab, transform);
            _playersCreated.Add(p);
            p.Configure(pc, this);
        }
    }

    public void UpdatePositions()
    {
        _linesRenderers = GetComponentsInChildren<LineRenderer>();
        var listAux = transform.GetComponentsInChildren<Image>().ToList().ConvertAll(x => x.rectTransform);
        ChildsPositions = listAux.ConvertAll(img => img.position).ToArray();
    }

    [ContextMenu("Debug Positions")]
    public void CreatePositionsDebug()
    {
        UpdatePositions();
        CreateLines();
    }

    [ContextMenu("Create Lines")]
    public void CreateLines()
    {
        if (IsValidMatch() is false)
        {
            return;
        }

        ClearAllLines();

        int p1 = MatchPlayersIndex[0];
        int p2 = MatchPlayersIndex[1];

        var firstChildPositions = GetChildPositions(ChildsPositions[p1], ChildsPositions[p2]);
        var secondChildPositions = GetChildPositions(ChildsPositions[p2], ChildsPositions[p1]);

        MoveLinesFoward(_linesRenderers[p1], firstChildPositions);
        MoveLinesFoward(_linesRenderers[p2], secondChildPositions);
    }

    private Vector3[] GetChildPositions(Vector3 firstChild, Vector3 secondChild)
    {
        Vector3[] positions = new Vector3[3];

        Vector3 startPosition = firstChild;
        Vector3 endPosition = new Vector3(startPosition.x + Distance, startPosition.y, startPosition.z);

        Vector3 middlePosition = (firstChild + secondChild) / 2;
        middlePosition.x = endPosition.x;

        positions[0] = startPosition;
        positions[1] = endPosition;
        positions[2] = middlePosition;

        return positions;
    }

    private void MoveLinesFoward(LineRenderer line, Vector3[] positions)
    {
        _animationFinish = false;
        SetLosers();

        for (int i = 0; i < 3; i++)
        {
            line.SetPosition(i, positions[0]);
        }

        Vector3 auxPos = positions[0];

        DOTween.To(x => { auxPos.x = x; line.SetPosition(1, auxPos); line.SetPosition(2, auxPos); }, auxPos.x, positions[1].x, 2f)
        .OnComplete(() =>
        {
            line.SetPosition(1, positions[1]);

            auxPos = positions[1];
            DOTween.To(y => { auxPos.y = y; line.SetPosition(2, auxPos); }, auxPos.y, positions[2].y, 2f)
            .OnComplete(() =>
            {
                FinalMatchPoint = auxPos;
                _animationFinish = true;
            });
        });
    }

    private void CreateNewMatch()
    {
        Vector3 auxFinalMatch = FinalMatchPoint;

        if (Players.Count == 1)
        {
            return;
        }

        var match = Instantiate(MatchPrefab, auxFinalMatch, Quaternion.identity, transform.parent);
        _competitionController.CurrentMatch = match;

        if (Players.Count == 3)
        {
            List<PlayerConfig> nextPlayers = new() { Players[MatchPlayersIndex[0]], Players[MatchPlayersIndex[1]] };

            match.SetPlayers(nextPlayers);
            auxFinalMatch.y += 1.5f;
            auxFinalMatch.x += 0.5f;
            match.transform.position = auxFinalMatch;
            match.MatchPrefab = MatchPrefab;
            return;
        }

        if (Players.Count == 2)
        {
            List<PlayerConfig> nextPlayers = new() { Players[MatchPlayersIndex[IndexWinner]] };
            match.SetPlayers(nextPlayers);
            auxFinalMatch.y += 0.5f;
            auxFinalMatch.x += 0.5f;
            match.transform.position = auxFinalMatch;
            return;
        }
    }

    private void SetLosers()
    {
        List<PlayerController> auxP = new(_playersCreated);
        if (Players.Count == 3)
        {
            List<PlayerController> elementsToRemove = new();

            foreach (int i in MatchPlayersIndex)
            {
                elementsToRemove.Add(auxP[i]);
            }

            foreach (PlayerController el in elementsToRemove)
            {
                auxP.Remove(el);
            }

            auxP[0].SetPlayerLost();
            return;
        }

        if (Players.Count == 2)
        {
            auxP.RemoveAt(IndexWinner);
            auxP[0].SetPlayerLost();
            return;
        }
    }

    private bool IsValidMatch()
    {
        AutomaticConfigurePlayers();

        if (MatchPlayersIndex.Count > 2)
        {
            Debug.LogError("Só é possível realizar o Match entre 2 jogadores!");
            return false;
        }

        if (ExistSameElement())
        {
            Debug.LogError("Não é possível realizar o Match com elementos iguais!");
            return false;
        }

        return true;
    }

    private bool ExistSameElement()
    {
        var l1 = MatchPlayersIndex.ToList();

        for (int i = 0; i < MatchPlayersIndex.Count; i++)
        {
            for (int ii = 0; ii < MatchPlayersIndex.Count; ii++)
            {
                if (MatchPlayersIndex[i] == MatchPlayersIndex[ii] && i != ii)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void AutomaticConfigurePlayers()
    {
        if (MatchPlayersIndex.Count < 2)
        {
            MatchPlayersIndex = new() { 0, 1 };
        }

        if (MatchPlayersIndex.Count == 2 && ExistSameElement())
        {
            MatchPlayersIndex[0] = 0;
            MatchPlayersIndex[1] = 1;
        }
    }

    private void ClearAllLines()
    {
        foreach (LineRenderer line in _linesRenderers)
        {
            line.SetPositions(new Vector3[3] { Vector3.zero, Vector3.zero, Vector3.zero });
        }
    }
}
