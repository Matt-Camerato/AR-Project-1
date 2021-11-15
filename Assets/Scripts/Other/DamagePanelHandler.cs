using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//When damage screen turns on, image color starts at 0.5 alpha and then fades away over around a second before object is disabled again
public class DamagePanelHandler : MonoBehaviour
{
    [SerializeField] private Color enableColor;
    private Image image;
    private float alpha;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        image.color = enableColor;
        alpha = enableColor.a;
    }

    private void Update()
    {
        alpha -= Time.deltaTime * 0.5f;
        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

        if(alpha <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
