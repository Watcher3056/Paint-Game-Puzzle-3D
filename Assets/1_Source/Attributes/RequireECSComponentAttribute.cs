using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamAlpha.Source
{
    [AttributeUsage(AttributeTargets.Class)]
    public partial class RequireECSComponentAttribute : Attribute
    {
        public Type[] filter = new Type[0];


        public RequireECSComponentAttribute(params Type[] filter)
        {
            this.filter = filter;
        }
    }
}
