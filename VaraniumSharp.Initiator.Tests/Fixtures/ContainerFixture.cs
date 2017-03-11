using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DryIoc;
using ImTools;

// ReSharper disable InconsistentNaming - Implementation of an Interface
// ReSharper disable UnassignedGetOnlyAutoProperty - Test Fixture

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    public class ContainerFixture : IContainer
    {
        #region Properties

        public ContainerWeakRef ContainerWeakRef { get; }
        public Request EmptyRequest { get; }
        public object[] ResolutionStateCache { get; }
        public Rules Rules { get; }
        public IScopeContext ScopeContext { get; }

        #endregion

        #region Public Methods

        public void CacheFactoryExpression(int factoryID, Expression factoryExpression)
        {
            throw new NotImplementedException();
        }

        public IContainer CreateFacade()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KV<object, Factory>> GetAllServiceFactories(Type serviceType, bool bothClosedAndOpenGenerics = false)
        {
            throw new NotImplementedException();
        }

        public Expression GetCachedFactoryExpressionOrDefault(int factoryID)
        {
            throw new NotImplementedException();
        }

        public Expression GetDecoratorExpressionOrDefault(Request request)
        {
            throw new NotImplementedException();
        }

        public Factory[] GetDecoratorFactoriesOrDefault(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public int GetOrAddStateItem(object item)
        {
            throw new NotImplementedException();
        }

        public Expression GetOrAddStateItemExpression(object item, Type itemType = null, bool throwIfStateRequired = false)
        {
            throw new NotImplementedException();
        }

        public Factory GetServiceFactoryOrDefault(Request request)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ServiceRegistrationInfo> GetServiceRegistrations()
        {
            throw new NotImplementedException();
        }

        public Type GetWrappedType(Type serviceType, Type requiredServiceType)
        {
            throw new NotImplementedException();
        }

        public Factory GetWrapperFactoryOrDefault(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object InjectPropertiesAndFields(object instance, PropertiesAndFieldsSelector propertiesAndFields)
        {
            throw new NotImplementedException();
        }

        public bool IsRegistered(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition)
        {
            throw new NotImplementedException();
        }

        public IContainer OpenScope(object name = null, Func<Rules, Rules> configure = null)
        {
            throw new NotImplementedException();
        }

        public void Register(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered ifAlreadyRegistered,
            bool isStaticallyChecked)
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type serviceType, bool ifUnresolvedReturnDefault)
        {
            return _resolveItem;
        }

        public object Resolve(Type serviceType, object serviceKey, bool ifUnresolvedReturnDefault, Type requiredServiceType,
            RequestInfo preResolveParent, IScope scope)
        {
            throw new NotImplementedException();
        }

        public Factory ResolveFactory(Request request)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> ResolveMany(Type serviceType, object serviceKey, Type requiredServiceType, object compositeParentKey,
            Type compositeParentRequiredType, RequestInfo preResolveParent, IScope scope)
        {
            throw new NotImplementedException();
        }

        public void SetupItemToResolve(object item)
        {
            _resolveItem = item;
        }

        public void Unregister(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition)
        {
            throw new NotImplementedException();
        }

        public IContainer With(Func<Rules, Rules> configure = null, IScopeContext scopeContext = null)
        {
            throw new NotImplementedException();
        }

        public IContainer WithNoMoreRegistrationAllowed(bool ignoreInsteadOfThrow = false)
        {
            throw new NotImplementedException();
        }

        public IContainer WithoutCache()
        {
            throw new NotImplementedException();
        }

        public IContainer WithoutSingletonsAndCache()
        {
            throw new NotImplementedException();
        }

        public IContainer WithRegistrationsCopy(bool preserveCache = false)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Variables

        private object _resolveItem;

        #endregion
    }
}