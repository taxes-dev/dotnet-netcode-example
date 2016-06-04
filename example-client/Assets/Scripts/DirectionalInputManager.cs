using UnityEngine;

using Example.Messages;

namespace Example.Client
{
    /// <summary>
    /// Handles input for basic movement controls.
    /// </summary>
    public class DirectionalInputManager : MonoBehaviour
    {
        #region Private fields
        private float lastDirection = Msgs.SCMD_INPUT_MOVE_STOP;
        private Vector3 touchPosStart, touchPosEnd;
        #endregion

        /// <summary>
        /// Sensitivity for horizontal movement (non-touch).
        /// </summary>
        public float HorizontalThreshold = 0.125f;

        /// <summary>
        /// Sensitivity for vertical movement (non-touch).
        /// </summary>
        public float VerticalThreshold = 0.125f;

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            ushort direction = Msgs.SCMD_INPUT_MOVE_STOP;
            float horiz, vert;
            GetInputAxes(out horiz, out vert);

            // This is kind of confusing, but the input manager is translating control input directions to world directions
            // relative to the camera. Thus, if the isometric angles used by the camera change this code needs to be updated.
            // Since I don't plan to change that, this is a more efficient solution than doing quaternion math on every frame
            // to handle possible changing camera angles. Keep in mind that the world is skewed 45 degrees to the right, so on
            // an 8-direction grid, everything shifts counter-clockwise 1 place.
            if (IsDown(vert) && IsLeft(horiz))
            {
                // down+left (SW) = S
                direction = Msgs.SCMD_INPUT_MOVE_S;
            }
            else if (IsDown(vert) && IsRight(horiz))
            {
                // down+right (SE) = E
                direction = Msgs.SCMD_INPUT_MOVE_E;
            }
            else if (IsUp(vert) && IsLeft(horiz))
            {
                // up+left (NW) = W
                direction = Msgs.SCMD_INPUT_MOVE_W;
            }
            else if (IsUp(vert) && IsRight(horiz))
            {
                // up+right (NE) = N
                direction = Msgs.SCMD_INPUT_MOVE_N;
            }
            else if (IsLeft(horiz))
            {
                // left (W) = SW
                direction = Msgs.SCMD_INPUT_MOVE_SW;
            }
            else if (IsRight(horiz))
            {
                // right (E) = NE
                direction = Msgs.SCMD_INPUT_MOVE_NE;
            }
            else if (IsDown(vert))
            {
                // down (S) = SE
                direction = Msgs.SCMD_INPUT_MOVE_SE;
            }
            else if (IsUp(vert))
            {
                // up (N) = NW
                direction = Msgs.SCMD_INPUT_MOVE_NW;
            }

            // Prevent STOP spam
            if (!(direction == Msgs.SCMD_INPUT_MOVE_STOP && direction == lastDirection))
            {
                lastDirection = direction;
                // Notify other objects that the player wants to move in the specified direction
                MessageBroker.Instance.Enqueue(Msg.ClientMsg(Msgs.CMD_INPUT_MOVE, direction));
            }
        }

        #region Helper methods
        /// <summary>
        /// Returns a set of input axes based on the current active controls.
        /// </summary>
        /// <param name="horiz">Returns a horizontal direction from -1.0 to 1.0.</param>
        /// <param name="vert">Returns a vertical direction from -1.0 to 1.0.</param>
        private void GetInputAxes(out float horiz, out float vert)
        {
            if (Input.touchSupported)
            {
                GetTouchControlAxes(out horiz, out vert);
            }
            else
            {
                GetStandardControlAxes(out horiz, out vert);
            }
        }

