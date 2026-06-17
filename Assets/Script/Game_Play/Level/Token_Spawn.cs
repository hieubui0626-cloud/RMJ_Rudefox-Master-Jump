using UnityEngine;
using System.Collections.Generic;

public class Token_Spawn : MonoBehaviour
{
    [SerializeField] private GameObject layoutToken;
    public int numberToEnable;
    void Start()
    {
        RandomizeChildObjects();
    }

    // Update is called once per frame
    public void RandomizeChildObjects()
    {
        if (layoutToken == null)
        {
            Debug.LogError("Vui lòng kéo Layout_Token vào Script!");
            return;
        }

        // 1. Lấy tất cả các child object trực tiếp của Layout_Token
        List<GameObject> childList = new List<GameObject>();
        foreach (Transform child in layoutToken.transform)
        {
            childList.Add(child.gameObject);
        }

        int totalChildren = childList.Count;

        // Kiểm tra điều kiện an toàn cho biến int
        if (numberToEnable < 0) numberToEnable = 0;
        if (numberToEnable > totalChildren) numberToEnable = totalChildren;

        // 2. Thuật toán xáo trộn ngẫu nhiên (Fisher-Yates Shuffle)
        for (int i = 0; i < childList.Count; i++)
        {
            GameObject temp = childList[i];
            int randomIndex = Random.Range(i, childList.Count);
            childList[i] = childList[randomIndex];
            childList[randomIndex] = temp;
        }

        // 3. Kích hoạt số lượng chỉ định, tắt số còn lại
        for (int i = 0; i < childList.Count; i++)
        {
            if (i < numberToEnable)
            {
                childList[i].SetActive(true);
            }
            else
            {
                childList[i].SetActive(false);
            }
        }

        Debug.Log($"Đã kích hoạt ngẫu nhiên {numberToEnable}/{totalChildren} object con.");
    }
}
