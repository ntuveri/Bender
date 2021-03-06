﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Bender
{
    public interface IKeyFilter
    {
        string Filter(string key);
    }

    public class IdentityFilter : IKeyFilter
    {
        public string Filter(string key) { return key; }
    }

    public class DotFilter : IKeyFilter 
    {
        public string Filter(string key) 
        { 
            if(key == null) return null;

            int index = key.IndexOf(".", StringComparison.Ordinal);
            if(index >= 0) 
            {
                return key.Remove(index, 1);
            }
            return key;
        }
    }

    public class PrefixPostfixFilter : IKeyFilter
    {
        private string[] Prefixes { get; set; }
        private string[] Postfixes { get; set; }

        private static readonly Regex BracketsRegex = new Regex(@"^\w+(\[\d+\])$", RegexOptions.Compiled | RegexOptions.RightToLeft); 
        
        public PrefixPostfixFilter(string[] prefixes, string[] postfixes)
        {
            Prefixes = prefixes ?? new string[] {};
            Postfixes = postfixes ?? new string[] {};
        }

        public string Filter(string key) 
        {
            if(key == null) return null;

            string[] parts = key.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                foreach (string prefix in Prefixes)
                {
                    if(parts[i].StartsWith(prefix, StringComparison.Ordinal))
                    {
                        parts[i] = parts[i].Remove(0, prefix.Length);
                        break;
                    }
                }
                
                foreach (string postfix in Postfixes)
                {
                    if(parts[i].EndsWith(postfix, StringComparison.Ordinal))
                    {   
                        parts[i] = parts[i].Remove(parts[i].Length - postfix.Length, postfix.Length);
                        break;
                    }

                    Match match = BracketsRegex.Match(parts[i]);
                    string brackets = "";
                    if(match.Success)
                    {
                        brackets = match.Groups[1].Value;
                    }
                    if(parts[i].EndsWith(postfix + brackets, StringComparison.Ordinal))
                    {   
                        parts[i] = parts[i].Remove(parts[i].Length - postfix.Length - brackets.Length, postfix.Length);
                        break;
                    }
                }
            }
                
            key = string.Join(".", parts);
            return key;
        }
    }

    public class CompositeFilter : IKeyFilter
    {
        private IKeyFilter[] Filters { get; set; }

        public CompositeFilter(IKeyFilter[] filters)
        {
            Filters = filters;
        }

        public string Filter(string key)
        {
            foreach (var filter in Filters)
            {
                key = filter.Filter(key);
            }
            return key;
        }
    }
}
