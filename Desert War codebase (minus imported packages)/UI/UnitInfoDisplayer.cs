using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitInfoDisplayer : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text name_;
    [SerializeField] TMP_Text description;
    [SerializeField] TMP_Text cost;
    [SerializeField] TMP_Text maxHealth;
    [SerializeField] TMP_Text damage;
    [SerializeField] TMP_Text attackSpeed;
    [SerializeField] TMP_Text range;
    [SerializeField] TMP_Text movementSpeed;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Display(Stats stats)
    {
        gameObject.SetActive(true);

        icon.sprite = stats.icon;
        name_.text = stats.name;
        description.text = stats.description;
        cost.text = "<b>Oil</b>: " + stats.cost;
        maxHealth.text = "<b>Health</b>: " + stats.maxHealth;
        damage.text = "<b>Damage</b>: " + stats.damage;
        attackSpeed.text = "<b>Attack Speed</b>: " + stats.attackSpeed;
        range.text = "<b>Range</b>: " + stats.attackRange;
        movementSpeed.text = "<b>Movement Speed</b>: " + stats.movementSpeed;
    }
}
