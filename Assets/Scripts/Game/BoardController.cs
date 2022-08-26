using System;
using System.Collections.Generic;
using Config.Board;
using Cysharp.Threading.Tasks;
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
            var column = _boardConfig.SizeX;
            var row = _boardConfig.SizeY;
            var elementOffset = _boardConfig.ElementOffset;
            
            _elements = new Element[column, row];
            
            var startPosition = new Vector2(-elementOffset * column * 0.5f + elementOffset * 0.5f,
                elementOffset * row * 0.5f - elementOffset * 0.5f);

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
            var column = _boardConfig.SizeX;
            var row = _boardConfig.SizeY;
            var elementOffset = _boardConfig.ElementOffset;
            
            var startPosition = new Vector2(-elementOffset * column * 0.5f + elementOffset * 0.5f,
                elementOffset * row * 0.5f - elementOffset * 0.5f);

            _elements = new Element[column, row];
            for (int y = 0; y < row; y++)
            {
                for (int x = 0; x < column; x++)
                {
                    var position = startPosition + new Vector2(elementOffset * x, -elementOffset * y);
                    var element = _factory.Create(new ElementPosition(position, new Vector2(x, y)),
                        GetPossibleElement(x, y, column, row));
                    element.Initialize();
                    _elements[x, y] = element;
                }
            }
        }
        
        public string[] GetBoardState()
        {
            var array = new string[_boardConfig.SizeX * _boardConfig.SizeY];
            var column = _boardConfig.SizeX;
            var row = _boardConfig.SizeY;
            var index = 0;
            for (int y = 0; y < row; y++)
            {
                for (int x = 0; x < column; x++)
                {
                    array[index++] = _elements[x, y].Key;
                }
            }

            return array;
        }

        private ElementConfigItem GetPossibleElement(int column, int row, int columnCount, int rowCount)
        {
            var tempList = new List<ElementConfigItem>(_elementsConfig.ConfigItem);
            int x = column;
            int y = row -1;

            if (x >= 0 && x < columnCount && y >=0 && y < rowCount)
            {
                if (_elements[x, y].IsInitialized)
                {
                    tempList.Remove(_elements[x, y].ConfigItem);
                }
            }

            x = column - 1;
            y = row;
            if (x >= 0 && x < columnCount && y >=0 && y < rowCount)
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
                    ChekBoard(). Forget();
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

        private async UniTaskVoid ChekBoard()
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
                    await DisableElements(elementsForCollecting);
                    _signalBus.Fire(new OnBoardMatchSignal(elementsForCollecting.Count));
                    await NormalizeBoard();
                    isNeedRecheck = true;
                }
            } while (isNeedRecheck);

            _isBlock = false;
        }
        private List<Element> SearchLines()
        {
            List<Element> elementsForCollecting = new List<Element>();

            var column = _boardConfig.SizeX;
            var row = _boardConfig.SizeY;
            for (int y = 0; y < row; y++)
            {
                for (int x = 0; x < column; x++)
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
            var column = _boardConfig.SizeX;
            var row = _boardConfig.SizeY;

            int nextcolumn = x + 1;
            int nextrow = y;

            if (nextcolumn >= column)
                return null;

            List<Element> elementsInLine = new List<Element>();
            Element mainElement = _elements[x, y];

            while (_elements[nextcolumn, nextrow].IsActive && mainElement.Key == _elements[nextcolumn, nextrow].Key)
            {
                elementsInLine.Add(_elements[nextcolumn, nextrow]);
                if (nextcolumn + 1 < column)
                {
                    nextcolumn++;
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
            var column = _boardConfig.SizeX;
            var row = _boardConfig.SizeY;

            int nextcolumn = x;
            int nextrow = y + 1;

            if (nextrow >= row)
                return null;

            List<Element> elementsInLine = new List<Element>();
            Element mainElement = _elements[x, y];

            while (_elements[nextcolumn, nextrow].IsActive && mainElement.Key == _elements[nextcolumn, nextrow].Key)
            {
                elementsInLine.Add(_elements[nextcolumn, nextrow]);
                if (nextrow + 1 < row)
                {
                    nextrow++;
                }
                else
                {
                    break;
                }
            }

            return elementsInLine;
        }

        private async UniTask DisableElements(List<Element> elementsForCollecting)
        {
            var tasks = new List<UniTask>();
            foreach (var element in elementsForCollecting)
            {
                tasks.Add(element.Disable());
            }

           await UniTask.WhenAll(tasks);
        }
        private async UniTask NormalizeBoard()
        {
            var column = _boardConfig.SizeX;
            var row = _boardConfig.SizeY;

            for (int x = column - 1; x >= 0; x--)
            {
                List<Element> freeElements = new List<Element>();
                for (int y = row - 1; y >= 0; y--)
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

            List<UniTask> tasks = new List<UniTask>();
            for (int y = row - 1; y >= 0; y--)
            {
                for (int x = column - 1; x >= 0; x--)
                {
                    if (!_elements[x, y].IsActive)
                    {
                        GenerateRandomElement(_elements[x, y], column, row);
                        tasks.Add(_elements[x, y].Enable());
                    }
                }
            }

            await UniTask.WhenAll(tasks);
        }

        private void GenerateRandomElement(Element element, int column, int row)
        {
            Vector2 gridPosition = element.GridPosition;
            var elements = GetPossibleElement((int) gridPosition.x, (int) gridPosition.y, column, row);
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

        public async void Restart()
        {
            var column = _boardConfig.SizeX;
            var row = _boardConfig.SizeY;
            
            for (int y = 0; y < row; y++)
            {
                for (int x = 0; x < column; x++)
                {
                    _elements[x, y].DestroySelf();
                }
            }

            _elements = null;
            await UniTask.Yield();
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