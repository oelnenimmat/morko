﻿using UnityEngine;

public static class AvatarInstantiator
{
    public static GameObject[] Instantiate(int[] avatarModelIds, string folderName = "AvatarPrefabs")
    {
        int count = avatarModelIds.Length;
        
        var prefabs = Resources.LoadAll<GameObject>(folderName);
        var results = new GameObject[count];

        for (int avatarIndex = 0; avatarIndex < count; avatarIndex++)
        {
        	int modelIndex = avatarModelIds[avatarIndex];
            results[avatarIndex] = MonoBehaviour.Instantiate(prefabs[modelIndex]);
        }
        
        return results;
    }
}