﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace System {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal partial class SR {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Antelcat.DependencyInjectionEx.Resources.Strings", typeof(SR).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性，对
        ///   使用此强类型资源类的所有资源查找执行重写。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 Unable to activate type &apos;{0}&apos;. The following constructors are ambiguous: 的本地化字符串。
        /// </summary>
        internal static string AmbiguousConstructorException {
            get {
                return ResourceManager.GetString("AmbiguousConstructorException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Unable to create an Enumerable service of type &apos;{0}&apos; because it is a ValueType. Native code to support creating Enumerable services might not be available with native AOT. 的本地化字符串。
        /// </summary>
        internal static string AotCannotCreateEnumerableValueType {
            get {
                return ResourceManager.GetString("AotCannotCreateEnumerableValueType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Unable to create a generic service for type &apos;{0}&apos; because &apos;{1}&apos; is a ValueType. Native code to support creating generic services might not be available with native AOT. 的本地化字符串。
        /// </summary>
        internal static string AotCannotCreateGenericValueType {
            get {
                return ResourceManager.GetString("AotCannotCreateGenericValueType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Arity of open generic service type &apos;{0}&apos; does not equal arity of open generic implementation type &apos;{1}&apos;. 的本地化字符串。
        /// </summary>
        internal static string ArityOfOpenGenericServiceNotEqualArityOfOpenGenericImplementation {
            get {
                return ResourceManager.GetString("ArityOfOpenGenericServiceNotEqualArityOfOpenGenericImplementation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 &apos;{0}&apos; type only implements IAsyncDisposable. Use DisposeAsync to dispose the container. 的本地化字符串。
        /// </summary>
        internal static string AsyncDisposableServiceDispose {
            get {
                return ResourceManager.GetString("AsyncDisposableServiceDispose", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Call site type {0} is not supported 的本地化字符串。
        /// </summary>
        internal static string CallSiteTypeNotSupported {
            get {
                return ResourceManager.GetString("CallSiteTypeNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Unable to resolve service for type &apos;{0}&apos; while attempting to activate &apos;{1}&apos;. 的本地化字符串。
        /// </summary>
        internal static string CannotResolveService {
            get {
                return ResourceManager.GetString("CannotResolveService", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 A circular dependency was detected for the service of type &apos;{0}&apos;. 的本地化字符串。
        /// </summary>
        internal static string CircularDependencyException {
            get {
                return ResourceManager.GetString("CircularDependencyException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Constant value of type &apos;{0}&apos; can&apos;t be converted to service type &apos;{1}&apos; 的本地化字符串。
        /// </summary>
        internal static string ConstantCantBeConvertedToServiceType {
            get {
                return ResourceManager.GetString("ConstantCantBeConvertedToServiceType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Cannot resolve {1} service &apos;{0}&apos; from root provider. 的本地化字符串。
        /// </summary>
        internal static string DirectScopedResolvedFromRootException {
            get {
                return ResourceManager.GetString("DirectScopedResolvedFromRootException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 GetCaptureDisposable call is supported only for main scope 的本地化字符串。
        /// </summary>
        internal static string GetCaptureDisposableNotSupported {
            get {
                return ResourceManager.GetString("GetCaptureDisposableNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Implementation type &apos;{0}&apos; can&apos;t be converted to service type &apos;{1}&apos; 的本地化字符串。
        /// </summary>
        internal static string ImplementationTypeCantBeConvertedToServiceType {
            get {
                return ResourceManager.GetString("ImplementationTypeCantBeConvertedToServiceType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Invalid service descriptor 的本地化字符串。
        /// </summary>
        internal static string InvalidServiceDescriptor {
            get {
                return ResourceManager.GetString("InvalidServiceDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The type of the key used for lookup doesn&apos;t match the type in the constructor parameter with the ServiceKey attribute. 的本地化字符串。
        /// </summary>
        internal static string InvalidServiceKeyType {
            get {
                return ResourceManager.GetString("InvalidServiceKeyType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 A suitable constructor for type &apos;{0}&apos; could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor. 的本地化字符串。
        /// </summary>
        internal static string NoConstructorMatch {
            get {
                return ResourceManager.GetString("NoConstructorMatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 No service for type &apos;{0}&apos; has been registered. 的本地化字符串。
        /// </summary>
        internal static string NoServiceRegistered {
            get {
                return ResourceManager.GetString("NoServiceRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Open generic service type &apos;{0}&apos; requires registering an open generic implementation type. 的本地化字符串。
        /// </summary>
        internal static string OpenGenericServiceRequiresOpenGenericImplementation {
            get {
                return ResourceManager.GetString("OpenGenericServiceRequiresOpenGenericImplementation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Cannot consume {2} service &apos;{0}&apos; from {3} &apos;{1}&apos;. 的本地化字符串。
        /// </summary>
        internal static string ScopedInSingletonException {
            get {
                return ResourceManager.GetString("ScopedInSingletonException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Cannot resolve &apos;{0}&apos; from root provider because it requires {2} service &apos;{1}&apos;. 的本地化字符串。
        /// </summary>
        internal static string ScopedResolvedFromRootException {
            get {
                return ResourceManager.GetString("ScopedResolvedFromRootException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Requested service descriptor doesn&apos;t exist. 的本地化字符串。
        /// </summary>
        internal static string ServiceDescriptorNotExist {
            get {
                return ResourceManager.GetString("ServiceDescriptorNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Generic implementation type &apos;{0}&apos; has a DynamicallyAccessedMembers attribute applied to a generic argument type, but the service type &apos;{1}&apos; doesn&apos;t have a matching DynamicallyAccessedMembers attribute on its generic argument type. 的本地化字符串。
        /// </summary>
        internal static string TrimmingAnnotationsDoNotMatch {
            get {
                return ResourceManager.GetString("TrimmingAnnotationsDoNotMatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Generic implementation type &apos;{0}&apos; has a DefaultConstructorConstraint (&apos;new()&apos; constraint), but the generic service type &apos;{1}&apos; doesn&apos;t. 的本地化字符串。
        /// </summary>
        internal static string TrimmingAnnotationsDoNotMatch_NewConstraint {
            get {
                return ResourceManager.GetString("TrimmingAnnotationsDoNotMatch_NewConstraint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Cannot instantiate implementation type &apos;{0}&apos; for service type &apos;{1}&apos;. 的本地化字符串。
        /// </summary>
        internal static string TypeCannotBeActivated {
            get {
                return ResourceManager.GetString("TypeCannotBeActivated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 No constructor for type &apos;{0}&apos; can be instantiated using services from the service container and default values. 的本地化字符串。
        /// </summary>
        internal static string UnableToActivateTypeException {
            get {
                return ResourceManager.GetString("UnableToActivateTypeException", resourceCulture);
            }
        }
    }
}