        /// <summary>
        /// Returns a set of input axes based on touch control.
        /// </summary>
        /// <param name="horiz">Returns a horizontal direction from -1.0 to 1.0.</param>
        /// <param name="vert">Returns a vertical direction from -1.0 to 1.0.</param>
        private void GetTouchControlAxes(out float horiz, out float vert)
        {
            horiz = 0f;
            vert = 0f;

            if (Input.touchCount == 0)
            {
                touchPosStart = Vector3.zero;
                touchPosEnd = Vector3.zero;
                return;
            }

            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchPosStart = touch.position;
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Ended:
                    touchPosEnd = touch.deltaPosition;
                    break;
                case TouchPhase.Stationary:
                    break;
                case TouchPhase.Canceled:
                    touchPosStart = Vector3.zero;
                    touchPosEnd = Vector3.zero;
                    break;
            }

            if (touchPosStart != Vector3.zero && touchPosEnd != Vector3.zero)
            {
                // calculate angle to figure out which direction user pointed
                Vector3 endNormal = touchPosEnd.normalized;
                float angle = Vector3.Angle(Vector3.up, endNormal);
                int cardinal = 45 * (int)Mathf.Round(angle / 45.0f);

                // HACK: this is probably terrible
                float dir = -Vector3.up.x * endNormal.y + Vector3.up.y * endNormal.x;
                if (dir < 0)
                {
                    cardinal = 360 - cardinal;
                }
                else if (dir == 0 && endNormal.y < 0f)
                {
                    cardinal = 180;
                }
                
                switch (cardinal)
                {
                    case 45:
                        // NE
                        vert = 1.0f;
                        horiz = 1.0f;
                        break;
                    case 90:
                        // E
                        horiz = 1.0f;
                        break;
                    case 135:
                        // SE
                        vert = -1.0f;
                        horiz = 1.0f;
                        break;
                    case 180:
                        // S
                        vert = -1.0f;
                        break;
                    case 225:
                        // SW
                        vert = -1.0f;
                        horiz = -1.0f;
                        break;
                    case 270:
                        // W
                        horiz = -1.0f;
                        break;
                    case 315:
                        // NW
                        vert = 1.0f;
                        horiz = -1.0f;
                        break;
                    default:
                        // N
                        vert = 1.0f;
                        break;
                }
            }
        }

        /// <summary>
        /// Returns a set of input axes based on non-touch control.
        /// </summary>
        /// <param name="horiz">Returns a horizontal direction from -1.0 to 1.0.</param>
        /// <param name="vert">Returns a vertical direction from -1.0 to 1.0.</param>
        private void GetStandardControlAxes(out float horiz, out float vert)
        {
            horiz = Input.GetAxis("Horizontal");
            vert = Input.GetAxis("Vertical");
        }

        /// <summary>
        /// Tests to see if the horizontal control is pushed to the left.
        /// </summary>
        /// <param name="horiz">The horizontal direction.</param>
        /// <returns>True if the horizontal control is pushed to the left; otherwise, false.</returns>
        private bool IsLeft(float horiz)
        {
            return horiz < HorizontalThreshold * -1.0f;
        }

        /// <summary>
        /// Tests to see if the horizontal control is pushed to the right.
        /// </summary>
        /// <param name="horiz">The horizontal direction.</param>
        /// <returns>True if the horizontal control is pushed to the right; otherwise, false.</returns>
        private bool IsRight(float horiz)
        {
            return horiz > HorizontalThreshold;
        }

        /// <summary>
        /// Tests to see if the vertical control is pushed up.
        /// </summary>
        /// <param name="vert">The vertical direction.</param>
        /// <returns>True if the vertical control is pushed up; otherwise, false.</returns>
        private bool IsUp(float vert)
        {
            return vert > VerticalThreshold;
        }

        /// <summary>
        /// Tests to see if the vertical control is pushed down.
        /// </summary>
        /// <param name="vert">The vertical direction.</param>
        /// <returns>True if the vertical control is pushed down; otherwise, false.</returns>
        private bool IsDown(float vert)
        {
            return vert < VerticalThreshold * -1.0f;
        }
        #endregion

    }
}
