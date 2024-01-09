using UnityEngine;
using Unity.Jobs.LowLevel.Unsafe;

namespace com.thousandant.boids.sample
{

    public class MaxJobUtility : MonoBehaviour
    {

#pragma warning disable 649
        [SerializeField]
        private ushort jobsCount = 4;
#pragma warning restore 649

        private void Awake()
        {
            JobsUtility.JobWorkerCount = jobsCount;
        }
    }
}
