using System;
using Unity.VisualScripting;
using UnityEngine;

namespace GridLayout
{
    public class Inventory : AbstractGridLayout
    {
        [SerializeField] private GameObject itemPrefab;
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
            Align();
        }

        public override void Align()
        {
            Align(rowCount,columnCount, Spacing, VerticalPadding, HorizontalPadding);
        }

        public override void Align(bool checkChildCount)
        {
            Align(rowCount,columnCount, Spacing, VerticalPadding, HorizontalPadding,checkChildCount);
        }

        private void Update()
        {
            if(!Application.isPlaying)
                Awake();
        }
        
        public void AddItem()
        {
            if(ColumnNum == columnCount && RowNum + 1 >= rowCount)
                throw new ArgumentOutOfRangeException();
            var item = Instantiate(itemPrefab, transform);
            var itemRect = item.GetComponent<RectTransform>();
            var col = ColumnNum;
            var row = RowNum;
            if (ColumnNum == columnCount)
            {
                col = ColumnNum = 0;
                row = ++RowNum;
            }

            itemRect.anchorMax = new Vector2(CellWidth * (col + 1) + Spacing.x * col,  CellHeight * row + CellHeight + Spacing.y * row);
            itemRect.anchorMin = new Vector2(CellWidth * col + Spacing.x * col, CellHeight * row + Spacing.y * row);
            ColumnNum++;
        }
    }
}