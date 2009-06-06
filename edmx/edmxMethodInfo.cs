using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace csdb.edmx
{
    class edmxMethodInfo : MethodInfo
    {
        public override MethodInfo GetBaseDefinition() {
            throw new NotImplementedException();
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes {
            get { throw new NotImplementedException(); }
        }

        public override MethodAttributes Attributes {
            get { return MethodAttributes.Virtual; }
        }

        public override MethodImplAttributes GetMethodImplementationFlags() {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetParameters() {
            throw new NotImplementedException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        public override RuntimeMethodHandle MethodHandle {
            get { throw new NotImplementedException(); }
        }

        public override Type DeclaringType {
            get { throw new NotImplementedException(); }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit) {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit) {
            throw new NotImplementedException();
        }

        public override string Name {
            get { throw new NotImplementedException(); }
        }

        public override Type ReflectedType {
            get { throw new NotImplementedException(); }
        }
    }
}
