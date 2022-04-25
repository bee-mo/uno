using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TurnBanner : MonoBehaviour {
    // total time of banner entering and leaving screen
    private const float animationTime = 2.5f;
    // true: text and banner move together
    // false: text and banner enter and leave from opposite direction
    private bool isAttached = false;
    private Vector2 startPosition = new Vector2(-Screen.width * 5 / 3, Screen.height / 2);

    private Vector2 endPosition = new Vector2(Screen.width * 8 / 3, Screen.height / 2);

    private float elapsedTime = 0;

    [SerializeField]
    private AnimationCurve curve;

    public GameObject background;
    public GameObject text;

    // Start is called before the first frame update
    void Start() {
        // Debug.Log(background.GetComponent<RectTransform>().rect.height);
        // Debug.Log(text.GetComponent<RectTransform>().rect.height);
    }

    // Update is called once per frame
    void Update() {
        elapsedTime += Time.deltaTime;
        float percentage = elapsedTime / animationTime;
        background.transform.position = Vector2.Lerp(startPosition, endPosition, curve.Evaluate(percentage));
        text.transform.position = Vector2.Lerp(isAttached ? startPosition : endPosition
                                            , isAttached ? endPosition : startPosition
                                            , curve.Evaluate(percentage));
    }
}
