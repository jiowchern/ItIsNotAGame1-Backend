﻿using System;

using Regulus.Remoting;
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

		INotifier<Data.IInventoryController> InventoryControllerProvider { get; }
		INotifier<Data.IBagNotifier> BagNotifierProvider { get; }
		INotifier<Data.IEquipmentNotifier> EquipmentNotifierProvider { get; }
		INotifier<Data.INormalSkill> NormalControllerProvider { get; }

		INotifier<Data.IBattleSkill> BattleControllerProvider { get; }

		INotifier<Data.ICastSkill> BattleCastControllerProvider { get; }

		INotifier<Data.IEmotion> EmotionControllerProvider { get; }
		INotifier<Data.IMakeSkill> MakeControllerProvider { get; }

		INotifier<Data.IDevelopActor> DevelopActorProvider { get; }

        INotifier<Data.IJumpMap> JumpMapProvider { get; }




    }
}
