﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regulus.Project.ItIsNotAGame1.Data
{
    public interface IPlayerProperys
    {

        string Realm { get; }
        Guid Id { get; }

        float Strength { get; }

        float Health { get; }
    }
}
