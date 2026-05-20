using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadManager : MonoBehaviour
{
    [SerializeField] private LoadingScreen _loadingScreen;
    [SerializeField] private Transform _parentContainer;

    private List<GameObject> _objectsToLoad = new List<GameObject>();

    private void Start()
    {
        if (_parentContainer != null)
        {
            foreach (Transform child in _parentContainer)
            {
                _objectsToLoad.Add(child.gameObject);
            }
        }

        int totalObjects = _objectsToLoad.Count;
        StartCoroutine(LoadAllObjectsRoutine(totalObjects));
    }

    private IEnumerator LoadAllObjectsRoutine(int totalObjects)
    {
        if (totalObjects == 0) yield break;

        for (int i = 0; i < totalObjects; i++)
        {
            _objectsToLoad[i].SetActive(true);

            yield return new WaitForSeconds(0.2f);

            float currentPercentage = ((float)(i + 1) / totalObjects) * 100f;
            _loadingScreen.Progress = currentPercentage;
        }
    }
}