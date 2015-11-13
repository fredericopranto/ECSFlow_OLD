using Mono.Cecil;

namespace eFlowNET.Fody
{
    public class AttributeFinder
    {
        public AttributeFinder(MethodDefinition method)
        {
            var customAttributes = method.CustomAttributes;
            if (customAttributes.ContainsAttribute("eFlowNET.Fody.ExceptionRaiseSiteAttribute"))
            {
                Raising = true;
            }

            if (customAttributes.ContainsAttribute("eFlowNET.Fody.ExceptionChannelAttribute"))
            {
                Channel = true;
            }

            if (customAttributes.ContainsAttribute("eFlowNET.Fody.ExceptionHandlerAttribute"))
            {
                Handler = true;
            }

            if (customAttributes.ContainsAttribute("eFlowNET.Fody.ExceptionInterfaceAttribute"))
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