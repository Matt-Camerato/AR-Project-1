using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;

public class HealthDisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject playerHealthBannerPrefab;

    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private Sprite fullHeartSprite;

    private GameplayManager gameplayManager;

    private void Start()
    {
        gameplayManager = FindObjectOfType<GameplayManager>();

        for (int i = 0; i < GameplayManager.numPlayers; i++)
        {
            GameObject newPlayerBanner = Instantiate(playerHealthBannerPrefab, transform.GetChild(0).GetChild(2));
            newPlayerBanner.GetComponent<Image>().color = gameplayManager.players[i].color;
            newPlayerBanner.transform.GetChild(0).GetComponent<TMP_Text>().text = "Player " + gameplayManager.players[i].playerNum + ":";
            Transform heartIcons = newPlayerBanner.transform.GetChild(1);

            int health = gameplayManager.players[i].health;
            if (health == 3)
            {
                foreach (Transform child in heartIcons)
                {
                    child.GetComponent<Image>().sprite = fullHeartSprite;
                }
            }
            else if (health == 2)
            {
                heartIcons.GetChild(0).GetComponent<Image>().sprite = fullHeartSprite;
                heartIcons.GetChild(1).GetComponent<Image>().sprite = fullHeartSprite;
                heartIcons.GetChild(2).GetComponent<Image>().sprite = emptyHeartSprite;
            }
            else if (health == 1)
            {
                heartIcons.GetChild(0).GetComponent<Image>().sprite = fullHeartSprite;
                heartIcons.GetChild(1).GetComponent<Image>().sprite = emptyHeartSprite;
                heartIcons.GetChild(2).GetComponent<Image>().sprite = emptyHeartSprite;
            }
            else
            {
                foreach (Transform child in heartIcons)
                {
                    child.GetComponent<Image>().sprite = emptyHeartSprite;
                }
            }
        }
    }

    private void Update()
    {
        
    }
}
