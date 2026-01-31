using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> buttons;

    private float buttonMoveTime = 1f;
    
    public void OnButtonClick()
    {
        StartCoroutine(MoveButtonsSequentially());
    }

    private IEnumerator MoveButtonsSequentially()
    {
        foreach (var button in buttons)
        {
            yield return StartCoroutine(MoveGameObjectToTheLeft(button, buttonMoveTime));
        }
    }

    private IEnumerator MoveGameObjectToTheLeft(GameObject gameObject, float time)
    {
        // move the gameObject to the left over the given time
        gameObject.transform.DOMoveX(gameObject.transform.position.x - 500f, time).SetEase(Ease.InCubic);

        yield return new WaitForSeconds(time/2);
    }
}