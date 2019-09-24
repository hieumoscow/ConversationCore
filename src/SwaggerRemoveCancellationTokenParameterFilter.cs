using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerRemoveCancellationTokenParameterFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
    {
    public void Apply(Operation operation, OperationFilterContext context)
    {
        context.ApiDescription.ParameterDescriptions
                .Where(pd =>
                    pd.ModelMetadata.ContainerType == typeof(System.Threading.CancellationToken) ||
                    pd.ModelMetadata.ContainerType == typeof(System.Threading.WaitHandle) ||
                    pd.ModelMetadata.ContainerType == typeof(Microsoft.Win32.SafeHandles.SafeWaitHandle))
                .ToList()
                .ForEach(
                    pd =>
                    {
                        if (operation.Parameters != null)
                        {
                            var cancellationTokenParameter = operation.Parameters.FirstOrDefault(p => p.Name.Equals(pd.Name, System.StringComparison.OrdinalIgnoreCase));
                            if(cancellationTokenParameter != null)
                                operation.Parameters.Remove(cancellationTokenParameter);
                        }
                    });
    }
}
    