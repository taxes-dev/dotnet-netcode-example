using UnityEngine;

namespace Example.Client
{
    /// <summary>
    /// Simple component to have a UI element follow a game object.
    /// </summary>
    [ExecuteInEditMode]
    public class UIElementOnObject : MonoBehaviour
    {
        /// <summary>
        /// The game object to follow.
        /// </summary>
        public GameObject FollowObject;

        /// <summary>
        /// An X and Y offset in world points to locate the UI element.
        /// </summary>
        public Vector2 Offset;

        /// <summary>
        /// Called every fixed frame update.
        /// </summary>
        void FixedUpdate()
        {
            if (this.FollowObject != null)
            {
                Vector3 objectPos = this.FollowObject.transform.position + new Vector3(Offset.x, Offset.y, 0);
                Vector3 point = Camera.main.WorldToScreenPoint(objectPos);
                this.transform.position = new Vector3(point.x, Screen.height - point.y);
            }
        }
    }

}