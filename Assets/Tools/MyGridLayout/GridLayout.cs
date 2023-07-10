using UnityEngine;

namespace GridLayout
{
    public class GridLayout : AbstractGridLayout
    {
        [Min(1)] [SerializeField] private int columnCount;
        [Min(1)] [SerializeField] private int rowCount;
        [SerializeField] private Vector2 spacing;
        [SerializeField] private Vector2 horizontalPadding;
        [SerializeField] private Vector2 verticalPadding;
        
        private Vector2 Spacing => spacing / 1000;
        private Vector2 HorizontalPadding => horizontalPadding / 1000;
        private Vector2 VerticalPadding => verticalPadding / 1000;
        
        private void Awake()
        {
            base.Awake();
            Align(rowCount,columnCount, Spacing, VerticalPadding, HorizontalPadding);
        }
        
        private void Update()
        {
            if(!Application.isPlaying )
                Awake();
        }

        public override void Align()
        {
            Align(rowCount,columnCount, Spacing, VerticalPadding, HorizontalPadding);
        }

        public override void Align(bool checkChildCount)
        {
            Align(rowCount,columnCount, Spacing,  VerticalPadding, HorizontalPadding, checkChildCount);
        }
    }
}