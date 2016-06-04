using System;
using System.Collections;
using UnityEngine;

using Example.Messages;

namespace Example.Client
{
    /// <summary>
    /// Place this component on the object(s) which should receive player movement input.
    /// </summary>
    public class PlayerPositionUpdate : MonoBehaviour, ISubscriber
    {
        #region Private fields
        /// <summary>
        /// How far the player has to move before we tell the server.
        /// </summary>
        private const float PKT_SEND_TOLERANCE = 0.25f;

        private Rigidbody _rigidbody;
        private Vector3 direction;
        private Vector3 syncPosition;
        private Vector3 lastSentPosition;
        private bool ignoreInputDirection;
        #endregion

        /// <summary>
        /// Speed for the object.
        /// </summary>
        public float Speed = 6.0f;

        /// <summary>
        /// Use this for initialization.
        /// </summary>
        void Start()
        {
            this._rigidbody = GetComponent<Rigidbody>();
            if (this._rigidbody != null)
            {
                // We want to know when the player requests to move, so that we can move in the desired direction
                // We also want to receive general position updates from the server
                MessageBroker.Instance.Subscribe(this, Msgs.CMD_INPUT_MOVE, Msgs.CMD_POS);
            }
        }

        /// <summary>
        /// Called every fixed frame update.
        /// </summary>
        void FixedUpdate()
        {
            // If we received a position from the server, lerp to that position now instead of following user input
            if (this.syncPosition != Vector3.zero)
            {
                this.ignoreInputDirection = true;
                this.direction = Vector3.zero;
                StartCoroutine(BlendSyncPosition(this.syncPosition));
                this.syncPosition = Vector3.zero;
                return;
            }

            // Move us in the direction indicated by the player
            if (this.direction == Vector3.zero)
            {
                this._rigidbody.angularVelocity = Vector3.zero;
                this._rigidbody.velocity = Vector3.zero;
            }
            else
            {
                this.transform.LookAt(this.transform.position + this.direction);
                this._rigidbody.velocity = this.direction * Speed;
            }

            // Send positional updates periodically, PKT_SEND_TOLERANCE determines how far we wait for the
            // player to move (in world distance) before sending an update.
            if (Vector3.Distance(this.lastSentPosition, this.transform.position) > PKT_SEND_TOLERANCE)
            {
                // update the server on client player position
                Msg update = MsgBuilder.Server()
                    .Command(Msgs.CMD_POS)
                    .Subcommand(Msgs.SCMD_POS_UPDATE)
                    .SourceID(1) // TODO: actual player ID
                    .Vector(this.transform.position.x, this.transform.position.y, this.transform.position.z)
                    .Build();
                MessageBroker.Instance.Enqueue(update);

                this.lastSentPosition = this.transform.position;
            }
        }

        /// <summary>
        /// Lerps the player object to a desired position.
        /// </summary>
        /// <param name="toPosition">Destination vector.</param>
        /// <returns>Coroutine.</returns>
        IEnumerator BlendSyncPosition(Vector3 toPosition)
        {
            float rate = 1.0f / 0.25f;
            float i = 0f;
            while (i < 1.0f) {
                i += Time.deltaTime * rate;
                this.transform.position = Vector3.Lerp(this.transform.position, toPosition, i);
                yield return null;
            }
            this.ignoreInputDirection = false;
        }

        /// <summary>
        /// Called when the object is about to be destroyed.
        /// </summary>
        void Destroy()
        {
            MessageBroker.Instance.Unsubscribe(this);
        }

        #region ISubscriber implementation

        public void Handle(Msg message)
        {
            float x = 0, y = 0, z = 0;
            if (message.cmd == Msgs.CMD_POS && message.subcmd == Msgs.SCMD_POS_CORRECTION)
            {
                // server is telling us where we're supposed to be, directly update our position
                Debug.LogWarning(String.Format("[PlayerPositionUpdate.Handle] Player position sync to {0}, {1}, {2}", x, y, z));
                this.syncPosition = message.vector.ToVector3();
                this.direction = Vector3.zero;
                this.ignoreInputDirection = true;
            }
            else if (message.cmd == Msgs.CMD_INPUT_MOVE && !this.ignoreInputDirection)
            {
                switch (message.subcmd)
                {
                    case Msgs.SCMD_INPUT_MOVE_SW:
                        x = -1.0f;
                        z = -1.0f;
                        break;
                    case Msgs.SCMD_INPUT_MOVE_SE:
                        x = 1.0f;
                        z = -1.0f;
                        break;
                    case Msgs.SCMD_INPUT_MOVE_NW:
                        x = -1.0f;
                        z = 1.0f;
                        break;
                    case Msgs.SCMD_INPUT_MOVE_NE:
                        x = 1.0f;
                        z = 1.0f;
                        break;
                    case Msgs.SCMD_INPUT_MOVE_E:
                        x = 1.0f;
                        break;
                    case Msgs.SCMD_INPUT_MOVE_N:
                        z = 1.0f;
                        break;
                    case Msgs.SCMD_INPUT_MOVE_S:
                        z = -1.0f;
                        break;
                    case Msgs.SCMD_INPUT_MOVE_W:
                        x = -1.0f;
                        break;
                }
                this.direction = new Vector3(x, 0, z);
            }
        }

        #endregion
    }
}
