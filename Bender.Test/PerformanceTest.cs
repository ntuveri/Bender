using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using System.Reflection.Emit;

namespace Bender.Test
{
    [TestFixture]
    public class PerformanceTest
    {
        private static readonly Dictionary<Type, OpCode> ValueTypesOpCodes = new Dictionary<Type, OpCode>
        {
            {typeof (sbyte), OpCodes.Ldind_I1},
            {typeof (byte), OpCodes.Ldind_U1},
            {typeof (char), OpCodes.Ldind_U2},
            {typeof (short), OpCodes.Ldind_I2},
            {typeof (ushort), OpCodes.Ldind_U2},
            {typeof (int), OpCodes.Ldind_I4},
            {typeof (uint), OpCodes.Ldind_U4},
            {typeof (long), OpCodes.Ldind_I8},
            {typeof (ulong), OpCodes.Ldind_I8},
            {typeof (bool), OpCodes.Ldind_I1},
            {typeof (double), OpCodes.Ldind_R8},
            {typeof (float), OpCodes.Ldind_R4}
        };

        private static void EmitCall(ILGenerator il, MethodInfo method)
        {
            if (method.IsVirtual)
            {
                il.Emit(OpCodes.Callvirt, method);
            }
            else
            {
                il.Emit(OpCodes.Call, method);
            }
        }

        private static void EmitConvert(ILGenerator il, Type memberType)
        {
            if (memberType.IsValueType)
            {
                il.Emit(OpCodes.Unbox, memberType);

                if (ValueTypesOpCodes.ContainsKey(memberType))
                {
                    var load = ValueTypesOpCodes[memberType];
                    il.Emit(load);
                }
                else
                {
                    il.Emit(OpCodes.Ldobj, memberType);
                }
            }
            else
            {
                il.Emit(OpCodes.Castclass, memberType);
            }
        }
        
        private Func<object, object> GetGetter(Type targetType, string propertyName)
        {
            var method= new DynamicMethod("getterInvoke", MethodAttributes.Static |   
                MethodAttributes.Public, CallingConventions.Standard,
                typeof(object), new Type[] { typeof(object) }, targetType, true);
            PropertyInfo propertyInfo = targetType.GetProperty(propertyName);
            MethodInfo methodInfo =  propertyInfo.GetGetMethod(true);
            
            
            ILGenerator generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, targetType);
            EmitCall(generator, methodInfo);
            if (propertyInfo.PropertyType.IsValueType)
            {
                generator.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }
            generator.Emit(OpCodes.Ret);
            var delegateMethod = method.CreateDelegate(typeof(Func<object, object>));

            return (Func<object, object>) delegateMethod;
        }


        private Action<object, object> GetSetter(Type targetType, string propertyName)
        {
            var method= new DynamicMethod("setterInvoke", MethodAttributes.Static |   
                MethodAttributes.Public, CallingConventions.Standard,
                null, new Type[] { typeof(object), typeof(object) }, targetType, true);
            PropertyInfo propertyInfo = targetType.GetProperty(propertyName);
            MethodInfo methodInfo =  propertyInfo.GetSetMethod(true);
         
            ILGenerator generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, targetType);
            generator.Emit(OpCodes.Ldarg_1);
            EmitConvert(generator, propertyInfo.PropertyType);
            EmitCall(generator, methodInfo);
            generator.Emit(OpCodes.Ret);
            var delegateMethod = method.CreateDelegate(typeof(Action<object, object>));

            return (Action<object, object>) delegateMethod;
        }

        [Test]
        public void ReflectionEmitTest()
        {
            Type targetType = typeof(MapperTest.G);
                
            var delegateMethodGetterText = GetGetter(targetType, "Text");
            var delegateMethodGetterValue = GetGetter(targetType, "Value");
            
            var delegateMethodSetterText = GetSetter(targetType, "Text");
            var delegateMethodSetterValue = GetSetter(targetType, "Value");
                    
                
            for (int i = 0; i < 100000; i++)
            {
                var t = delegateMethodGetterText.DynamicInvoke(new MapperTest.G() { Text = "Nicola" });
                var v = delegateMethodGetterValue.DynamicInvoke(new MapperTest.G() { Value = 123L });

                Assert.AreEqual("Nicola", t);
                Assert.AreEqual(123L, v);

                var g = new MapperTest.G();

                delegateMethodSetterText.DynamicInvoke(g, "Tuveri");
                delegateMethodSetterValue.DynamicInvoke(g, 456L);

                Assert.AreEqual("Tuveri", g.Text);
                Assert.AreEqual(456L, g.Value);
            }            
        }

        [Test]
        public void ReflectionPerformanceTest()
        {
            Type targetType = typeof(MapperTest.G);
            PropertyInfo propertyInfoText = targetType.GetProperty("Text");
            MethodInfo methodInfoGetterText =  propertyInfoText.GetGetMethod(true);
                
            PropertyInfo propertyInfoValue = targetType.GetProperty("Value");
            MethodInfo methodInfoGetterValue =  propertyInfoValue.GetGetMethod(true);

            MethodInfo methodInfoSetterText =  propertyInfoText.GetSetMethod(true);
            MethodInfo methodInfoSetterValue =  propertyInfoValue.GetSetMethod(true);
            
            for (int i = 0; i < 10000; i++)
            {
                
                var t = methodInfoGetterText.Invoke(new MapperTest.G() { Text = "Nicola" }, null);
                var v = methodInfoGetterValue.Invoke(new MapperTest.G() { Value = 123L }, null);

                Assert.AreEqual("Nicola", t);
                Assert.AreEqual(123L, v);

                var g = new MapperTest.G();

                methodInfoSetterText.Invoke(g, new object[] { "Tuveri" });
                methodInfoSetterValue.Invoke(g, new object[] { 456L });

                Assert.AreEqual("Tuveri", g.Text);
                Assert.AreEqual(456L, g.Value);
            }            
        }
    }
}
