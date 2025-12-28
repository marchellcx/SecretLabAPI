using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Enums;
using SecretLabAPI.Actions.Attributes;

using UnityEngine;

namespace SecretLabAPI.Actions.Functions
{
    /// <summary>
    /// Provides static methods for performing numeric operations.
    /// </summary>
    public static class NumericFunctions
    {
        /// <summary>
        /// Compares two integer values and determines whether they are equal.
        /// </summary>
        /// <remarks>The result of the equality comparison is stored in the action context's memory as a
        /// Boolean value: <see langword="true"/> if the values are equal; otherwise, <see langword="false"/>. This
        /// method does not throw exceptions for invalid types; ensure that both values in the context are of type
        /// Int32.</remarks>
        /// <param name="context">A reference to the current action context. The context must provide two integer values: the first is the
        /// value to compare, and the second is the target value.</param>
        /// <returns>An ActionResultFlags value indicating the result of the action. The comparison result is stored in the
        /// action context's memory.</returns>
        [Action("IsEqualTo", "Checks if two integer values are equal.")]
        [ActionParameter("Variable", "The name of the variable that contains the first number - must be an Int32.")]
        [ActionParameter("Target", "The target number to compare against - must be an Int32.")]
        public static ActionResultFlags IsEqualTo(ref ActionContext context)
        {
            var value = context.GetValue<int>(0);
            var target = context.GetValue<int>(1);

            context.SetMemory(value == target);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Determines whether the value stored in the specified variable is greater than the target value.
        /// </summary>
        /// <remarks>The result of the comparison is written to the action context's memory. The stored
        /// Boolean is <see langword="true"/> if the variable's value is greater than the target; otherwise, <see
        /// langword="false"/>.</remarks>
        /// <param name="context">A reference to the current action context containing the variable and target values to compare. The variable
        /// and target must both be of type Int32.</param>
        /// <returns>An ActionResultFlags value indicating the result of the action. The comparison result is stored in the
        /// action context's memory as a Boolean value.</returns>
        [Action("IsMoreThan", "Checks if a value is larger than another value.")]
        [ActionParameter("Variable", "The name of the variable that contains the first number - must be an Int32.")]
        [ActionParameter("Target", "The target number to compare against - must be an Int32.")]
        public static ActionResultFlags IsMoreThan(ref ActionContext context)
        {
            var value = context.GetValue<int>(0);
            var target = context.GetValue<int>(1);

            context.SetMemory(value > target);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Determines whether the value stored in the specified variable is greater than the target value.
        /// </summary>
        /// <remarks>The result of the comparison is written to the action context's memory. The stored
        /// Boolean is <see langword="true"/> if the variable's value is greater than the target; otherwise, <see
        /// langword="false"/>.</remarks>
        /// <param name="context">A reference to the current action context containing the variable and target values to compare. The variable
        /// and target must both be of type Int32.</param>
        /// <returns>An ActionResultFlags value indicating the result of the action. The comparison result is stored in the
        /// action context's memory as a Boolean value.</returns>
        [Action("IsMoreThanOrEqual", "Checks if a value is larger or equal to another value.")]
        [ActionParameter("Variable", "The name of the variable that contains the first number - must be an Int32.")]
        [ActionParameter("Target", "The target number to compare against - must be an Int32.")]
        public static ActionResultFlags IsMoreThanOrEqual(ref ActionContext context)
        {
            var value = context.GetValue<int>(0);
            var target = context.GetValue<int>(1);

            context.SetMemory(value >= target);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Determines whether the value stored in the specified variable is less than the target value.
        /// </summary>
        /// <remarks>The result of the comparison is written to the action context's memory as a Boolean
        /// value. Use this result in subsequent actions as needed.</remarks>
        /// <param name="context">A reference to the current action context containing the variable and target values to compare. The variable
        /// and target must be of type Int32.</param>
        /// <returns>An ActionResultFlags value indicating the result of the action. The comparison result is stored in the
        /// action context's memory.</returns>
        [Action("IsLessThan", "Checks if a value is smaller than another value.")]
        [ActionParameter("Variable", "The name of the variable that contains the first number - must be an Int32.")]
        [ActionParameter("Target", "The target number to compare against - must be an Int32.")]
        public static ActionResultFlags IsLessThan(ref ActionContext context)
        {
            var value = context.GetValue<int>(0);
            var target = context.GetValue<int>(1);

            context.SetMemory(value < target);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Determines whether the value stored in the specified variable is less than the target value.
        /// </summary>
        /// <remarks>The result of the comparison is written to the action context's memory as a Boolean
        /// value. Use this result in subsequent actions as needed.</remarks>
        /// <param name="context">A reference to the current action context containing the variable and target values to compare. The variable
        /// and target must be of type Int32.</param>
        /// <returns>An ActionResultFlags value indicating the result of the action. The comparison result is stored in the
        /// action context's memory.</returns>
        [Action("IsLessThanOrEqual", "Checks if a value is smaller than another value.")]
        [ActionParameter("Variable", "The name of the variable that contains the first number - must be an Int32.")]
        [ActionParameter("Target", "The target number to compare against - must be an Int32.")]
        public static ActionResultFlags IsLessThanOrEqual(ref ActionContext context)
        {
            var value = context.GetValue<int>(0);
            var target = context.GetValue<int>(1);

            context.SetMemory(value <= target);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Calculates the percentage that one integer value represents of another and stores the result in the action
        /// context.
        /// </summary>
        /// <remarks>If the total value is zero, the result stored in the context will be zero to avoid
        /// division by zero. The calculated percentage is rounded up to the nearest integer before being
        /// stored.</remarks>
        /// <param name="context">The action context containing the input values. The first value is the number to calculate the percentage
        /// for; the second value is the total to calculate the percentage against. The result is stored in the
        /// context's memory.</param>
        /// <returns>An ActionResultFlags value indicating the outcome of the operation. Returns SuccessDispose when the
        /// calculation completes.</returns>
        [Action("PercentageOf", "Calculates what percentage the first number is of the second number.")]
        [ActionParameter("Value", "The value to calculate the percentage for - must be an Int32.")]
        [ActionParameter("Total", "The total value to calculate the percentage against - must be an Int32.")]
        public static ActionResultFlags PercentageOf(ref ActionContext context)
        {
            var value = context.GetValue<int>(0);
            var total = context.GetValue<int>(1);

            if (total <= 0)
            {
                context.SetMemory(0);
            }
            else
            {
                var percentage = (float)value / total * 100;

                context.SetMemory(Mathf.CeilToInt(percentage));
            }

            return ActionResultFlags.SuccessDispose;
        }
    }
}