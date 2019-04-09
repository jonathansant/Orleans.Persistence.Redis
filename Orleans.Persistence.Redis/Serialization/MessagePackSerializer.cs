using MessagePack;
using MessagePack.Resolvers;
using System;
using static MessagePack.MessagePackSerializer;

namespace Orleans.Persistence.Redis.Serialization
{
	public class MessagePackSerializer : ISerializer
	{
		public byte[] Serialize(IGrainState raw, Type type)
		{
			var grainState = (IGrainState)raw;
			var state = NonGeneric.Serialize(
				grainState.State.GetType(),
				grainState.State,
				ContractlessStandardResolver.Instance
			);

			var dao = new StateData
			{
				ETag = grainState.ETag,
				State = state
			};

			return Serialize<StateData>(dao);
		}

		public IGrainState Deserialize(byte[] serializedData, Type type)
		{
			var dao = Deserialize<StateData>(serializedData);
		}
	}

	[MessagePackObject]
	public class StateData
	{
		[Key(0)]
		public byte[] State { get; set; }

		[Key(1)]
		public string ETag { get; set; }
	}
}