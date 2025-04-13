using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] GameObject healthBarParent;
    [SerializeField] Image healthBarImage;

    private void Awake()
    {
        health.clientOnHealthUpdated += OnHealthUpdated;
    }

    private void OnDestroy()
    {
        health.clientOnHealthUpdated -= OnHealthUpdated;
    }

    private void OnMouseEnter()
    {
        healthBarParent.SetActive(true);
    }

    private void OnMouseExit()
    {
        healthBarParent.SetActive(false);
    }

    void OnHealthUpdated(int maxHealth, int currentHealth)
    {
        healthBarImage.fillAmount = (float)currentHealth / maxHealth;
    }


}
