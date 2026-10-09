using UnityEngine;
using UnityEngine.XR;

namespace Heroes3QuestPrototype
{
    // Uses standard UnityEngine.XR feature usages to avoid proprietary Meta input dependencies.
    public sealed class QuestXRInput : MonoBehaviour
    {
        private readonly Vector3 desktopHeadOffset = new Vector3(0f, 1.65f, 0f);
        private static readonly Quaternion DesktopHeadRotation = Quaternion.Euler(28f, 0f, 0f);

        public void GetPointingRay(out Ray ray, out bool pressed)
        {
            var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (head.isValid &&
                head.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 hmdPos) &&
                head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion hmdRot))
            {
                transform.localPosition = hmdPos;
                transform.localRotation = hmdRot;
            }
            else
            {
                transform.localPosition = desktopHeadOffset;
                transform.localRotation = DesktopHeadRotation;
            }

            var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (right.isValid &&
                right.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 handPos) &&
                right.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion handRot))
            {
                Vector3 origin = transform.parent ? transform.parent.TransformPoint(handPos) : handPos;
                Vector3 direction = transform.parent ?
                    transform.parent.TransformDirection(handRot * Vector3.forward) :
                    handRot * Vector3.forward;
                ray = new Ray(origin, direction);
                pressed = right.TryGetFeatureValue(CommonUsages.triggerButton, out bool isDown) && isDown;
                return;
            }

            // Desktop and simulator input; mouse clicks use Camera.ScreenPointToRay in game controller.
            ray = new Ray(transform.position, transform.forward);
            pressed = false;
        }
    }
}