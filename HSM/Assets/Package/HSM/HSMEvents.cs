using System.Collections.Generic;

namespace Paps.FSM.HSM
{
    public delegate void HierarchyChanged<TState>(IEnumerable<TState> previous, IEnumerable<TState> current);
}
