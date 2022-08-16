using System.Collections.Generic;
using Config.Board;
using UnityEngine;

namespace Game
{
    public class BoardController
    {
        private readonly BoardConfig _boardConfig;
        private readonly ElementsConfig _elementsConfig;
        private readonly Element.Factory _factory;

        private Element[,] _elements;
        private Element _firstSelected;
        private bool _isBlock;

        public BoardController(BoardConfig boardConfig, ElementsConfig elementsConfig, Element.Factory factory)
        {
            _boardConfig = boardConfig;
            _elementsConfig = elementsConfig;
            _factory = factory;
        }

        public void Initialize(string[] dataBoardState = null)
        {
            
            if (dataBoardState == null) 
            {
                GenerateElements();
            }
            else
            {
                LoadElements(dataBoardState);
            }
        }

        private void LoadElements(string[] dataBoardState)
        {
            var row = _boardConfig.SizeX;
            var column = _boardConfig.SizeY;
            var elementOffset = _boardConfig.ElementOffset;
            
            _elements = new Element[column, row];
            
            var startPosition = new Vector2(-elementOffset * row * 0.5f + elementOffset * 0.5f,
                elementOffset * column * 0.5f - elementOffset * 0.5f);

            for (int i = 0; i < dataBoardState.Length; i++)
            {
                int y = i / _boardConfig.SizeX;
                int x = i % _boardConfig.SizeX;
                
                var position = startPosition + new Vector2(elementOffset * x, -elementOffset * y);
                var element = _factory.Create(new ElementPosition(position, new Vector2(y, x)),
                    _elementsConfig.GetByKey(dataBoardState[i]));
                element.Initialize();
                _elements[y, x] = element;
            }
        }

        private void GenerateElements()
        {
            var row = _boardConfig.SizeX;
            var column = _boardConfig.SizeY;
            var elementOffset = _boardConfig.ElementOffset;
            
            var startPosition = new Vector2(-elementOffset * row * 0.5f + elementOffset * 0.5f,
                elementOffset * column * 0.5f - elementOffset * 0.5f);

            _elements = new Element[column, row];
            for (int y = 0; y < column; y++)
            {
                for (int x = 0; x < row; x++)
                {
                    var position = startPosition + new Vector2(elementOffset * x, -elementOffset * y);
                    var element = _factory.Create(new ElementPosition(position, new Vector2(y, x)),
                        GetPossibleElement(x, y, row, column));
                    element.Initialize();
                    _elements[y, x] = element;
                }
            }
        }

        private ElementConfigItem GetPossibleElement(int row, int column, int rowCount, int columnCount)
        {
            var tempList = new List<ElementConfigItem>(_elementsConfig.ConfigItem);
            int x = row;
            int y = column -1;

            if (x >= 0 && x < rowCount && y >=0 && y < columnCount)
            {
                if (_elements[y, x].IsInitialized)
                {
                    tempList.Remove(_elements[y, x].ConfigItem);
                }
            }

            x = row - 1;
            y = column;
            if (x >= 0 && x < rowCount && y >=0 && y < columnCount)
            {
                if (_elements[y, x].IsInitialized)
                {
                    tempList.Remove(_elements[y, x].ConfigItem);
                }
            }
            return tempList[Random.Range(0, tempList.Count)];
            
        }
    }
}