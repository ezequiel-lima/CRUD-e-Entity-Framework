using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Blog.Extensions
{
    // por padrão as classes de extensão devem ser static e os métodos tambem
    public static class ModelStateExtension
    {
        // não pode esquecer o this para tornar um método de extensão
        public static List<string> GetErros(this ModelStateDictionary modelState)
        {
            var result = new List<string>();
            foreach (var item in modelState.Values)
                result.AddRange(item.Errors.Select(error => error.ErrorMessage));
                
            return result;
        }
    }
}
