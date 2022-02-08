using System;
using System.Collections.Generic;

class DamageBoost : Boost
{
    public DamageBoost()
    {
        isStackable = true;
        duration = 5f;
    }
}


