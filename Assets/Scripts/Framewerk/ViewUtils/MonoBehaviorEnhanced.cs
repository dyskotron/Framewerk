using UnityEngine;

namespace Framewerk.ViewUtils
{
    public class MonoBehaviorEnhanced : MonoBehaviour
    {

        public void SetXTo (GameObject obj, float x)
        {
            Vector3 pos = obj.transform.localPosition;
            pos.x = x;
            obj.transform.localPosition = pos;
        }

        public void SetX (float x)
        {
            SetXTo (gameObject, x);
        }

        public void SetYTo (GameObject obj, float y)
        {
            Vector3 pos = obj.transform.localPosition;
            pos.y = y;
            obj.transform.localPosition = pos;
        }

        public void SetY (float y)
        {
            SetYTo (gameObject, y);
        }

        public void SetZTo (GameObject obj, float z)
        {
            Vector3 pos = obj.transform.localPosition;
            pos.z = z;
            obj.transform.localPosition = pos;
        }

        public void SetZ (float z)
        {
            SetZTo (gameObject, z);
        }

        public void SetRotX (float x)
        {
            SetRotXTo (gameObject, x);
        }

        protected void SetRotXTo (GameObject obj, float x)
        {
            Quaternion rot = obj.transform.localRotation;
            Vector3 rotv3 = rot.eulerAngles;
            rotv3.x = x;
            obj.transform.localRotation = Quaternion.Euler (rotv3);
        }

        public void SetRotY (float y)
        {
            SetRotYTo (gameObject, y);
        }

        protected void SetRotYTo (GameObject obj, float y)
        {
            Quaternion rot = obj.transform.localRotation;
            Vector3 rotv3 = rot.eulerAngles;
            rotv3.y = y;
            obj.transform.localRotation = Quaternion.Euler (rotv3);
        }

        public void SetRotZ (float z)
        {
            SetRotZTo (gameObject, z);
        }

        protected void SetRotZTo (GameObject obj, float z)
        {
            Quaternion rot = obj.transform.localRotation;
            Vector3 rotv3 = rot.eulerAngles;
            rotv3.z = z;
            obj.transform.localRotation = Quaternion.Euler (rotv3);
        }
    }
}