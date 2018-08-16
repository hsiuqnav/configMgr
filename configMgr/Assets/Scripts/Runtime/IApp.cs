namespace Kernel.Runtime
{
	public interface IApp
	{
		float DeltaTime { get; }

		float UnscaledDeltaTime
		{
			get;
		}

		void Awake();
		void Update();
		void OnApplicationPause(bool pause);
		void OnApplicationFocus(bool focus);
		void OnApplicationQuit();
	}
}