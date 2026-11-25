using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ReactionType
{
    Combat,
    Friendly,
    Magic,
    Extra
}

public class ReactionTracker : MonoBehaviour
{
    [Header("References")]
    public ImageTracker tracker;
    public List<BeastReaction> reactions = new List<BeastReaction>();
    public AudioSource source;

    [Header("Reaction Settings")]
    public float repeatInterval = 3f;

    private GameObject result;

    private TrackedBest bA;
    private TrackedBest bB;

    private Animator animA;
    private Animator animB;

    private BeastReaction currentReaction;

    private bool hasInteract = false;

    private Coroutine reactionCoroutine;

    private void Update()
    {
        if (hasInteract)
        {
            if (!PairStillValid())
            {
                CancelReaction();
            }

            return;
        }

        List<TrackedBest> visible = new List<TrackedBest>();

        foreach (TrackedBest beast in tracker.SpawnedObject)
        {
            if (beast.IsTracking() && !beast.isReacting)
            {
                visible.Add(beast);
            }
        }

        // Buscar combinaciones
        for (int i = 0; i < visible.Count; i++)
        {
            for (int j = i + 1; j < visible.Count; j++)
            {
                if (CheckReaction(visible[i], visible[j]))
                    return;
            }
        }
    }

    private bool CheckReaction(TrackedBest a, TrackedBest b)
    {
        foreach (BeastReaction reaction in reactions)
        {
            if (reaction == null)
                continue;

            if (!reaction.CanReaction(a.bestData, b.bestData))
                continue;

            float distance = Vector3.Distance(a.GetPos(), b.GetPos());

            if (distance > reaction.distance)
                continue;

            ExecuteReaction(a, b, reaction);
            return true;
        }

        return false;
    }

    private void ExecuteReaction(TrackedBest a, TrackedBest b, BeastReaction reaction)
    {
        ModelInfo modelA = a.instance.GetComponentInChildren<ModelInfo>(true);
        ModelInfo modelB = b.instance.GetComponentInChildren<ModelInfo>(true);

        if (modelA == null || modelB == null)
        {
            Debug.LogError("[ReactionTracker] No se encontró ModelInfo en una de las criaturas.");
            return;
        }

        animA = modelA.animator;
        animB = modelB.animator;

        if (animA == null || animB == null)
        {
            Debug.LogError("[ReactionTracker] No se encontró el Animator de una de las criaturas.");
            return;
        }

        hasInteract = true;

        bA = a;
        bB = b;

        currentReaction = reaction;

        tracker.SetBestRract(a, true);
        tracker.SetBestRract(b, true);

        Debug.Log($"[ReactionTracker] Iniciando reacción: {reaction.type}");

        // Ejecutar inmediatamente
        PlayReaction();

        // Iniciar repetición
        reactionCoroutine = StartCoroutine(RepeatReaction());
    }

    private IEnumerator RepeatReaction()
    {
        while (hasInteract)
        {
            yield return new WaitForSeconds(repeatInterval);

            if (!hasInteract)
                yield break;

            if (!PairStillValid())
            {
                CancelReaction();
                yield break;
            }

            PlayReaction();
        }
    }

    private void PlayReaction()
    {
        if (animA == null || animB == null || currentReaction == null)
            return;

        string trigger = "";

        switch (currentReaction.type)
        {
            case ReactionType.Combat:
                trigger = "Combat";
                break;

            case ReactionType.Friendly:
                trigger = "Friendly";
                break;

            case ReactionType.Magic:
                trigger = "Magic";
                break;

            case ReactionType.Extra:
                trigger = "Extra";
                break;
        }

        if (string.IsNullOrEmpty(trigger))
            return;

        animA.ResetTrigger(trigger);
        animB.ResetTrigger(trigger);

        animA.SetTrigger(trigger);
        animB.SetTrigger(trigger);

        if (source != null && currentReaction.clip != null)
            source.PlayOneShot(currentReaction.clip);

        Debug.Log($"[ReactionTracker] Reproduciendo: {trigger}");
    }

    private bool PairStillValid()
    {
        if (bA == null || bB == null)
            return false;

        if (!bA.IsTracking() || !bB.IsTracking())
            return false;

        if (currentReaction == null)
            return false;

        float distance = Vector3.Distance(bA.GetPos(), bB.GetPos());

        if (distance > currentReaction.distance)
            return false;

        return true;
    }

    private void CancelReaction()
    {
        if (reactionCoroutine != null)
        {
            StopCoroutine(reactionCoroutine);
            reactionCoroutine = null;
        }

        if (result != null)
        {
            Destroy(result);
            result = null;
        }

        hasInteract = false;

        if (bA != null)
            tracker.SetBestRract(bA, false);

        if (bB != null)
            tracker.SetBestRract(bB, false);

        Debug.Log("[ReactionTracker] Interacción cancelada.");

        bA = null;
        bB = null;

        animA = null;
        animB = null;

        currentReaction = null;
    }

    public void ResetReaction()
    {
        if (hasInteract)
        {
            CancelReaction();
        }
    }
}