using System;

namespace Common
{
	/// <summary>
	/// создает параметры,изменение значений которых можно отслеживать
	/// </summary>
	internal class TrackedParameter
	{
		#region definition
		/// <summary>
		/// параметр, который может изменять и выгружать только его создатель
		/// (через out-делегаты, получаемые при вызове TrackedParameter.Create-методов)
		/// при выгрузке не требуется отписка наблюдателей
		/// </summary>
		/// <typeparam name="T">тип хранимого значения</typeparam>
		public interface IParameter<T>
		{
			/// <summary>
			/// текущее значение параметра
			/// </summary>
			T Value { get; }
			/// <summary>
			/// вызывается после изменения параметра (сам параметр, старое значение)
			/// </summary>
			event Action<IParameter<T>, T> Changed;
		}
		/// <summary>
		/// аналогичен IParameter для типа <typeparam name="T"/>,
		/// только позволяет устанавливать значение всем, кто получт ссылку на этот параметр
		/// </summary>
		public interface IChangableParameter<T> : IParameter<T>
		{
			/// <summary>
			/// задает новое значение
			/// </summary>
			new T Value { get; set; }
		}

		//базовый шаблон для закрыто изменяемых параметров, реализует все, кроме метода сравнения значений
 		//при выгрузке сам отписывает наблюдателей
		private abstract class ParameterBase<T> : IParameter<T>
		{
			//действие, выполняемое перед тем, как изменить значения (старое значение, новое значение)
			public Action<T, T> _preChange;

			#region IParameter<T> Members
			private T _value;
			public T Value
			{
				get { return _value; }
				set
				{
					if (IsEquals(_value, value)) return;
					if (_preChange != null) _preChange(_value, value);
					T from = _value;
					_value = value;
					if (_changed != null) _changed(this, from);
				}
			}

			private bool _isDisposed;
			public void Dispose()
			{
				if(_isDisposed) return;
				_isDisposed = true;
				
				_changed = null;
				_preChange = null;
				_value = default(T);			
			}

			private Action<IParameter<T>, T> _changed;
			public event Action<IParameter<T>, T> Changed
			{
				add { _changed -= value; _changed += value; } remove { _changed -= value; }
			}
			#endregion

			public ParameterBase(Action<T, T> preChange, T value)
			{
				_value = value;
				_preChange = preChange;
			}

			//определяет метод сравнения старого и нового значений
			protected abstract bool IsEquals(T first, T second);
		}
		//аналогичен ParameterBase, только позволяет изменять значения извне
		private abstract class ChangableParameterBase<T> : ParameterBase<T>, IChangableParameter<T>
		{
			new public T Value { set { base.Value = value; } }

			public ChangableParameterBase(Action<T, T> preChange, T value) : base(preChange, value) { }
		}

		//реализует вариант ParameterBase для работы с ссылочными типами данных
		private class ParameterForClass<T> : ParameterBase<T> where T : class
		{
			public ParameterForClass(Action<T, T> preChange, T value) : base(preChange, value) { }

			//выдает true, если ссылки ссылаются на один объект
			protected override bool IsEquals(T first, T second) { return object.ReferenceEquals(first, second); }
		}
		//реализует вариант ParameterBase для работы с типами значений,
		//для предотвращения упаковки при сравнении такие типы длжны реализовать IEquatable<T>
		private class ParameterForStruct<T> : ParameterBase<T> where T : struct, IEquatable<T>
		{
			public ParameterForStruct(Action<T, T> preChange, T value) : base(preChange, value) { }

			//сравнивает два значения по определенному в них методу Equals
			protected override bool IsEquals(T first, T second) { return ((IEquatable<T>)first).Equals(second); }
		}
		//аналогичен ParameterForClass, только расширяет ChangableParameterBase
		private class ChangebleParameterForClass<T> : ChangableParameterBase<T> where T : class
		{
			public ChangebleParameterForClass(Action<T, T> preChange, T value) : base(preChange, value) { }

			//выдает true, если ссылки ссылаются на один объект
			protected override bool IsEquals(T first, T second) { return object.ReferenceEquals(first, second); }
		}
		//аналогичен ParameterForStruct, только расширяет ChangableParameterBase
		private class ChangebleParameterForStruct<T> : ChangableParameterBase<T> where T : struct, IEquatable<T>
		{
			public ChangebleParameterForStruct(Action<T, T> preChange, T value) : base(preChange, value) { }

			//сравнивает два значения по определенному в них методу Equals
			protected override bool IsEquals(T first, T second) { return ((IEquatable<T>)first).Equals(second); }
		}
		#endregion

