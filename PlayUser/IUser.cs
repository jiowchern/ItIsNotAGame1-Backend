﻿using Regulus.Remoting;
using Regulus.Utility;



namespace Regulus.Project.ItIsNotAGame1
{
	public interface IUser : IUpdatable
	{
		Remoting.User Remoting { get; }

		INotifier<Data.IVerify> VerifyProvider { get; }

        INotifier<Data.IVisible> VisibleProvider { get; }

        INotifier<Data.IMoveController> MoveControllerProvider { get; }

        INotifier<Data.IPlayerProperys> PlayerProperysProvider { get; }

        INotifier<Data.IAccountStatus> AccountStatusProvider { get; }

        INotifier<Data.IInventoryNotifier> InventoryNotifierProvider { get; }
        INotifier<Data.ISkillController> SkillControllerProvider { get; }

        INotifier<Data.IActorStatus> ActorStatusProvider { get; }


    }
}
