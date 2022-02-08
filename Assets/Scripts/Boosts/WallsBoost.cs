using System;
using System.Collections.Generic;

class WallsBoost : Boost
{
    public WallsBoost()
    {
        isStackable = false;
        duration = 4f;
    }
}

