using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Toggl.Storage;
using Toggl.Storage.Exceptions;

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

        protected static IObservable<T> CreateObservable<T>(Func<T> getFunction)
        {
            return Observable.Create<T>(observer =>
            {
                try
                {
                    var data = getFunction();
                    observer.OnNext(data);
                    observer.OnCompleted();
                }
                catch (InvalidOperationException ex)
                {
                    observer.OnError(new DatabaseOperationException<TModel>(ex));
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }

                return Disposable.Empty;
            });
        }
    }

    public class FakeRepository<TModel> : FakeBaseStorage<TModel>, IRepository<TModel>
    {
        public IObservable<TModel> GetById(long id)
        {
            throw new InvalidOperationException();
            //throw new NotImplementedException();
            // return Observable.Return((TModel) null);
        }

        public IObservable<IEnumerable<TModel>> GetByIds(long[] ids)
        {
            //throw new NotImplementedException();
            throw new InvalidOperationException();
        }

        public IObservable<TModel> ChangeId(long currentId, long newId)
        {
            //throw new NotImplementedException();
            throw new InvalidOperationException();
        }
    }
}
