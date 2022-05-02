using System.Collections;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        // Use this for initialization
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}