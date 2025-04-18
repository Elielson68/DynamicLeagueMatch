using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public class CompetitionController : MonoBehaviour
{
    public Vector3[] ChildsPositions;
    private LineRenderer[] _linesRenderers;

    IEnumerator Start()
    {
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
        for (int i = 0; i < ChildsPositions.Length; i += 2)
        {
            if (i + 1 < ChildsPositions.Length)
            {
                Vector3[] firstChildPositions = GetChildPositions(ChildsPositions[i], ChildsPositions[i + 1]);
                Vector3[] secondChildPositions = GetChildPositions(ChildsPositions[i + 1], ChildsPositions[i]);

                //secondChildPositions[2].y *= -1;

                _linesRenderers[i].SetPositions(firstChildPositions);
                _linesRenderers[i + 1].SetPositions(secondChildPositions);
            }
        }
    }

    private Vector3[] GetChildPositions(Vector3 firstChild, Vector3 secondChild)
    {
        Vector3[] positions = new Vector3[3];

        float distance = -4;
        Vector3 startPosition = firstChild;
        Vector3 endPosition = new Vector3(distance, startPosition.y, startPosition.z);

        Vector3 middlePosition = (firstChild + secondChild) / 2;
        middlePosition.x = distance;

        positions[0] = startPosition;
        positions[1] = endPosition;
        positions[2] = middlePosition;

        return positions;
    }
}
