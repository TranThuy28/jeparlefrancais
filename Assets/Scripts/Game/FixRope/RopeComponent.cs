using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

public class RopeComponent : MonoBehaviour, IPointerClickHandler
{
    public RopeType ropeType;
    RopeState currentState = RopeState.D0;
    private bool isRotating = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isRotating) return;

        isRotating = true;
        transform.DORotate(new Vector3(0, 0, transform.eulerAngles.z + 90f),
                    0.25f).SetEase(Ease.InQuad).onComplete = () =>
                    {
                        isRotating = false;
                        RopeManagement.Instance.CheckWin();
                    };
        Debug.Log($"Rope {ropeType} rotated from {currentState} to {(RopeState)(((int)currentState + 1) % 4)}");
        currentState = (RopeState)(((int)currentState + 1) % 4);
    }
}