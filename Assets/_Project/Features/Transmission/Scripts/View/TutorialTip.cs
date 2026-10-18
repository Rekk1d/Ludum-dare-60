using System;
using UnityEngine;

[Serializable]
public class TutorialTip
{
    [SerializeField] private int round;
    [SerializeField] private bool beforeClock;
    [SerializeField] private string title;
    [SerializeField, TextArea(2, 4)] private string body;
    [SerializeField] private TutorialGoal goal;
    [SerializeField] private TutorialHighlight highlight;
    [SerializeField] private RectTransform[] targets;
    [SerializeField] private bool top;
    [SerializeField] private bool revealsTimer;

    public int Round => round;
    public bool BeforeClock => beforeClock;
    public string Title => title;
    public string Body => body;
    public TutorialGoal Goal => goal;
    public TutorialHighlight Highlight => highlight;
    public RectTransform[] Targets => targets;
    public bool Top => top;
    public bool RevealsTimer => revealsTimer;
}
