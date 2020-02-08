using System;

namespace Paps.StateMachines
{
    public class InvalidSubstateRelationException : Exception
    {
        public InvalidSubstateRelationException()
        {

        }

        public InvalidSubstateRelationException(string message) : base(message)
        {

        }
    }
}