namespace TimeTracker.Web.Core.Helpers
{
    public static class Randomizer
    {
        private static Random _randomizer = new();
        
        public static int GetNumber()
        {
            return _randomizer.Next();
        }
        
        public static int GetNumber(int minValue, int maxValue)
        {
            return _randomizer.Next(minValue, maxValue);
        }
    }
}