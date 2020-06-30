using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Storage;

namespace Toggl.WPF.Database
{
    public class FakeBaseStorage<TModel> : IBaseStorage<TModel>
    {
        protected readonly List<TModel> models = new List<TModel>();
        public IObservable<TModel> Create(TModel entity)
        {
            models.Add(entity);
            return Observable.Return<TModel>(entity);
        }

        public IObservable<IEnumerable<TModel>> GetAll()
        {
            return Observable.Return(models);
        }

        public IObservable<IEnumerable<TModel>> GetAll(Func<TModel, bool> predicate)
        {
            return Observable.Return(models.Where(predicate));
        }

        public IObservable<TModel> Update(long id, TModel entity)
        {
            if (models.Count < id)
            {
                models[(int) id] = entity;
            }
            return Observable.Return(entity);
        }

        public IObservable<IEnumerable<IConflictResolutionResult<TModel>>> BatchUpdate(IEnumerable<(long Id, TModel Entity)> entities, Func<TModel, TModel, ConflictResolutionMode> conflictResolution, IRivalsResolver<TModel> rivalsResolver = null)
        {
            return Observable.Return(new IConflictResolutionResult<TModel>[0]);
        }

        public IObservable<Unit> Delete(long id)
        {
            if (models.Count < id)
            {
                models.RemoveAt((int) id);
            }
            return Observable.Return(Unit.Default);
        }
    }

    public class FakeRepository<TModel> : FakeBaseStorage<TModel>, IRepository<TModel>
    {
        public IObservable<TModel> GetById(long id)
        {
            throw new NotImplementedException();
            // return Observable.Return((TModel) null);
        }

        public IObservable<IEnumerable<TModel>> GetByIds(long[] ids)
        {
            throw new NotImplementedException();
        }

        public IObservable<TModel> ChangeId(long currentId, long newId)
        {
            throw new NotImplementedException();
        }
    }
}
