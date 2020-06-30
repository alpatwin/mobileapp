using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Storage;

namespace Toggl.WPF.Database
{
    public class FakeSingleObjectStorage<TModel> : FakeBaseStorage<TModel>, ISingleObjectStorage<TModel>
        where TModel : IDatabaseSyncable
    {
        public IObservable<TModel> Single()
        {
            return Observable.Return(models.Single());
        }

        public IObservable<Unit> Delete()
        {
            models.Clear();
            return Observable.Return(Unit.Default);
        }

        public IObservable<TModel> Update(TModel entity)
        {
            models.Clear();
            models.Add(entity);
            return Observable.Return(entity);
        }
    }
}
