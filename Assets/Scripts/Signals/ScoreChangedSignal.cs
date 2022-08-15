namespace Signals
{
    public class ScoreChangedSignal
    {
        public readonly int Value;

        public ScoreChangedSignal(int value)
        {
            Value = value;
        }
    }
}