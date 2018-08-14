using System;

namespace Kernel.Runtime
{
    public interface IWork
    {
        float GetTotal();
        float GetCurrent();
        float GetProgress();
        string GetLabel();
        bool IsFinished();
        bool IsStarted();
        void Start();
        void Tick(float deltaTime);
        void AddOnStart(Action onStart);
        void AddOnFinish(Action onFinish);
    }
}
