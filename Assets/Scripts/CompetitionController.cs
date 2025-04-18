using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public class CompetitionController : MonoBehaviour
{
    public MatchController CurrentMatch;

    public void StartMatch()
    {
        CurrentMatch.StartMove();
    }
}
