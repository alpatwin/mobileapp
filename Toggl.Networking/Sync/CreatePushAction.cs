﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Toggl.Shared.Models;

namespace Toggl.Networking.Sync
{
    internal class CreatePushAction<TPayload> : PushAction<CreateMeta>
        where TPayload : IIdentifiable
    {
        [JsonConverter(typeof(StringEnumConverter), true)]
        public override ActionType Type => ActionType.Create;

        public TPayload Payload { get; set; }

        public CreatePushAction(TPayload payload)
        {
            Payload = payload;
            Meta = new CreateMeta(payload.Id);
        }
    }
}
