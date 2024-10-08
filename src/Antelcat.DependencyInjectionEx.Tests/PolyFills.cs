#if NETFRAMEWORK
namespace System.Runtime.CompilerServices
{
    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple=true, Inherited=false)]
    public sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string feature) { }
    }
    
    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple=true, Inherited=false)]
    public sealed class RequiredMemberAttribute : Attribute
    {
        public RequiredMemberAttribute() { }
    }
}
#endif