using System;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Game.GUI
{
    public enum SelectionStatus
    {
        /// <summary>
        /// The UI object can be selected.
        /// </summary>
        Normal,

        /// <summary>
        /// The UI object is highlighted.
        /// </summary>
        Highlighted,

        /// <summary>
        /// The UI object is pressed.
        /// </summary>
        Pressed,

        /// <summary>
        /// The UI object is selected
        /// </summary>
        Selected,

        /// <summary>
        /// The UI object cannot be selected.
        /// </summary>
        Disabled,
    }
    public class ButtonWithEvent : Button
    {
        public event Action<SelectionStatus> OnStateTransition;
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state,instant);
            OnStateTransition?.Invoke(FromState(state));
        }

        protected static SelectionStatus FromState(SelectionState state)
        {
            var newState = (SelectionStatus)(int)state;
#if UNITY_EDITOR
            Assert.AreEqual(newState.ToString(),state.ToString());
#endif
            return newState;
        }
    }
}