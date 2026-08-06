using System;
using System.Collections.Generic;
using UnityEngine;

public class listtestdel : MonoBehaviour
{
   // [Serializable]
    public class NamedObject
    {
        public string objectName;
        public GameObject gameObject;
    }

    public List<NamedObject> objects = new List<NamedObject>();

    public void createList()
    {
        objects.Add(new NamedObject());
    }

    // Pass one name ("apple") or multiple names separated by commas ("banana,apple").
    // Matching GameObjects turn ON. Every other GameObject in the list turns OFF.
    public void ActivateObjects(string names)
    {
        string[] requestedNames = names.Split(',');
        HashSet<string> targetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string rawName in requestedNames)
        {
            targetNames.Add(rawName.Trim());
        }

        foreach (NamedObject entry in objects)
        {
            if (entry.gameObject == null)
            {
                Debug.LogWarning("No GameObject assigned for entry: " + entry.objectName);
                continue;
            }

            bool shouldBeOn = targetNames.Contains(entry.objectName.Trim());
            entry.gameObject.SetActive(shouldBeOn);
        }
    }
}
