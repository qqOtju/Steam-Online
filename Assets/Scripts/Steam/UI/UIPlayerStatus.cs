using TMPro;
using UnityAtoms.BaseAtoms;

namespace Steam.UI
{
    public class UIPlayerStatus
    {
        private readonly TextMeshProUGUI _groundedStatusText;
        private readonly TextMeshProUGUI _sprintStatusText;
        private readonly BoolEvent _groundedChange;
        private readonly BoolEvent _sprintChange;
        
        public UIPlayerStatus(BoolEvent groundedChange, BoolEvent sprintChange, TextMeshProUGUI groundedStatusText, TextMeshProUGUI sprintStatusText)
        {
            _groundedStatusText = groundedStatusText;
            _sprintStatusText = sprintStatusText;
            _groundedChange = groundedChange;
            _sprintChange = sprintChange;
            _groundedChange.Register(OnGroundedChange);
            _sprintChange.Register(OnSprintChange);
        }
        
        public void OnDestroy()
        {
            _groundedChange.Unregister(OnGroundedChange);
            _sprintChange.Unregister(OnSprintChange);
        }

        private void OnGroundedChange(bool value) =>
            _groundedStatusText.text = value ? $"Grounded: <color=green>{value}</color>" : $"Grounded: <color=red>{value}</color>";

        private void OnSprintChange(bool value) =>
            _sprintStatusText.text = value ? $"Sprint: <color=green>{value}</color>" : $"Sprint: <color=red>{value}</color>";
        
    }
}