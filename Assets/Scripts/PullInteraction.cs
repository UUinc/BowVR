using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Feedback;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class PullInteraction : XRBaseInteractable
{
    public static event Action<float> PullActionReleased;
    public Transform start, end;
    public GameObject notch;

    public float pullAmount { get; private set; } = 0.0f;

    private LineRenderer lineRenderer;
    private IXRSelectInteractor pullingInteractor = null;

    private AudioSource pullAudioSource;

    protected override void Awake()
    {
        base.Awake();
        lineRenderer = GetComponent<LineRenderer>();
        pullAudioSource = GetComponent<AudioSource>();
    }

    public void SetPullInteractor(SelectEnterEventArgs args)
    {
        pullingInteractor = args.interactorObject;
    }

    public void Release()
    {
        PullActionReleased?.Invoke(pullAmount);
        pullingInteractor = null;
        pullAmount = 0.0f;
        notch.transform.localPosition = new Vector3(notch.transform.localPosition.x, notch.transform.localPosition.y, 0);
        UpdateString();

        PlayReleaseSound();
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
            {
                Vector3 pullPosition = pullingInteractor.transform.position;
                pullAmount = CalculatePull(pullPosition);

                UpdateString();

                HapticFeedback();
            }
        }
    }

    private float CalculatePull(Vector3 pullPosition)
    {
        Vector3 pullDirection = pullPosition - start.position;
        Vector3 targetDirection = end.position - start.position;
        float maxLength = targetDirection.magnitude;

        targetDirection.Normalize();
        float pullValue = Vector3.Dot(pullDirection, targetDirection) / maxLength;
        return Mathf.Clamp(pullValue, 0.0f, 1.0f);
    }

    private void UpdateString()
    {
        Vector3 linePosition = Vector3.forward * Mathf.Lerp(start.transform.localPosition.z, end.transform.localPosition.z, pullAmount);
        notch.transform.localPosition = new Vector3(notch.transform.localPosition.x, notch.transform.localPosition.y, linePosition.z + 0.2f);
        lineRenderer.SetPosition(1, linePosition);
    }

    private void HapticFeedback()
    {
        if (pullingInteractor != null)
        {
            SimpleHapticFeedback haptic = pullingInteractor.transform.gameObject.GetComponent<SimpleHapticFeedback>();
            haptic.hapticImpulsePlayer.SendHapticImpulse(pullAmount, 0.1f);
        }
    }

    private void PlayReleaseSound()
    {
        pullAudioSource.Play();
    }
}
