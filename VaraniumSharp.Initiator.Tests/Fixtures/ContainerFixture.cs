using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DryIoc;
using FastExpressionCompiler;
using ImTools;

// ReSharper disable InconsistentNaming - Implementation of an Interface
// ReSharper disable UnassignedGetOnlyAutoProperty - Test Fixture

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    public class ContainerFixture : IContainer
    {
        #region Properties

        public IScope CurrentScope { get; }

        public Request EmptyRequest { get; }

        public bool IsDisposed { get; }
        public IScope OwnCurrentScope { get; }
        public IResolverContext Parent { get; }
        public object[] ResolutionStateCache { get; }
        public IResolverContext Root { get; }

        public Rules Rules { get; }

        public IScopeContext ScopeContext { get; }
        public IScope SingletonScope { get; }

        #endregion

        #region Public Methods

        public void CacheFactoryExpression(int factoryID, Expression factoryExpression)
        {
            throw new NotImplementedException();
        }

        public bool ClearCache(Type serviceType, FactoryType? factoryType, object serviceKey)
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

        public IEnumerable<KV<object, Factory>> GetAllServiceFactories(Type serviceType,
            bool bothClosedAndOpenGenerics = false)
        {
            throw new NotImplementedException();
        }

        public Expression GetCachedFactoryExpressionOrDefault(int factoryID)
        {
            throw new NotImplementedException();
        }

        public ExpressionInfo GetConstantExpression(object item, Type itemType = null,
            bool throwIfStateRequired = false)
        {
            throw new NotImplementedException();
        }

        public ExpressionInfo GetDecoratorExpressionOrDefault(Request request)
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

        public Expression GetOrAddStateItemExpression(object item, Type itemType = null,
            bool throwIfStateRequired = false)
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

        public void InjectPropertiesAndFields(object instance, string[] propertyAndFieldNames)
        {
            throw new NotImplementedException();
        }

        public object InjectPropertiesAndFields(object instance, PropertiesAndFieldsSelector propertiesAndFields)
        {
            throw new NotImplementedException();
        }

        public bool IsRegistered(Type serviceType, object serviceKey, FactoryType factoryType,
            Func<Factory, bool> condition)
        {
            throw new NotImplementedException();
        }

        public IContainer OpenScope(object name = null, Func<Rules, Rules> configure = null)
        {
            throw new NotImplementedException();
        }

        public void Register(Factory factory, Type serviceType, object serviceKey,
            IfAlreadyRegistered? ifAlreadyRegistered,
            bool isStaticallyChecked)
        {
            throw new NotImplementedException();
        }

        public void Register(Factory factory, Type serviceType, object serviceKey,
            IfAlreadyRegistered ifAlreadyRegistered,
            bool isStaticallyChecked)
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type serviceType, IfUnresolved ifUnresolved)
        {
            return _resolveItem;
        }

        public object Resolve(Type serviceType, object serviceKey, IfUnresolved ifUnresolved, Type requiredServiceType,
            Request preResolveParent, object[] args)
        {
            throw new NotImplementedException();
        }

        public Factory ResolveFactory(Request request)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> ResolveMany(Type serviceType, object serviceKey, Type requiredServiceType,
            Request preResolveParent,
            object[] args)
        {
            throw new NotImplementedException();
        }

        public void SetupItemToResolve(object item)
        {
            _resolveItem = item;
        }

        public void Unregister(Type serviceType, object serviceKey, FactoryType factoryType,
            Func<Factory, bool> condition)
        {
            throw new NotImplementedException();
        }

        public void UseInstance(Type serviceType, object instance, IfAlreadyRegistered IfAlreadyRegistered,
            bool preventDisposal,
            bool weaklyReferenced, object serviceKey)
        {
            throw new NotImplementedException();
        }

        public IContainer With(Func<Rules, Rules> configure = null, IScopeContext scopeContext = null)
        {
            throw new NotImplementedException();
        }

        public IContainer With(Rules rules, IScopeContext scopeContext, RegistrySharing registrySharing,
            IScope singletonScope)
        {
            throw new NotImplementedException();
        }

        public IResolverContext WithCurrentScope(IScope scope)
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