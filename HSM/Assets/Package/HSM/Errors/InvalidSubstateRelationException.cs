using System;

namespace Paps.FSM.HSM
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