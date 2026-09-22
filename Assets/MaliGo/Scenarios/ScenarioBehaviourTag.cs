namespace MaliGo.Scenarios
{
    /// <summary>
    /// Behavioural signal carried by a scenario choice. Used to gradually
    /// nudge FinancialStats behaviour scores - never to grade the choice as "right" or "wrong".
    /// </summary>
    public enum ScenarioBehaviourTag
    {
        Neutral,
        Frugal,
        Discretionary,
        Deferred
    }
}
