﻿// Copyright (C) 2007 James Newton-King
// Copyright (C) 2007 2014 Extesla, LLC.
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Security;
#if !(NETFX_CORE || PORTABLE || PORTABLE40)
using System.Security.Permissions;
#endif
using OpenGamingLibrary.Json.Utilities;
using System.Linq;
using System.Runtime.Serialization;

namespace OpenGamingLibrary.Json.Serialization
{
    internal static class JsonTypeReflector
    {
        private static bool? _dynamicCodeGeneration;
        private static bool? _fullyTrusted;

        public const string IdPropertyName = "$id";
        public const string RefPropertyName = "$ref";
        public const string TypePropertyName = "$type";
        public const string ValuePropertyName = "$value";
        public const string ArrayValuesPropertyName = "$values";

        public const string ShouldSerializePrefix = "ShouldSerialize";
        public const string SpecifiedPostfix = "Specified";

        private static readonly ThreadSafeStore<Type, Func<object[], JsonConverter>> JsonConverterCreatorCache = 
            new ThreadSafeStore<Type, Func<object[], JsonConverter>>(GetJsonConverterCreator);

        private static readonly ThreadSafeStore<Type, Type> AssociatedMetadataTypesCache = new ThreadSafeStore<Type, Type>(GetAssociateMetadataTypeFromAttribute);
        private static ReflectionObject _metadataTypeAttributeReflectionObject;

        public static T GetCachedAttribute<T>(object attributeProvider) where T : Attribute
        {
            return CachedAttributeGetter<T>.GetAttribute(attributeProvider);
        }

		public static DataContractAttribute GetDataContractAttribute(Type type)
		{
			// DataContractAttribute does not have inheritance
			Type currentType = type;

			while (currentType != null)
			{
				DataContractAttribute result = CachedAttributeGetter<DataContractAttribute>.GetAttribute(currentType);
				if (result != null)
					return result;

				currentType = currentType.BaseType();
			}

			return null;
		}

		public static DataMemberAttribute GetDataMemberAttribute(MemberInfo memberInfo)
		{
			// DataMemberAttribute does not have inheritance

			// can't override a field
			if (memberInfo.MemberType() == MemberTypes.Field)
				return CachedAttributeGetter<DataMemberAttribute>.GetAttribute(memberInfo);

			// search property and then search base properties if nothing is returned and the property is virtual
			var propertyInfo = (PropertyInfo)memberInfo;
			DataMemberAttribute result = CachedAttributeGetter<DataMemberAttribute>.GetAttribute(propertyInfo);
			if (result == null)
			{
				if (propertyInfo.IsVirtual())
				{
					Type currentType = propertyInfo.DeclaringType;

					while (result == null && currentType != null)
					{
						PropertyInfo baseProperty = (PropertyInfo)ReflectionUtils.GetMemberInfoFromType(currentType, propertyInfo);
						if (baseProperty != null && baseProperty.IsVirtual())
							result = CachedAttributeGetter<DataMemberAttribute>.GetAttribute(baseProperty);

						currentType = currentType.BaseType();
					}
				}
			}

			return result;
		}

        public static MemberSerialization GetObjectMemberSerialization(Type objectType, bool ignoreSerializableAttribute)
        {
            JsonObjectAttribute objectAttribute = GetCachedAttribute<JsonObjectAttribute>(objectType);
            if (objectAttribute != null)
                return objectAttribute.MemberSerialization;

			DataContractAttribute dataContractAttribute = GetDataContractAttribute(objectType);
			if (dataContractAttribute != null)
				return MemberSerialization.OptIn;

            if (!ignoreSerializableAttribute)
            {
                SerializableAttribute serializableAttribute = GetCachedAttribute<SerializableAttribute>(objectType);
                if (serializableAttribute != null)
                    return MemberSerialization.Fields;
            }

            // the default
            return MemberSerialization.OptOut;
        }

        public static JsonConverter GetJsonConverter(object attributeProvider)
        {
            JsonConverterAttribute converterAttribute = GetCachedAttribute<JsonConverterAttribute>(attributeProvider);

            if (converterAttribute != null)
            {
                Func<object[], JsonConverter> creator = JsonConverterCreatorCache.Get(converterAttribute.ConverterType);
                if (creator != null)
                    return creator(converterAttribute.ConverterParameters);
            }

            return null;
        }

        /// <summary>
        /// Lookup and create an instance of the JsonConverter type described by the argument.
        /// </summary>
        /// <param name="converterType">The JsonConverter type to create.</param>
        /// <param name="converterArgs">Optional arguments to pass to an initializing constructor of the JsonConverter.
        /// If null, the default constructor is used.</param>
        public static JsonConverter CreateJsonConverterInstance(Type converterType, object[] converterArgs)
        {
            Func<object[], JsonConverter> converterCreator = JsonConverterCreatorCache.Get(converterType);
            return converterCreator(converterArgs);
        }

