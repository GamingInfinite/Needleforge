using System;
using System.Collections.Generic;
using System.Text;

namespace Needleforge.Data;

public class UniqueBindEvent(UniqueBindDirection Direction, Action<Action> lambdaMethod)
{
    public UniqueBindDirection Direction = Direction;
    public Action<Action> lambdaMethod = lambdaMethod;
}
