using UnityEngine;

namespace GridLayout
{
    public class VerticalLayout : AbstractGridLayout
    {
        [Min(1)] [SerializeField] private int _rowCount;
        [SerializeField] private Vector2 _spacing;
        [SerializeField] private Vector2 _horizontalPadding;
        [SerializeField] private Vector2 _verticalPadding;
        private Vector2 Spacing => _spacing / 1000;
        private Vector2 HorizontalPadding => _horizontalPadding / 1000;
        private Vector2 VerticalPadding => _verticalPadding / 1000;
        private void Awake()
        {
            base.Awake();
            Align();
        }

        public override void Align() =>
            Align(_rowCount,1, Spacing, VerticalPadding, HorizontalPadding);

        public override void Align(bool checkChildCount)=>
            Align(_rowCount,1, Spacing, VerticalPadding, HorizontalPadding,checkChildCount);
        
        private void Update()
        {
            if(!Application.isPlaying )
                Awake();
        }
    }
}