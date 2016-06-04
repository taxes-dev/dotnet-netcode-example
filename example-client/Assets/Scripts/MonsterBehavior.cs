using System.Collections;
using UnityEngine;

using Example.GameStructures.Npc;
using Example.Messages;

namespace Example.Client
{
    public class MonsterBehavior : MonoBehaviour, ISubscriber
    {
        public int ObjectID;
        public float SpeedFactor = 5.0f;
        private Vector3 newPosition;

        // Use this for initialization
        void Start()
        {
            // Interested in receiving positional updates from the server
            MessageBroker.Instance.Subscribe(this, Msgs.CMD_POS);
        }

        // Update is called once per frame
        void Update()
        {
            if (this.newPosition != Vector3.zero)
            {
                StartCoroutine(BlendNewPosition(this.newPosition));
                this.newPosition = Vector3.zero;
            }

        }

        void Destroy()
        {
            MessageBroker.Instance.Unsubscribe(this);
        }

        /// <summary>
        /// Lerps the monster object to a desired position.
        /// </summary>
        /// <param name="toPosition">Destination vector.</param>
        /// <returns>Coroutine.</returns>
        IEnumerator BlendNewPosition(Vector3 toPosition)
        {
            float rate = 1.0f / SpeedFactor;
            float i = 0f;
            while (i < 1.0f)
            {
                i += Time.deltaTime * rate;
                this.transform.position = Vector3.Lerp(this.transform.position, toPosition, i);
                yield return null;
            }
            Monster monster = SpawnManager.Instance.GetMonsterByID(this.ObjectID);
            if (monster != null)
                monster.WorldLoc = toPosition.ToVector3D();
        }


        #region ISubscriber implementation

        public void Handle(Msg message)
        {
            if (message.cmd == Msgs.CMD_POS && message.subcmd == Msgs.SCMD_POS_UPDATE && message.target_id == ObjectID)
            {
                this.newPosition = new Vector3(message.vector.X, this.transform.position.y, message.vector.Z);
            }
        }

        #endregion
    }

}