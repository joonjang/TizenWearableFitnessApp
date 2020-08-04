using FitWatch.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FitWatch.Singleton
{
    //https://github.com/skimedic/presentations/blob/25b94da882b0b9787152b51e2ab8ae0daf66b16a/Patterns/Current/Creational/Singleton/MySingletonClass.cs
    public sealed class WatchSingletonClass
    {
        //the volatile keyword ensures that the instantiation is complete 
        //before it can be accessed further helping with thread safety.
        private static volatile WatchSingletonClass _instance;
        private static readonly object _syncLock = new object();
        private WatchSingletonClass()
        {

        }

		//uses a pattern known as double check locking
		public static WatchSingletonClass Instance
		{
			get
			{
				if (_instance != null) return _instance;
				lock (_syncLock)
				{
					if (_instance == null)
					{
						_instance = new WatchSingletonClass();
					}
				}
				return _instance;
			}
		}

		public WatchModel WatchObject { get; set; }
	}
}
