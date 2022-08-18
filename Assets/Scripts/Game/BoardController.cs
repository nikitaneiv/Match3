using System;
using System.Collections.Generic;
using Config.Board;
using Signals;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Game
{
    public class BoardController: IDisposable
    {
        private readonly BoardConfig _boardConfig;
        private readonly ElementsConfig _elementsConfig;
        private readonly Element.Factory _factory;
        private readonly SignalBus _signalBus;
        private readonly DiContainer _container;

        private Element[,] _elements;
        private Element _firstSelected;
        private bool _isBlock;

        public BoardController(BoardConfig boardConfig, ElementsConfig elementsConfig, Element.Factory factory, SignalBus signalBus)
        {
            _boardConfig = boardConfig;
            _elementsConfig = elementsConfig;
            _factory = factory;
            _signalBus = signalBus;
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
            _signalBus.Subscribe<OnElementSignal>(OnElementClick);
        }

        private void LoadElements(string[] dataBoardState)
        {
            var row = _boardConfig.SizeX;
            var column = _boardConfig.SizeY;
            var elementOffset = _boardConfig.ElementOffset;
            
            _elements = new Element[row, column];
            
            var startPosition = new Vector2(-elementOffset * row * 0.5f + elementOffset * 0.5f,
                elementOffset * column * 0.5f - elementOffset * 0.5f);

            for (int i = 0; i < dataBoardState.Length; i++)
            {
                int y = i / _boardConfig.SizeX;
                int x = i % _boardConfig.SizeX;
                
                var position = startPosition + new Vector2(elementOffset * x, -elementOffset * y);
                var element = _factory.Create(new ElementPosition(position, new Vector2(x, y)),
                    _elementsConfig.GetByKey(dataBoardState[i]));
                element.Initialize();
                _elements[x, y] = element;
            }
        }

        private void GenerateElements() 
        {
            var row = _boardConfig.SizeX;
            var column = _boardConfig.SizeY;
            var elementOffset = _boardConfig.ElementOffset;
            
            var startPosition = new Vector2(-elementOffset * row * 0.5f + elementOffset * 0.5f,
                elementOffset * column * 0.5f - elementOffset * 0.5f);

            _elements = new Element[row, column];
            for (int y = 0; y < column; y++)
            {
                for (int x = 0; x < row; x++)
                {
                    var position = startPosition + new Vector2(elementOffset * x, -elementOffset * y);
                    var element = _factory.Create(new ElementPosition(position, new Vector2(x, y)),
                        GetPossibleElement(x, y, row, column));
                    element.Initialize();
                    _elements[x, y] = element;
                }
            }
        }
        
        public string[] GetBoardState()
        {
            var array = new string[_boardConfig.SizeX * _boardConfig.SizeY];
            var row = _boardConfig.SizeX;
            var column = _boardConfig.SizeY;
            var index = 0;
            for (int y = 0; y < column; y++)
            {
                for (int x = 0; x < row; x++)
                {
                    array[index++] = _elements[x, y].Key;
                }
            }

            return array;
        }

        private ElementConfigItem GetPossibleElement(int row, int column, int rowCount, int columnCount)
        {
            var tempList = new List<ElementConfigItem>(_elementsConfig.ConfigItem);
            int x = row;
            int y = column -1;

            if (x >= 0 && x < rowCount && y >=0 && y < columnCount)
            {
                if (_elements[x, y].IsInitialized)
                {
                    tempList.Remove(_elements[x, y].ConfigItem);
                }
            }

            x = row - 1;
            y = column;
            if (x >= 0 && x < rowCount && y >=0 && y < columnCount)
            {
                if (_elements[x, y].IsInitialized)
                {
                    tempList.Remove(_elements[x, y].ConfigItem);
                }
            }
            return tempList[Random.Range(0, tempList.Count)];
        }

        private void OnElementClick(OnElementSignal signal)
        {
            if (_isBlock) return;
            
            var element = signal.Element;
            if (_firstSelected == null)
            {
                _firstSelected = element;
                element.SetSelected(true);
            }
            else
            {
                if (IsCanSwap(_firstSelected, element))  
                {
                    _firstSelected.SetSelected(false);
                    Swap(_firstSelected, element);
                    _firstSelected = null;
                    ChekBoard(); 
                }
                else
                {
                    if (_firstSelected == element)
                    {
                        _firstSelected.SetSelected(false);
                        _firstSelected = null;
                    }
                    else
                    {
                        _firstSelected.SetSelected(false);
                        _firstSelected = element;
                        element.SetSelected(true);
                    }
                }
            }
        }

        private void ChekBoard()
        {
            _isBlock = true;
            bool isNeedRecheck;
            List<Element> elementsForCollecting = new List<Element>();
            
            do
            {
                isNeedRecheck = false;
                elementsForCollecting.Clear();

                elementsForCollecting = SearchLines();

                if (elementsForCollecting.Count > 0)
                {
                    DisableElements(elementsForCollecting);
                    _signalBus.Fire(new OnBoardMatchSignal(elementsForCollecting.Count));
                    NormalizeBoard();
                    isNeedRecheck = true;
                }
            } while (isNeedRecheck);

            _isBlock = false;
        }
        private List<Element> SearchLines()
        {
            List<Element> elementsForCollecting = new List<Element>();

            var row = _boardConfig.SizeX;
            var column = _boardConfig.SizeY;
            for (int y = 0; y < column; y++)
            {
                for (int x = 0; x < row; x++)
                {
                    if (_elements[x, y].IsActive && !elementsForCollecting.Contains(_elements[x, y]))
                    {
                        List<Element> checkResult;
                        bool needAddFirst = false;
                        checkResult = CheckHorizontal(x, y);
                        if (checkResult != null && checkResult.Count >= 2)
                        {
                            needAddFirst = true;
                            elementsForCollecting.AddRange(checkResult);
                        }

                        checkResult = CheckVertical(x, y);
                        if (checkResult != null && checkResult.Count >= 2)
                        {
                            needAddFirst = true;
                            elementsForCollecting.AddRange(checkResult);
                        }

                        if (needAddFirst)
                        {
                            elementsForCollecting.Add(_elements[x, y]);
                        }
                    }
                }
            }

            return elementsForCollecting;
        }

        private List<Element> CheckHorizontal(int x, int y)
        {
            var row = _boardConfig.SizeX;
            var column = _boardConfig.SizeY;

            int nextRow = x + 1;
            int nextColumn = y;

            if (nextRow >= row)
                return null;

            List<Element> elementsInLine = new List<Element>();
            Element mainElement = _elements[x, y];

            while (_elements[nextRow, nextColumn].IsActive && mainElement.Key == _elements[nextRow, nextColumn].Key)
            {
                elementsInLine.Add(_elements[nextRow, nextColumn]);
                if (nextRow + 1 < row)
                {
                    nextRow++;
                }
                else
                {
                    break;
                }
            }

            return elementsInLine;
        }
        
        private List<Element> CheckVertical(int x, int y)
        {
            var row = _boardConfig.SizeX;
            var column = _boardConfig.SizeY;

            int nextRow = x;
            int nextColumn = y + 1;

            if (nextColumn >= column)
                return null;

            List<Element> elementsInLine = new List<Element>();
            Element mainElement = _elements[x, y];

            while (_elements[nextRow, nextColumn].IsActive && mainElement.Key == _elements[nextRow, nextColumn].Key)
            {
                elementsInLine.Add(_elements[nextRow, nextColumn]);
                if (nextColumn + 1 < column)
                {
                    nextColumn++;
                }
                else
                {
                    break;
                }
            }

            return elementsInLine;
        }

        private void DisableElements(List<Element> elementsForCollecting)
        {
            foreach (var element in elementsForCollecting)
            {
                element.Disable();
            }
        }
        private void NormalizeBoard()
        {
            var row = _boardConfig.SizeX;
            var column = _boardConfig.SizeY;

            for (int x = row - 1; x >= 0; x--)
            {
                List<Element> freeElements = new List<Element>();
                for (int y = column - 1; y >= 0; y--)
                {
                    while (y >= 0 && !_elements[x, y].IsActive)
                    {
                        freeElements.Add(_elements[x, y]);
                        y--;
                    }

                    if (y >= 0 && freeElements.Count > 0)
                    {
                        Swap(_elements[x, y], freeElements[0]);
                        freeElements.Add(freeElements[0]);
                        freeElements.RemoveAt(0);
                    }
                }
            }

            for (int y = column - 1; y >= 0; y--)
            {
                for (int x = row - 1; x >= 0; x--)
                {
                    if (!_elements[x, y].IsActive)
                    {
                        GenerateRandomElement(_elements[x, y], row, column);
                        _elements[x, y].Enable();
                    }
                }
            }
        }

        private void GenerateRandomElement(Element element, int row, int column)
        {
            Vector2 gridPosition = element.GridPosition;
            var elements = GetPossibleElement((int) gridPosition.x, (int) gridPosition.y, row, column);
            element.SetConfig(elements);
        }

        private void Swap(Element first, Element second)
        {
            _elements[(int)first.GridPosition.x, (int)first.GridPosition.y] = second;
            _elements[(int)second.GridPosition.x, (int)second.GridPosition.y] = first;

            Vector2 position = second.transform.localPosition;
            Vector2 gridPosition = second.GridPosition;
            second.SetLocalPosition(first.transform.localPosition, first.GridPosition);
            first.SetLocalPosition(position, gridPosition);
        }

        public void Restart()
        {
            var row = _boardConfig.SizeX;
            var column = _boardConfig.SizeY;
            
            for (int y = 0; y < column; y++)
            {
                for (int x = 0; x < row; x++)
                {
                    _elements[x, y].DestroySelf();
                }
            }

            _elements = null;
            GenerateElements();
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<OnElementSignal>(OnElementClick);
        }

        private bool IsCanSwap(Element first, Element second)
        {
            Vector2 comparePosition = first.GridPosition;
            comparePosition.x += 1;
            if (comparePosition == second.GridPosition)
            {
                return true;
            }

            comparePosition = first.GridPosition;
            comparePosition.x -= 1;
            if (comparePosition == second.GridPosition)
            {
                return true;
            }
            comparePosition = first.GridPosition;
            comparePosition.y += 1;
            if (comparePosition == second.GridPosition)
            {
                return true;
            }
            comparePosition = first.GridPosition;
            comparePosition.y -= 1;
            if (comparePosition == second.GridPosition)
            {
                return true;
            }

            return false;
        }
    }
}