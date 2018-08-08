namespace Kernel
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                return instance ?? (instance = new T());
            }
            protected set
            {
                instance = value;
            }
        }
    }
}
