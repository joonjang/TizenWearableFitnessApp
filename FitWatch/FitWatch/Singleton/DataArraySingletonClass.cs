using FitWatch.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FitWatch.Singleton
{

	public sealed class DataArraySingletonClass
	{
		//the volatile keyword ensures that the instantiation is complete 
		//before it can be accessed further helping with thread safety.
		private static volatile DataArraySingletonClass _instance;
		private static readonly object _syncLock = new object();
		private DataArraySingletonClass()
		{

		}

		//uses a pattern known as double check locking
		public static DataArraySingletonClass Instance
		{
			get
			{
				if (_instance != null) return _instance;
				lock (_syncLock)
				{
					if (_instance == null)
					{
						_instance = new DataArraySingletonClass();
					}
				}
				return _instance;
			}
		}

		public DataArrayModel DataArrayObject { get; set; }
	}
}
