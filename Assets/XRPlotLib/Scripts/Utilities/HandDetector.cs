using UnityEngine;

namespace MAGES.XRPlotLib
{
    public class HandDetector : MonoBehaviour
    {
        public int handTriggerStack = 0;

        // You can set this in the inspector to determine which hand this detector is for
        public GameObject hand;        

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.Equals(hand))
            {
                handTriggerStack++;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.Equals(hand))
            {
                handTriggerStack--;
            }
        }

        public bool isHandIn()
        {
            return handTriggerStack != 0;
        }
    }
}