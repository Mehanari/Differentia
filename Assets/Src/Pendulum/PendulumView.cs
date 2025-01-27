using UnityEngine;

namespace Src.Pendulum
{
    public class PendulumView : MonoBehaviour
    {
        [SerializeField] private GameObject rod;
        [SerializeField] private GameObject bob;
        [SerializeField] private GameObject pivot;

        public void SetLength(float length)
        {
            var rodScale = rod.transform.localScale;
            rodScale.y = length;
            rod.transform.localScale = rodScale;
            var rodPosition = rod.transform.localPosition;
            rodPosition.y = -length / 2;
            rod.transform.localPosition = rodPosition;
            var bobPosition = bob.transform.localPosition;
            bobPosition.y = -length;
            bob.transform.localPosition = bobPosition;
        }

        public void SetAngleRadians(float angle)
        {
            pivot.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        }
    }
}
