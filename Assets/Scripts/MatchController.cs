using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MatchController : MonoBehaviour
{
    public float Distance = -4;
    public Vector3 FinalMatchPoint;
    public Vector3[] ChildsPositions;
    public int[] MatchPlayersIndex = new int[2];
    private LineRenderer[] _linesRenderers;

    IEnumerator Start()
    {
        if (IsValidMatch() is false)
        {
            yield break;
        }

        yield return new WaitForEndOfFrame();
        _linesRenderers = GetComponentsInChildren<LineRenderer>();
        var listAux = transform.GetComponentsInChildren<Image>().ToList().ConvertAll(x => x.rectTransform);
        ChildsPositions = listAux.ConvertAll(img => img.position).ToArray();
        _linesRenderers.ToList().ForEach(x => Debug.Log(x.name, x.gameObject));
        CreateLines();
    }

    [ContextMenu("Debug Positions")]
    public void DebugPositions()
    {
        var listAux = transform.GetComponentsInChildren<Image>().ToList().ConvertAll(x => x.rectTransform);
        listAux.RemoveAt(0);
        listAux.ForEach(img => Debug.Log(img.position));
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

        FinalMatchPoint = ChildsPositions[p1] + ChildsPositions[p2] / 2;

        var firstChildPositions = GetChildPositions(ChildsPositions[p1], ChildsPositions[p2]);
        var secondChildPositions = GetChildPositions(ChildsPositions[p2], ChildsPositions[p1]);

        MoveLinesFoward(_linesRenderers[p1], firstChildPositions);
        MoveLinesFoward(_linesRenderers[p2], secondChildPositions);
    }

    private Vector3[] GetChildPositions(Vector3 firstChild, Vector3 secondChild)
    {
        Vector3[] positions = new Vector3[3];

        Vector3 startPosition = firstChild;
        Vector3 endPosition = new Vector3(Distance, startPosition.y, startPosition.z);

        Vector3 middlePosition = (firstChild + secondChild) / 2;
        middlePosition.x = Distance;

        positions[0] = startPosition;
        positions[1] = endPosition;
        positions[2] = middlePosition;

        return positions;
    }

    private void MoveLinesFoward(LineRenderer line, Vector3[] positions)
    {
        line.SetPosition(0, positions[0]);
        Vector3 moveAux = positions[0];

        DOTween.To(x => { moveAux.x = x; line.SetPosition(1, moveAux); line.SetPosition(2, moveAux); }, moveAux.x, positions[1].x, 2f)
        .OnComplete(() =>
        {
            line.SetPosition(1, positions[1]);

            moveAux = positions[1];
            DOTween.To(y => { moveAux.y = y; line.SetPosition(2, moveAux); }, moveAux.y, positions[2].y, 2f);
        });
    }

    private bool IsValidMatch()
    {
        if (MatchPlayersIndex.Length > 2)
        {
            Debug.LogError("Só é possível realizar o Match entre 2 jogadores!");
            return false;
        }

        var l1 = MatchPlayersIndex.ToList();

        foreach (var x in MatchPlayersIndex)
        {
            foreach (var y in MatchPlayersIndex)
            {
                Debug.Log($"x: {x} i: {l1.IndexOf(x)} | y: {y} i: {l1.IndexOf(y)}");

                if (x == y && l1.IndexOf(x) != l1.IndexOf(y))
                {
                    Debug.LogError("Não é possível realizar o Match com elementos iguais!");
                    return false;
                }
            }
        }

        AutomaticConfigurePlayers();
        return true;
    }

    private void AutomaticConfigurePlayers()
    {
        if (MatchPlayersIndex.Length < 2)
        {
            MatchPlayersIndex[0] = 0;
            MatchPlayersIndex[1] = 1;
        }
    }

    private void ClearAllLines()
    {
        foreach (LineRenderer line in _linesRenderers)
        {
            line.SetPositions(new Vector3[3]);
        }
    }
}
