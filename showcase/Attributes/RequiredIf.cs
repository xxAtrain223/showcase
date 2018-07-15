using DynamicExpresso;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace showcase.Attributes
{
    public class RequiredIfAttribute : RequiredAttribute
    {
        public string Expression { get; set; }
        public Type[] ImportTypes { get; set; }

        public RequiredIfAttribute(string expression)
        {
            Expression = expression;
        }

        public RequiredIfAttribute(string expression, params Type[] importTypes)
        {
            Expression = expression;
            ImportTypes = importTypes;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Object instance = validationContext.ObjectInstance;
            Type type = instance.GetType();
            
            var interpreter = new Interpreter();
            foreach (Type t in ImportTypes)
            {
                interpreter.Reference(t);
            }
            var parameters = type.GetProperties().Select(p => new Parameter(p.Name, p.PropertyType, p.GetValue(instance))).ToArray();
            bool expressionResult = interpreter.Eval<bool>(Expression, parameters);

            if (!expressionResult)
            {
                return ValidationResult.Success;
            }
            return base.IsValid(value, validationContext);
        }
    }
}
