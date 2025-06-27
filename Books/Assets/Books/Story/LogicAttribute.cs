using System;

namespace Books.Story
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class LogicAttribute : Attribute
    {
        public LogicIdx[] Idx { get; }
        public LogicAttribute(params LogicIdx[] idx) => Idx = idx;
    }
}