using System;
using System.Collections.Generic;
using UnityEngine;

using Example.GameStructures;
using Example.GameStructures.Npc;

namespace Example.Client
{
    /// <summary>
    /// The spawn manager is a simple controller that handles creating/updating monsters in the scene. It works a
    /// lot like the <see cref="MessageBroker"/> but is designed to only handle message packets containing monster
    /// data.
    /// </summary>
    public class SpawnManager : Singleton<SpawnManager>
    {
        #region Private fields
        private const int QUEUE_SIZE = 100;
        private const int INITIAL_MONSTER_SIZE = 100;
        private Queue<Monster> queuedMonsters;
        private Dictionary<int, Monster> monsters;
        private GameObject debugMonster;
        #endregion

        protected SpawnManager()
        {
            this.queuedMonsters = new Queue<Monster>(QUEUE_SIZE);
            this.monsters = new Dictionary<int, Monster>(INITIAL_MONSTER_SIZE);
        }

        void Start()
        {
            // DEBUG: Hardcoded monster prefab
            this.debugMonster = Resources.Load<GameObject>("Monsters/Cha_Slime");
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            while (this.queuedMonsters.Count > 0)
            {
                Monster monster = this.queuedMonsters.Dequeue();
                if (this.monsters.ContainsKey(monster.ObjectID))
                {
                    UpdateMonster(monster);
                }
                else
                {
                    SpawnMonster(monster);
                }
            }
        }

        /// <summary>
        /// Queue an updated monster for the spawn manager to process.
        /// </summary>
        /// <param name="monster">The monster data.</param>
        public void QueueMonsterUpdate(Monster monster)
        {
            if (monster == null)
                throw new ArgumentNullException("monster");
            this.queuedMonsters.Enqueue(monster);
        }

        /// <summary>
        /// Update an existing monster in the scene.
        /// </summary>
        /// <param name="monster">The monster data.</param>
        private void UpdateMonster(Monster monster)
        {
            if (monster.Status == CharacterStatus.Dead)
            {
                this.monsters.Remove(monster.ObjectID);
                // TODO: notifications
            }
            else
            {
                this.monsters[monster.ObjectID] = monster;
                // TODO: notifications
            }
        }

        /// <summary>
        /// Gets a monster that the spawn manager is currently tracking by its world object identifier.
        /// </summary>
        /// <param name="objectID">The object identifier of the monster to get.</param>
        /// <returns>A <see cref="Monster"/> object if one is being tracked; otherwise, null.</returns>
        public Monster GetMonsterByID(int objectID)
        {
            return this.monsters[objectID];
        }

        /// <summary>
        /// Create a new monster in the scene.
        /// </summary>
        /// <param name="monster">The monster data.</param>
        private void SpawnMonster(Monster monster)
        {
            // add to tracking collection
            this.monsters.Add(monster.ObjectID, monster);

            // generate from prefab
            // TODO: handle other monster types
            GameObject obj = (GameObject)GameObject.Instantiate(this.debugMonster, monster.WorldLoc.ToVector3(), Quaternion.LookRotation(monster.Facing.ToVector3(), Vector3.up));
            MonsterBehavior mb = obj.GetComponent<MonsterBehavior>();
            if (mb != null)
                mb.ObjectID = monster.ObjectID;

            // TODO: notifications
        }
    }
}