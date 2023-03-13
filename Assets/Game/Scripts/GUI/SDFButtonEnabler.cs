using Beamable.UI.Buss;
using Game.GUI;
using UnityEngine;

public class SDFButtonEnabler : MonoBehaviour
{
    [SerializeField] private BussElement _element;
    [SerializeField] private ButtonWithEvent _selectable;

    private void OnValidate()
    {
        if(_element == null)
        {
            _element = GetComponent<BussElement>();
        }
        if (_selectable == null)
        {
            _selectable = GetComponent<ButtonWithEvent>();
        }
    }

    private void OnEnable()
    {
        _selectable.OnStateTransition += HandleStateTransition;
    }

    private void HandleStateTransition(SelectionStatus status)
    {
        switch (status)
        {
            case SelectionStatus.Normal:
                _element.SetClass(".disabled", false);
                break;
            case SelectionStatus.Disabled:
                _element.SetClass(".disabled", true);
                break;
        }
    }
}