		/// <summary>
		/// создает новый параметр IParameter, хранящий значения указанного в <typeparamref name="T"/> ссылочного типа 
		/// </summary>
		/// <typeparam name="T">тип хранимого значения, должен быть ссылочным</typeparam>
		/// <param name="preChangeAction">указатель на метод, который вызывается до присвоения параметру нового значения,
		/// можно не задавать</param>
		/// <param name="changeAction">указатель на метод, с помошью которого можно изменить значение данного параметра</param>
		/// <param name="disposeAction">указатель на метод, с помошью которого можно выгрузить данный параметр</param>
		/// <param name="defaultValue">начальное значение параметра</param>
		public static IParameter<T> CreateForClass<T>(
			Action<T, T> preChangeAction, out Action<T> changeAction, out Action disposeAction, T defaultValue = default(T)
			) where T : class
		{
			return BindActions(new ParameterForClass<T>(preChangeAction, defaultValue), out changeAction, out disposeAction);
		}
		/// <summary>
		/// создает новый параметр IParameter, хранящий значения указанного в <typeparamref name="T"/> типа значений,
		/// реализующего IEquatable для <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">тип хранимого значения,
		/// должен быть типом значений, и неализовывать IEquatable для <typeparamref name="T"/></typeparam>
		/// <param name="preChangeAction">указатель на метод, который вызывается до присвоения параметру нового значения,
		/// можно не задавать</param>
		/// <param name="changeAction">указатель на метод, с помошью которого можно изменить значение данного параметра</param>
		/// <param name="disposeAction">указатель на метод, с помошью которого можно выгрузить данный параметр</param>
		/// <param name="defaultValue">начальное значение параметра</param>
		public static IParameter<T> CreateForStruct<T>(
			Action<T, T> preChangeAction, out Action<T> changeAction, out Action disposeAction, T defaultValue = default(T)
			) where T : struct, IEquatable<T>
		{
			return BindActions(new ParameterForStruct<T>(preChangeAction, defaultValue), out changeAction, out disposeAction);
		}
		/// <summary>
		/// создает новый параметр IChangableParameter, хранящий значения указанного в <typeparamref name="T"/> ссылочного типа
		/// </summary>
		/// <typeparam name="T">тип хранимого значения, должен быть ссылочным</typeparam>
		/// <param name="preChangeAction">указатель на метод, который вызывается до присвоения параметру нового значения,
		/// можно не задавать</param>
		/// <param name="disposeAction">указатель на метод, с помошью которого можно выгрузить данный параметр</param>
		/// <param name="defaultValue">начальное значение параметра</param>
		public static IChangableParameter<T> CreateChangableForClass<T>(
			Action<T, T> preChangeAction, out Action disposeAction, T defaultValue = default(T)
			) where T : class
		{
			return BindActionsChangable<T>(new ChangebleParameterForClass<T>(preChangeAction, defaultValue), out disposeAction);
		}
		/// <summary>
		/// создает новый параметр IChangableParameter, хранящий значения указанного в <typeparamref name="T"/> типа значений,
		/// реализующего IEquatable для <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">тип хранимого значения,
		/// должен быть типом значений, и неализовывать IEquatable для <typeparamref name="T"/></typeparam>
		/// <param name="preChangeAction">указатель на метод, который вызывается до присвоения параметру нового значения,
		/// можно не задавать</param>
		/// <param name="disposeAction">указатель на метод, с помошью которого можно выгрузить данный параметр</param>
		/// <param name="defaultValue">начальное значение параметра</param>
		public static IChangableParameter<T> CreateChangableForStruct<T>(
			Action<T, T> preChangeAction, out Action disposeAction, T defaultValue = default(T)
			) where T : struct, IEquatable<T>
		{
			return BindActionsChangable<T>(new ChangebleParameterForStruct<T>(preChangeAction, defaultValue), out disposeAction);
		}

		//выполняет общие операции для Create-методов
		private static ParameterBase<T> BindActions<T>(
			ParameterBase<T> newParameter, out Action<T> changeAction, out Action disposeAction
			)
		{
			changeAction = val => newParameter.Value = val; disposeAction = () => newParameter.Dispose(); return newParameter;
		}
		//выполняет общие операции для CreateChangable-методов
		private static ChangableParameterBase<T> BindActionsChangable<T>(
			ChangableParameterBase<T> newParameter, out Action disposeAction
			)
		{
			disposeAction = () => newParameter.Dispose(); return newParameter;
		}
	}
}