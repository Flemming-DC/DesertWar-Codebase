using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingInfoDisplayer : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text name_;
    [SerializeField] TMP_Text description;
    [SerializeField] TMP_Text cost;
    [SerializeField] TMP_Text producedUnit;
    [SerializeField] TMP_Text unitCost;
    [SerializeField] TMP_Text requirements;
    [SerializeField] TMP_Text maxHealth;

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



        if (stats.producedUnit != null)
        {
            producedUnit.text = "<b>Produces</b>: " + stats.producedUnit.name;
            unitCost.text = "<b>unit cost</b>: " + stats.producedUnit.GetComponent<Selectable>().cost;
        }
        else
        {
            producedUnit.text = "<b>Produces</b>: None";
            unitCost.text = "<b>unit cost</b>: None";
        }
        if (stats.requirements.Length > 0)
        {
            requirements.text = "<b>Requirements</b>: ";
            foreach (var requiredBuilding in stats.requirements)
                requirements.text += requiredBuilding.name + ", ";
            requirements.text = requirements.text.TrimEnd(new char[] { ',', ' ' });
        }
        else
            requirements.text = "<b>Requirements</b>: None";

    }




}
