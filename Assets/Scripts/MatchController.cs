using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MatchController : MonoBehaviour
{
    public bool InitializeMatch;
    public int[] MatchPlayersIndex = new int[2];
    public int IndexWinner;
    public List<PlayerConfig> Players;
    public PlayerController PlayerPrefab;
    public MatchController MatchPrefab;
    public float Distance = 2;
    public Vector3 FinalMatchPoint;
    public Vector3[] ChildsPositions;

    private LineRenderer[] _linesRenderers;
    private bool _animationFinish;

    IEnumerator Start()
    {
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

        yield return new WaitUntil(() => InitializeMatch);

        UpdatePositions();

        yield return new WaitForEndOfFrame();

        CreateLines();

        yield return new WaitUntil(() => _animationFinish);

        CreateNewMatch();
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
            p.Configure(pc);
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

    public float GetMidPointBetweenPlayers()
    {
        UpdatePositions();

        int p1 = MatchPlayersIndex[0];
        int p2 = MatchPlayersIndex[1];

        Vector3 middlePosition = (ChildsPositions[p1] + ChildsPositions[p2]) / 2;
        return middlePosition.y;
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
        StartCoroutine(MoveLinesRoutine(line, positions));
    }

    private IEnumerator MoveLinesRoutine(LineRenderer line, Vector3[] positions)
    {
        _animationFinish = false;

        for (int i = 0; i < 3; i++)
        {
            line.SetPosition(i, positions[0]);
        }

        Vector3 auxPos = positions[0];

        yield return new WaitForSeconds(3f);

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

    private bool IsValidMatch()
    {
        AutomaticConfigurePlayers();

        if (MatchPlayersIndex.Length > 2)
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

        for (int i = 0; i < MatchPlayersIndex.Length; i++)
        {
            for (int ii = 0; ii < MatchPlayersIndex.Length; ii++)
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
        if (MatchPlayersIndex.Length < 2)
        {
            MatchPlayersIndex = new int[2];
            MatchPlayersIndex[0] = 0;
            MatchPlayersIndex[1] = 1;
        }

        if (MatchPlayersIndex.Length == 2 && ExistSameElement())
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
