using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RichTap.Demo
{
    public class LoaderCallback : MonoBehaviour
    {


        // Update is called once per frame
        void Update()
        {
            Loader.OnLoaderCallback();

        }
    }
}