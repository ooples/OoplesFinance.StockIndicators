using System.Runtime.Serialization;

namespace OoplesFinance.StockIndicators.Exceptions
{
    [Serializable]
    public sealed class CalculationException : Exception
    {
        public CalculationException()
        {

        }

        public CalculationException(string message) : base(message)
        {

        }

        public CalculationException(string message, Exception inner) : base(message, inner)
        {

        }

        private CalculationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}
