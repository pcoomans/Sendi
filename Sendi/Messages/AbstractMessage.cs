using System.Runtime.Serialization;

namespace Sendi.Messages
{

	public abstract class AbstractMessage
	{

		[DataMember]
		public readonly static int MessageTypeId;

		[DataMember]
		public MessageAdministration MessageConfig { private set; get; }

		/// <summary>
		/// For a class using generics, the static constructor will be called once for each unique 
		/// combination of generic arguments. This way we can automatically give every type a unique id.
		/// (The idea is that comparing an int goes faster then comparing a type)
		/// </summary>
		static AbstractMessage()
		{
			MessageTypeId = Sendi.Util.Utilities.CreateNewMsgTypeNr();
		}

		public AbstractMessage()
		{
			MessageConfig = new MessageAdministration(MessageTypeId);
		}

	}

}
