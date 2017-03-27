using GraphQL;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Validation;
using Nitrolize.Context;
using Nitrolize.Extensions;

namespace Nitrolize.Validation
{
    /// <summary>
    /// The validation roles for authorization.
    /// Checks on field access if the current user is logged in and if it has sufficient
    /// privileges by comparing the user roles with the required roles of the field.
    /// </summary>
    public class RequiresAuthValidationRule<TUserId> : IValidationRule
    {
        /// <summary>
        /// Validates each node on a query path.
        /// </summary>
        public INodeVisitor Validate(ValidationContext context)
        {
            var userContext = context.UserContext.As<IUserContext<TUserId>>();

            return new EnterLeaveListener(_ =>
            {
                _.Match<Field>(fieldAst =>
                {
                    var fieldDef = context.TypeInfo.GetFieldDef();

                    // check if field could be resolved
                    if (!this.FieldCouldBeResolved(fieldAst, fieldDef, context))
                    {
                        return;
                    }

                    // check if authentication is required
                    this.CheckForAuthentication(fieldAst, fieldDef, context, userContext);

                    // check if role requirements are met
                    this.CheckForAuthorization(fieldAst, fieldDef, context, userContext);
                });
            });
        }

        /// <summary>
        /// Checks if a field was resolved correctly and reports an error otherwise.
        /// </summary>
        private bool FieldCouldBeResolved(Field field, FieldType fieldType, ValidationContext context)
        {
            if (fieldType == null)
            {
                context.ReportError(new ValidationError(
                    context.OriginalQuery,
                    "auth-required",
                    $"This query could not be resolved: {context.OriginalQuery}",
                    field));

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if authentication is generally turned on on this field and if authentication
        /// is mandatory and the user is authenticated. Reports an error on any fail.
        /// </summary>
        private void CheckForAuthentication(Field field, FieldType fieldType, ValidationContext context, IUserContext<TUserId> userContext)
        {
            if (fieldType.RequiresAuthenticationCheck() && fieldType.RequiresAuthentication() && !userContext.IsUserAuthenticated)
            {
                context.ReportError(new ValidationError(
                    context.OriginalQuery,
                    "auth-required",
                    "You are not authorized to run this query.",
                    field));
            }
        }

        /// <summary>
        /// Checks if authorization is mandatory on this field and if the user roles meet the field requirements.
        /// Reports an error on any fail.
        /// </summary>
        private void CheckForAuthorization(Field field, FieldType fieldType, ValidationContext context, IUserContext<TUserId> userContext)
        {
            if (fieldType.RequiresRoles() && !fieldType.CanAccess(userContext.UserRoles))
            {
                context.ReportError(new ValidationError(
                    context.OriginalQuery,
                    "auth-required",
                    "You are not authorized to run this query.",
                    field));
            }
        }
    }
}
