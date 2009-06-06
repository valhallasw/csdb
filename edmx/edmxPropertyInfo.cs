using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace csdb.edmx
{
    class edmxPropertyInfo : PropertyInfo
    {
        public String name;

        public edmxPropertyInfo(String name) {
            this.name = name;
        }

        public override PropertyAttributes Attributes {
            get { throw new NotImplementedException(); }
        }

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanWrite {
            get { throw new NotImplementedException(); }
        }

        public override MethodInfo[] GetAccessors(bool nonPublic) {
            throw new NotImplementedException();
        }

        public override MethodInfo GetGetMethod(bool nonPublic) {
            return new edmxMethodInfo();
        }

        public override ParameterInfo[] GetIndexParameters() {
            throw new NotImplementedException();
        }

        public override MethodInfo GetSetMethod(bool nonPublic) {
            throw new NotImplementedException();
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        public override Type PropertyType {
            get { return typeof(String); }
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
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
            get { return name; }
        }

        public override Type ReflectedType {
            get { throw new NotImplementedException(); }
        }
    }
}
