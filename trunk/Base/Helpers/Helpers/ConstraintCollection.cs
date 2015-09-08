using System.Collections.Generic;
using Natek.Helpers.Execution;

namespace Natek.Helpers.Limit
{
    public class ConstraintCollection<T> 
    {
        protected List<Constraint<T>> Constraints;


        public ConstraintCollection()
        {
            Constraints = new List<Constraint<T>>();
        }
   
        public Constraint<T> AddConstraint(Constraint<T> constraint)
        {
            Constraints.Add(constraint);
            return constraint;
        }

        public Constraint<T> RemoveConstraint(Constraint<T> constraint)
        {
            return Constraints.Remove(constraint) ? constraint : null;
        }

        public NextInstruction Apply(T target, object context)
        {
            if (Constraints == null)
                return NextInstruction.Do;
            foreach (var constraint in Constraints)
            {
                var r = constraint.Apply(target, context);
                if ((r & NextInstruction.Continue) != NextInstruction.Continue)
                    return r;
            }
            return NextInstruction.Do;
        }


    }
}
