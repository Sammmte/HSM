using System.Collections.Generic;

namespace Paps.StateMachines
{
    public delegate void HierarchyChanged<TState>(IEnumerable<TState> previous, IEnumerable<TState> current);
}