        /// <summary>
        /// Create a factory function that can be used to create instances of a JsonConverter described by the 
        /// argument type.  The returned function can then be used to either invoke the converter's default ctor, or any 
        /// parameterized constructors by way of an object array.
        /// </summary>
        private static Func<object[], JsonConverter> GetJsonConverterCreator(Type converterType)
        {
            Func<object> defaultConstructor = (ReflectionUtils.HasDefaultConstructor(converterType, false))
                ? ReflectionDelegateFactory.CreateDefaultConstructor<object>(converterType)
                : null;

            return (parameters) =>
            {
                try
                {
                    if (parameters != null)
                    {
                        ObjectConstructor<object> parameterizedConstructor = null;
                        Type[] paramTypes = parameters.Select(param => param.GetType()).ToArray();
                        ConstructorInfo parameterizedConstructorInfo = converterType.GetConstructor(paramTypes);

                        if (null != parameterizedConstructorInfo)
                        {
                            parameterizedConstructor = ReflectionDelegateFactory.CreateParametrizedConstructor(parameterizedConstructorInfo);
                            return (JsonConverter)parameterizedConstructor(parameters);
                        }
                        else 
                        {
                            throw new JsonException("No matching parameterized constructor found for '{0}'.".FormatWith(CultureInfo.InvariantCulture, converterType));
                        }                        
                    }

                    if (defaultConstructor == null)
                        throw new JsonException("No parameterless constructor defined for '{0}'.".FormatWith(CultureInfo.InvariantCulture, converterType));

                    return (JsonConverter)defaultConstructor();
                }
                catch (Exception ex)
                {
                    throw new JsonException("Error creating '{0}'.".FormatWith(CultureInfo.InvariantCulture, converterType), ex);
                }
            };
        }

#if !(NETFX_CORE || PORTABLE40 || PORTABLE)
        public static TypeConverter GetTypeConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type);
        }
#endif

#if !(NET20 || NETFX_CORE)
        private static Type GetAssociatedMetadataType(Type type)
        {
            return AssociatedMetadataTypesCache.Get(type);
        }

        private static Type GetAssociateMetadataTypeFromAttribute(Type type)
        {
            object[] customAttributes;
#if !PORTABLE
            customAttributes = type.GetCustomAttributes(false);
#else
            customAttributes = type.GetTypeInfo().GetCustomAttributes(false).Cast<object>().ToArray();
#endif

            foreach (var attribute in customAttributes)
            {
                Type attributeType = attribute.GetType();

                // only test on attribute type name
                // attribute assembly could change because of type forwarding, etc
                if (string.Equals(attributeType.FullName, "System.ComponentModel.DataAnnotations.MetadataTypeAttribute", StringComparison.Ordinal))
                {
                    const string metadataClassTypeName = "MetadataClassType";

                    if (_metadataTypeAttributeReflectionObject == null)
                        _metadataTypeAttributeReflectionObject = ReflectionObject.Create(attributeType, metadataClassTypeName);

                    return (Type)_metadataTypeAttributeReflectionObject.GetValue(attribute, metadataClassTypeName);
                }
            }

            return null;
        }
#endif

        private static T GetAttribute<T>(Type type) where T : Attribute
        {
            T attribute;

#if !(NET20 || NETFX_CORE)
            Type metadataType = GetAssociatedMetadataType(type);
            if (metadataType != null)
            {
                attribute = ReflectionUtils.GetAttribute<T>(metadataType, true);
                if (attribute != null)
                    return attribute;
            }
#endif

            attribute = ReflectionUtils.GetAttribute<T>(type, true);
            if (attribute != null)
                return attribute;

            foreach (Type typeInterface in type.GetInterfaces())
            {
                attribute = ReflectionUtils.GetAttribute<T>(typeInterface, true);
                if (attribute != null)
                    return attribute;
            }

            return null;
        }

        private static T GetAttribute<T>(MemberInfo memberInfo) where T : Attribute
        {
            T attribute;

#if !(NET20 || NETFX_CORE)
            Type metadataType = GetAssociatedMetadataType(memberInfo.DeclaringType);
            if (metadataType != null)
            {
                MemberInfo metadataTypeMemberInfo = ReflectionUtils.GetMemberInfoFromType(metadataType, memberInfo);

                if (metadataTypeMemberInfo != null)
                {
                    attribute = ReflectionUtils.GetAttribute<T>(metadataTypeMemberInfo, true);
                    if (attribute != null)
                        return attribute;
                }
            }
#endif

            attribute = ReflectionUtils.GetAttribute<T>(memberInfo, true);
            if (attribute != null)
                return attribute;

            if (memberInfo.DeclaringType != null)
            {
                foreach (Type typeInterface in memberInfo.DeclaringType.GetInterfaces())
                {
                    MemberInfo interfaceTypeMemberInfo = ReflectionUtils.GetMemberInfoFromType(typeInterface, memberInfo);

                    if (interfaceTypeMemberInfo != null)
                    {
                        attribute = ReflectionUtils.GetAttribute<T>(interfaceTypeMemberInfo, true);
                        if (attribute != null)
                            return attribute;
                    }
                }
            }

            return null;
        }

        public static T GetAttribute<T>(object provider) where T : Attribute
        {
            Type type = provider as Type;
            if (type != null)
                return GetAttribute<T>(type);

            MemberInfo memberInfo = provider as MemberInfo;
            if (memberInfo != null)
                return GetAttribute<T>(memberInfo);

            return ReflectionUtils.GetAttribute<T>(provider, true);
        }

#if DEBUG
        internal static void SetFullyTrusted(bool fullyTrusted)
        {
            _fullyTrusted = fullyTrusted;
        }

        internal static void SetDynamicCodeGeneration(bool dynamicCodeGeneration)
        {
            _dynamicCodeGeneration = dynamicCodeGeneration;
        }
#endif

        public static bool DynamicCodeGeneration
        {
			get { return _dynamicCodeGeneration.Value; }
        }

        public static bool FullyTrusted
        {
            get { return _fullyTrusted.Value; }
        }

        public static ReflectionDelegateFactory ReflectionDelegateFactory
        {
            get
            {
				return DynamicCodeGeneration ? DynamicReflectionDelegateFactory.Instance : LateBoundReflectionDelegateFactory.Instance;
            }
        }
    }
}