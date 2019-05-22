using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Toggl.Core.Serialization;
using Toggl.Shared;

namespace Toggl.Core.Interactors.Timezones
{
    public sealed class GetSupportedTimezonesInteractor : IInteractor<IObservable<List<string>>>
    {
        private readonly IJsonSerializer jsonSerializer;

        public GetSupportedTimezonesInteractor(IJsonSerializer jsonSerializer)
        {
            this.jsonSerializer = jsonSerializer;
        }

        public IObservable<List<string>> Execute()
        {
            string json = Resources.TimezonesJson;

            var timezones = jsonSerializer
                .Deserialize<List<string>>(json);

            return Observable.Return(timezones);
        }
    }
}
