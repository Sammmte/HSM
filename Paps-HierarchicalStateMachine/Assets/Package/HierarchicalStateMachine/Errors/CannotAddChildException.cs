using System;

namespace Paps.StateMachines
{
    public class CannotAddChildException : Exception
    {
        public CannotAddChildException()
        {

        }

        public CannotAddChildException(string message) : base(message)
        {

        }
    }
}