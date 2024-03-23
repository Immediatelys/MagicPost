using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MagicPost
{
    public static class ModelStateMappingError
    {
        public static object MappingError(this ModelStateDictionary ModelState, string traceId)
        {
            var errors = ModelState.Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );
            return new { errors, traceId, status = 400 };
        }
    }
}
