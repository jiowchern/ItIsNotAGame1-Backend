﻿using System;

namespace Regulus.Project.ItIsNotAGame1.Data
{
    public interface IMoveController
    {
        void RunForward();
        void Forward();
        void Backward();

        void StopMove();

        void TrunLeft();

        void TrunRight();

        void StopTrun();

        
    }
}