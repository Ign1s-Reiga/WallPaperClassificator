namespace WallPaperClassificator.Util
{
	public static class Result
	{
		public static ResultData<T> Ok<T>(T result)
		{
			return new ResultData<T>(ResultStatus.Ok, result);
		}

		public static ResultData<T> Error<T>(T reason)
		{
			return new ResultData<T>(ResultStatus.Error, reason);
		}
	}

	public enum ResultStatus
	{
		Ok,
		Error,
	}

	public record struct ResultData<T>(ResultStatus Status, T Value);
}
