using Mono.Cecil;

namespace DotNetFlow.Fody
{
    public class AttributeFinder
    {
        public AttributeFinder(MethodDefinition method)
        {
            var customAttributes = method.CustomAttributes;
            if (customAttributes.ContainsAttribute("DotNetFlow.Fody.ExceptionRaiseSiteAttribute"))
            {
                Raising = true;
            }

            if (customAttributes.ContainsAttribute("DotNetFlow.Fody.ExceptionChannelAttribute"))
            {
                Channel = true;
            }

            if (customAttributes.ContainsAttribute("DotNetFlow.Fody.ExceptionHandlerAttribute"))
            {
                Handler = true;
            }

            if (customAttributes.ContainsAttribute("DotNetFlow.Fody.ExceptionInterfaceAttribute"))
            {
                Interface = true;
            }
        }

        public bool Swallow;
        public bool Raising;
        public bool Channel;
        public bool Handler;
        public bool Interface;
    }
}