﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


namespace Phoenix {
	public interface IMessageSerializer {
		string Serialize(Message message);
		Message Deserialize(string message);
	}

	public class Message : IEquatable<Message> {
		#region nested types

		/** 
		 * A reply appears within a message, returning a status and response.
		 */
		public sealed class Reply {

			public enum Status {
				ok,
				error,
				timeout,
			}

			// PhoenixJS maps incoming phx_reply to chan_reply_{ref} when broadcasting the event
			public static readonly string replyEventPrefix = "chan_reply_";

			public readonly string status;
			public readonly Dictionary<string, object> response;

			[System.Runtime.Serialization.IgnoreDataMember]
			public Status replyStatus {
				get {
					if (status == null) {
						// shouldn't happen
						return Status.error;
					}

					switch (status) {
						case "ok":
							return Status.ok;
						case "error":
							return Status.error;
						case "timeout":
							return Status.timeout;
						default:
							throw new ArgumentException("Unknown status: " + status);
					}
				}

			}

			public Reply(string status, Dictionary<string, object> response) {
				this.status = status;
				this.response = response;
			}
		}

		public enum InBoundEvent {
			phx_reply,
			phx_close,
			phx_error,
		}

		public enum OutBoundEvent {
			phx_join,
			phx_leave,
		}

		#endregion

		public readonly string topic;
		// unfortunate mutation of the original message
		public string @event;
		public readonly string @ref;
		public readonly Dictionary<string, object> payload;
		public readonly string joinRef;
		// private members are ignore by default
		private Reply _cachedReply;

		public Message(
				string topic = null,
				string @event = null,
				Dictionary<string, object> payload = null,
				string @ref = null,
				string joinRef = null
		) {
			this.topic = topic;
			this.@event = @event;
			this.payload = payload;
			this.@ref = @ref;
			this.joinRef = joinRef;
		}

		public Reply ParseReply() {
			if (_cachedReply != null) {
				return _cachedReply;
			}
			if (!@event.StartsWith(Reply.replyEventPrefix)
					&& @event != InBoundEvent.phx_reply.ToString()) {
				return null;
			}

			// TODO: use serializer to avoid coupling with JObject
			_cachedReply = JObject.FromObject(payload).ToObject<Reply>();
			return _cachedReply;
		}

		public override string ToString() {
			return string.Format("Message: {0} - {1}: {2}", @ref, topic, @event);
		}

		#region IEquatable methods

		public override int GetHashCode() {
			return topic.GetHashCode() + @event.GetHashCode() + @ref.GetHashCode();
		}

		public override bool Equals(object obj) {
			return (obj is Message) && Equals((Message)obj);
		}

		public bool Equals(Message that) {
			return this.topic == that.topic
					&& this.@event == that.@event
					&& this.@ref == that.@ref
					/* dictionary equality is hard */
					;
		}

		#endregion
	}


	public static class MessageInBoundEventExtensions {

		public static Message.InBoundEvent? Parse(string rawChannelEvent) {

			foreach (Message.InBoundEvent inboundEvent in Enum.GetValues(typeof(Message.InBoundEvent))) {
				if (inboundEvent.ToString() == rawChannelEvent) {
					return inboundEvent;
				}
			}

			return null;
		}
	}

	public static class MessageOutBoundEventExtensions {

		public static Message.OutBoundEvent Parse(string rawChannelEvent) {

			foreach (Message.OutBoundEvent outboundEvent in Enum.GetValues(typeof(Message.OutBoundEvent))) {
				if (outboundEvent.ToString() == rawChannelEvent) {
					return outboundEvent;
				}
			}

			throw new ArgumentOutOfRangeException(rawChannelEvent);
		}
	}
}