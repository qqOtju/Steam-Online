using UnityEngine;

namespace GridLayout
{
    [ExecuteInEditMode]
    public abstract class AbstractGridLayout : MonoBehaviour
    {
        protected Transform GridTransform;
        protected float CellHeight;
        protected float CellWidth;
        protected int ChildCount;

        protected int ColumnNum;
        protected int RowNum;
        
        protected void Awake()
        {
            GridTransform = transform;
            ChildCount = GridTransform.childCount;
        }

        protected void Align(int rows, int columns, Vector2 spacing, Vector2 paddingVertical, Vector2 paddingHorizontal, bool checkChildCount = false)
        {
            if(checkChildCount)
            {
                ChildCount = transform.childCount;
            }
            if(ChildCount == 0)
            {
                return;
            }
            var exit = false;
            CellWidth = 1 / (float)columns - spacing.x + spacing.x/columns - paddingHorizontal.x / columns - paddingHorizontal.y/columns;
            CellHeight = 1 / (float)rows - spacing.y + spacing.y / rows - paddingVertical.x / rows - paddingVertical.y / rows;
            
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if(i * columns + j >= ChildCount)
                    {
                        exit = true;
                        break;
                    }
                    
                    var childRect = GridTransform.GetChild(i * columns + j).GetComponent<RectTransform>();
                    childRect.anchorMax = new Vector2(CellWidth * (j + 1) + spacing.x * j + paddingHorizontal.x,  CellHeight * i + CellHeight + spacing.y * i  + paddingVertical.x);
                    childRect.anchorMin = new Vector2(CellWidth * j + spacing.x * j + paddingHorizontal.x, CellHeight * i + spacing.y * i  + paddingVertical.x);
                    childRect.offsetMax = Vector2.zero;
                    childRect.offsetMin = Vector2.zero;
                    ColumnNum = j + 1;
                }
                if (exit)
                    break;
                RowNum = i + 1;
            }
        }

        public abstract void Align();
        public abstract void Align(bool checkChildCount);
    }
}