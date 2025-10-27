namespace MoiraAlertPostprocessor.Api.Controllers.MoiraAlertRecieve.Dtos
{
    /// <summary>
    /// recird for checking health of API
    /// </summary>
    public record HealthCheckStatus
    {
        /// <summary>
        /// Status
        /// </summary>
        public required string Status { get; init; }

        /// <summary>
        /// Date of check
        /// </summary>
        public required DateTime Date { get; set; }
    }
}
