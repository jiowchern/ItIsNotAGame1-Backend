﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Regulus.Project.ItIsNotAGame1.Data;

namespace Regulus.Project.ItIsNotAGame1.Play.User
{
    public static class GPIHelper
    {
        public static void Equip(this IInventoryController gpi,string id)
        {

            var guid = new Guid(id);
            gpi.Equip(guid);
        }
    }
}
