using UnityEngine;

using Example.Messages;

namespace Example.Client
{
    /// <summary>
    /// Animates the knight model (requires legacy animation code, unfortunately).
    /// </summary>
    public class KnightAnimator : MonoBehaviour, ISubscriber
    {
        #region Private fields
        private const string WAIT_ANIM = "Wait";
        private const string WALK_ANIM = "Walk";
        private const string ATTACK_ANIM = "Attack";
        private const string ATK_SOUND = "knife_stab";
        private const string WALK_SOUND = "footsteps_on_soft_foam_play_mat";
        private Animation _animation;
        private Rigidbody _rigidbody;
        private bool attackPrimary = false;
        private AudioSource atkAudio;
        private AudioSource walkAudio;
        #endregion

        // Use this for initialization
        void Start()
        {
            this._animation = this.GetComponent<Animation>();
            this._rigidbody = this.GetComponent<Rigidbody>();
            AudioSource[] clips = this.GetComponents<AudioSource>();
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].clip.name == ATK_SOUND)
                    this.atkAudio = clips[i];
                if (clips[i].clip.name == WALK_SOUND)
                    this.walkAudio = clips[i];
            }

            // We want to know about player attack input so we can animate it
            MessageBroker.Instance.Subscribe(this, Msgs.CMD_INPUT_ATK);
        }

        void Destroy()
        {
            MessageBroker.Instance.Unsubscribe(this);
        }

        // Update is called once per frame
        void Update()
        {
            if (this._animation != null && this._rigidbody != null)
            {
                if (this.attackPrimary)
                {
                    this._animation.Play(ATTACK_ANIM, PlayMode.StopAll);
                    this.attackPrimary = false;
                    this.atkAudio.Play();
                }
                else if (this._rigidbody.velocity.magnitude > 0.1f)
                {
                    if (!this._animation.IsPlaying(WALK_ANIM))
                        this._animation.CrossFade(WALK_ANIM, 0.5f, PlayMode.StopAll);
                    if (!this.walkAudio.isPlaying)
                        this.walkAudio.Play();
                }
                else if (!this._animation.IsPlaying(WAIT_ANIM))
                {
                    this._animation.CrossFade(WAIT_ANIM, 0.5f, PlayMode.StopAll);
                    this.walkAudio.Stop();
                }
            }
        }

        #region ISubscriber implementation

        public void Handle(Msg message)
        {
            if (message.subcmd == Msgs.SCMD_INPUT_ATK_PRIMARY)
            {
                this.attackPrimary = true;
            }
        }

        #endregion
    }
}
