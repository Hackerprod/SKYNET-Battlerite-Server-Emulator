namespace CodeProject.ObjectPool.Core
{
	internal static class ErrorMessages
	{
		public const string NegativeOrZeroMaximumPoolSize = "Maximum pool size must be greater than zero.";

		public const string NegativeOrZeroTimeout = "Timeout must be greater than zero.";

		public const string NullResource = "Resource cannot be null.";
	}
}
