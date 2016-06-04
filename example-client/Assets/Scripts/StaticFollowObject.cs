using System;
using UnityEngine;

namespace Example.Client
{
    /// <summary>
    /// Allows one GameObject follow another GameObject, maintaing a static distance between them. The initial position
    /// of the two objects in the scene determine their offesets from one another.
    /// </summary>
    public class StaticFollowObject : MonoBehaviour
    {
        #region Private fields
        private Vector3 offset;
        #endregion

        /// <summary>
        /// Target to follow.
        /// </summary>
        public GameObject FollowObject;

        /// <summary>
        /// Use this for initialization. 
        /// </summary>
        void Start()
        {
            if (this.FollowObject != null)
            {
                this.offset = this.transform.position - this.FollowObject.transform.position;
            }
        }

        /// <summary>
        /// Update is called once per frame. 
        /// </summary>
        void Update()
        {
            if (this.FollowObject != null)
            {
                this.transform.position = this.FollowObject.transform.position + offset;
            }
        }
    }

}