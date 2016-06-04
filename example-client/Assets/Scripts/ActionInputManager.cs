using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Example.Messages;

namespace Example.Client
{
    /// <summary>
    /// Handles input for player action commands.
    /// </summary>
    public class ActionInputManager : MonoBehaviour
    {
        /// <summary>
        /// Update is called once per frame. 
        /// </summary>
        void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                var pointer = new PointerEventData(EventSystem.current);
                if (Input.touchSupported)
                {
                    pointer.position = Input.touches[0].position;
                }
                else
                {
                    pointer.position = Input.mousePosition;
                }
                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, results);
                if (results.Count > 0)
                {
                    return; // user is clicking on a UI item
                }

                // Notify other objects that the "attack" button was pressed
                MessageBroker.Instance.Enqueue(Msg.ClientMsg(Msgs.CMD_INPUT_ATK, Msgs.SCMD_INPUT_ATK_PRIMARY));
            }
        }
    }
}