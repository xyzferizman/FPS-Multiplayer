﻿using System;
using System.Collections.Generic;

class SpeedBoost : Boost
{    
    public SpeedBoost()
    {
        isStackable = false;
        duration = 3f;
    }
}