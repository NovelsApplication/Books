using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Books.Wardrobe
{
    public class EnumDisplayNameResolver
    {
        private List<(Enum enumVal, string strVal )> _cash = new List<(Enum enumVal, string strVal)>(10);

        public string GetDisplayName(Enum enumVal)
        {
            var cashedObj = _cash.FirstOrDefault(o => Equals(o.enumVal, enumVal));
            
            if (!cashedObj.Equals(default))
            {
                Debug.Log($"Display name for enum ({enumVal.ToString()} is {cashedObj.strVal} finded in Cash!)");
                return cashedObj.strVal;
            }
            
            Type type = enumVal.GetType();
            DisplayNameAttribute[] attributes = (DisplayNameAttribute[]) type.GetCustomAttributes(false);

            if (attributes.Length == 0)
            {
                Debug.LogErrorFormat("");
                return String.Empty;
            }

            string strVal = attributes[0].Name;
            _cash.Add((enumVal, strVal));
            Debug.Log($"Object with enumVal -({enumVal} and strVal - {strVal} added in Cash!)");
            
            return strVal;
        }

        public T GetEnumFromDisplayName<T>(string displayName) where T : struct, Enum
        {
            foreach (T enumVal in Enum.GetValues(typeof(T)))
            {
                string strVal = GetDisplayName(enumVal);
                if (displayName.Equals(strVal))
                {
                    return enumVal;
                }
            }

            Debug.LogErrorFormat($"There is NOT enumVal for display name - {displayName}");
            return default;
        }
    }
}