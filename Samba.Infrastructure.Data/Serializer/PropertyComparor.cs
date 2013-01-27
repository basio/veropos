﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Samba.Infrastructure.Data.Serializer
{
    public static class PropertyComparor
    {
        public static void ExtractEntities(object item, IDictionary<Type, Dictionary<int, IEntity>> types)
        {
            var itemName = item.GetType().Name;
            if (itemName.Contains("_")) itemName = itemName.Substring(0, itemName.IndexOf("_"));
            var properties = item.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(item, null);
                if (value is IEntity && value.GetType().GetProperty(itemName + "Id") == null)
                {
                    AddEntity(value as IEntity, types);
                }
                else if (value is IList)
                {
                    var list = value as IList;
                    Debug.Assert(list != null);
                    foreach (var listItem in list)
                    {
                        if (listItem is IEntity && listItem.GetType().GetProperty(itemName + "Id") == null)
                            AddEntity(listItem as IEntity, types);
                        else
                        {
                            ExtractEntities(listItem, types);
                        }
                    }
                }
            }
        }

        private static void AddEntity(IEntity entity, IDictionary<Type, Dictionary<int, IEntity>> types)
        {
            Debug.Assert(entity != null);

            if (!types.ContainsKey(entity.GetType()))
            {
                types.Add(entity.GetType(), new Dictionary<int, IEntity>());
            }

            if (!types[entity.GetType()].ContainsKey(entity.Id))
            {
                types[entity.GetType()].Add(entity.Id, entity);
            }
        }


        public static bool AreEquals(object actual, object expected)
        {
            try
            {
                if (!expected.GetType().IsClass) return actual.Equals(expected);
                if (expected is string) return actual.ToString() == expected.ToString();

                var properties = expected.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (property.GetSetMethod() == null)
                        continue;

                    var expectedValue = property.GetValue(expected, null);
                    var actualValue = property.GetValue(actual, null);

                    if (actualValue == null) if (expectedValue != null) return false;
                    if (expectedValue == null) if (actualValue != null) return false;
                    if (expectedValue == null) continue;

                    if (actualValue is IList)
                    {
                        if (!AssertListsAreEquals((IList)actualValue, (IList)expectedValue))
                            return false;
                    }
                    else if (actualValue.GetType().IsClass)
                    {
                        if (AreEquals(actualValue, expectedValue) == false)
                            return false;
                    }
                    else if (!Equals(expectedValue, actualValue))
                        return false;
                }
                return true;
            }
            catch (TargetException)
            {
                return false;
            }
        }

        private static bool AssertListsAreEquals(ICollection actualList, IList expectedList)
        {
            if (actualList == null) throw new ArgumentNullException("actualList");
            if (actualList.Count != expectedList.Count) return false;
            return !actualList.Cast<object>().Where((t, i) => !AreEquals(t, expectedList[i])).Any();
        }

        public static void ResetIds(object item)
        {
            var itemName = item.GetType().Name;
            if (itemName.Contains("_")) itemName = itemName.Substring(0, itemName.IndexOf("_"));

            var properties = item.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(item, null);
                if (value != null && value.GetType().GetProperty("Id") != null && value.GetType().GetProperty(itemName + "Id") != null)
                {
                    value.GetType().GetProperty("Id").SetValue(value, 0, null);
                }
                else if (value is IList)
                {
                    var list = value as IList;
                    Debug.Assert(list != null);
                    foreach (var listItem in list)
                    {
                        if (listItem.GetType().GetProperty("Id") != null && listItem.GetType().GetProperty(itemName + "Id") != null)
                            listItem.GetType().GetProperty("Id").SetValue(listItem, 0, null);
                        ResetIds(listItem);
                    }
                }
            }
        }
    }
}
