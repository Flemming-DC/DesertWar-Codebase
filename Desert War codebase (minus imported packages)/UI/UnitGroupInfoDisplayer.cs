using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitGroupInfoDisplayer : MonoBehaviour
{
    [SerializeField] TMP_Text mediumTankCountText;
    [SerializeField] TMP_Text mineLayerCountText;
    [SerializeField] TMP_Text missileCarCountText;
    [SerializeField] TMP_Text mortarTankCountText;

    int mediumTankCount;
    int mineLayerCount;
    int missileCarCount;
    int mortarTankCount;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Display(List<Selectable> selectedUnits)
    {
        gameObject.SetActive(true);

        mediumTankCount = 0;
        mineLayerCount = 0;
        missileCarCount = 0;
        mortarTankCount = 0;

        foreach (Selectable selectable in selectedUnits)
        {
            string name_ = selectable.GetComponent<StatsBehaviour>().stats.name;

            if (name_ == "Boring Tank")
                mediumTankCount++;
            else if (name_ == "Mine Layer")
                mineLayerCount++;
            else if (name_ == "Missile Car")
                missileCarCount++;
            else if (name_ == "Mortar Tank")
                mortarTankCount++;
            else
                Debug.LogWarning($"{name_} is not among the known units");
        }

        mediumTankCountText.text = mediumTankCount.ToString();
        mineLayerCountText.text = mineLayerCount.ToString();
        missileCarCountText.text = missileCarCount.ToString();
        mortarTankCountText.text = mortarTankCount.ToString();

    }
}
