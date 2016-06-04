using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Example.Messages;

namespace Example.Client
{
    /// <summary>
    /// Outputs console messages to the console log UI.
    /// </summary>
    /// <remarks>
    /// This is currently unused, as I never implemented chat payloads in the message system.
    /// </remarks>
    public class UIConsoleOutput : MonoBehaviour, ISubscriber
    {
        #region Private fields
        private ScrollRect guiScroll;
        private Text guiTxt;
        private List<String> messages; // TODO: create a ring buffer to prevent infinite backlog
        private bool dirty;
        #endregion

        public GameObject ScrollView;

        /// <summary>
        /// Use this for initialization.
        /// </summary>
        void Start()
        {
            this.dirty = false;
            this.messages = new List<string>();
            this.guiTxt = this.GetComponent<Text>();
            if (this.ScrollView != null)
            {
                this.guiScroll = this.ScrollView.GetComponent<ScrollRect>();
            }

            // Wait for messages intended to be sent to the console
            MessageBroker.Instance.Subscribe(this, Msgs.CMD_CONSOLE);
        }

        void FixedUpdate()
        {
            if (this.guiTxt != null && this.dirty)
            {
                this.guiTxt.text = String.Join("\n", messages.ToArray());
                this.dirty = false;
                if (this.guiScroll != null)
                {
                    this.guiScroll.verticalScrollbar.value = 0f;
                }
            }
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
            string prefix;
            switch (message.subcmd)
            {
                case Msgs.SCMD_CONSOLE_BATTLE:
                    prefix = "[Combat] ";
                    break;
                case Msgs.SCMD_CONSOLE_SAY:
                    prefix = "[Say] ";
                    break;
                case Msgs.SCMD_CONSOLE_SYSTEM:
                    prefix = "[System] ";
                    break;
                default:
                    prefix = "";
                    break;
            }
            this.messages.Add(prefix + message.ToString());
            this.dirty = true;
        }

        #endregion

    }
}
