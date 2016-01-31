using System;

namespace Common
{
	/// <summary>
	/// статичная фабрика, создает реализации параметров
	/// <seealso cref="Parameter<Towner, Tvalue>"/> и <seealso cref="ChangableParameterBase<Towner, Tvalue>"/>
	/// эти реализации скрыты, и отличаются только способами сравнения значений
	/// </summary>
	public class Parameter
	{
		#region definition
		//реализует вариант ParameterBase для работы с ссылочными типами данных
		private class ForClass<Town, Tval> : Parameter<Town, Tval> where Tval : class
		{
			public ForClass(Town owner, out Action<Tval> change, out Action dispose, Action<Tval, Tval> preChange, Tval val) :
				base(owner, out change, out dispose, preChange, val) { }

			//выдает true, если ссылки ссылаются на один объект
			protected override bool IsEquals(Tval first, Tval second) { return object.ReferenceEquals(first, second); }
		}
		//реализует вариант ParameterBase для работы с типами значений,
		//для предотвращения упаковки при сравнении такие типы должны реализовать IEquatable<T>
		private class ForStruct<Town, Tval> : Parameter<Town, Tval> where Tval : struct, IEquatable<Tval>
		{
			public ForStruct(Town owner, out Action<Tval> change, out Action dispose, Action<Tval, Tval> preChange, Tval val) :
				base(owner, out change, out dispose, preChange, val) { }

			//сравнивает два значения по определенному в них методу Equals
			protected override bool IsEquals(Tval first, Tval second) { return ((IEquatable<Tval>)first).Equals(second); }
		}
		//реализует вариант ParameterBase для работы в енумами
		private class ForEnum<Town, Tval> : Parameter<Town, Tval> where Tval : struct, IComparable, IFormattable, IConvertible
		{
			public ForEnum(Town owner, out Action<Tval> change, out Action dispose, Action<Tval, Tval> preChange, Tval val) :
				base(owner, out change, out dispose, preChange, val) { }
			
			protected override bool IsEquals(Tval first, Tval second) { return first.GetHashCode() == second.GetHashCode(); }
		}
		//аналогичен ParameterForClass, только расширяет ChangableParameterBase
		private class ChangebleForClass<Town, Tval> : ChangableParameter<Town, Tval> where Tval : class
		{
			public ChangebleForClass(Town owner, out Action dispose, Action<Tval, Tval> preChange, Tval val) :
				base(owner, out dispose, preChange, val) { }

			//выдает true, если ссылки ссылаются на один объект
			protected override bool IsEquals(Tval first, Tval second) { return object.ReferenceEquals(first, second); }
		}
		//аналогичен ParameterForStruct, только расширяет ChangableParameterBase
		private class ChangebleForStruct<Town, Tval> : ChangableParameter<Town, Tval> where Tval : struct, IEquatable<Tval>
		{
			public ChangebleForStruct(Town owner, out Action dispose, Action<Tval, Tval> preChange, Tval val) :
				base(owner, out dispose, preChange, val) { }

			//сравнивает два значения по определенному в них методу Equals
			protected override bool IsEquals(Tval first, Tval second) { return ((IEquatable<Tval>)first).Equals(second); }
		}
		//аналогичен ParameterForEnum, только расширяет ChangableParameterBase
		private class ChangableForEnum<Town, Tval> : ChangableParameter<Town, Tval> where Tval : struct, IComparable, IFormattable, IConvertible
		{
			public ChangableForEnum(Town owner, out Action dispose, Action<Tval, Tval> preChange, Tval val) :
				base(owner, out dispose, preChange, val) { }

			protected override bool IsEquals(Tval first, Tval second) { return first.GetHashCode() == second.GetHashCode(); }
		}
		#endregion

		/// <summary>
		/// создает новый параметр <see cref="Parameter<Towner, Tvalue>"/>, хранящий значения ссылочного <paramref name="Tvalue"/> типа 
		/// </summary>
		/// <typeparam name="Towner">тип владельца параметра</typeparam>
		/// <typeparam name="Tvalue">тип хранимого значения, должен быть ссылочным</typeparam>
		/// <param name="owner">хранитель и, скорее всего, создатель параметра</param>
		/// <param name="changeAction">указатель на метод, меняющий в параметре значение</param>
		/// <param name="disposeAction">указатель на метод,выгружающий параметр</param>
		/// <param name="preChangeAction">действие, выполняемое перед сменой значения</param>
		/// <param name="defaultValue">первоначальное значение</param>
		/// <returns>не null-значение</returns>
		public static Parameter<Towner, Tvalue> CreateForClass<Towner, Tvalue>(
			Towner owner, out Action<Tvalue> changeAction, out Action disposeAction,
			Action<Tvalue, Tvalue> preChangeAction = null, Tvalue defaultValue = default(Tvalue)
			) where Tvalue : class
		{
			return new ForClass<Towner, Tvalue>(owner, out changeAction, out disposeAction, preChangeAction, defaultValue);
		}
		/// <summary>
		/// создает новый параметр <see cref="Parameter<Towner, Tvalue>"/>, хранящий значения типа значения <paramref name="Tvalue"/> 
		/// </summary>
		/// <typeparam name="Towner">тип владельца параметра</typeparam>
		/// <typeparam name="Tvalue">тип хранимого значения, должен быть типом значения</typeparam>
		/// <param name="owner">хранитель и, скорее всего, создатель параметра</param>
		/// <param name="changeAction">указатель на метод, меняющий в параметре значение</param>
		/// <param name="disposeAction">указатель на метод, выгружающий параметр</param>
		/// <param name="preChangeAction">действие, выполняемое перед сменой значения</param>
		/// <param name="defaultValue">первоначальное значение</param>
		/// <returns>не null-значение</returns>
		public static Parameter<Towner, Tvalue> CreateForStruct<Towner, Tvalue>(
			Towner owner, out Action<Tvalue> changeAction, out Action disposeAction,
			Action<Tvalue, Tvalue> preChangeAction = null, Tvalue defaultValue = default(Tvalue)
			) where Tvalue : struct, IEquatable<Tvalue>
		{
			return new ForStruct<Towner, Tvalue>(owner, out changeAction, out disposeAction, preChangeAction, defaultValue);
		}
		/// <summary>
		/// создает новый параметр <see cref="Parameter<Towner, Tvalue>"/>, хранящий значения enum-типа <paramref name="Tvalue"/> 
		/// </summary>
		/// <typeparam name="Towner">тип владельца параметра</typeparam>
		/// <typeparam name="Tvalue">тип хранимого значения, его базовым типом должен быть <seealso cref="Enum"/></typeparam>
		/// <param name="owner">хранитель и, скорее всего, создатель параметра</param>
		/// <param name="changeAction">указатель на метод, меняющий в параметре значение</param>
		/// <param name="disposeAction">указатель на метод,выгружающий параметр</param>
		/// <param name="preChangeAction">действие, выполняемое перед сменой значения</param>
		/// <param name="defaultValue">первоначальное значение</param>
		/// <returns>не null-значение</returns>
		public static Parameter<Towner, Tvalue> CreateForEnum<Towner, Tvalue>(
			Towner owner, out Action<Tvalue> changeAction, out Action disposeAction,
			Action<Tvalue, Tvalue> preChangeAction = null, Tvalue defaultValue = default(Tvalue)
			) where Tvalue : struct, IComparable, IFormattable, IConvertible
		{
			if(typeof(Tvalue).BaseType != typeof(Enum))
				throw new ArgumentException(string.Format("Тип {0} не наследует Enum (не является enum)", typeof(Tvalue).FullName));
			return new ForEnum<Towner, Tvalue>(owner, out changeAction, out disposeAction, preChangeAction, defaultValue);
		}
		/// <summary>
		/// создает новый параметр <see cref="ChangableParameter<Towner, Tvalue>"/>, хранящий значения ссылочного <paramref name="Tvalue"/> типа 
		/// </summary>
		/// <typeparam name="Towner">тип владельца параметра</typeparam>
		/// <typeparam name="Tvalue">тип хранимого значения, должен быть ссылочным</typeparam>
		/// <param name="owner">хранитель и, скорее всего, создатель параметра</param>
		/// <param name="disposeAction">указатель на метод,выгружающий параметр</param>
		/// <param name="preChangeAction">действие, выполняемое перед сменой значения</param>
		/// <param name="defaultValue">первоначальное значение</param>
		/// <returns>не null-значение</returns>
		public static ChangableParameter<Towner, Tvalue> CreateChangableForClass<Towner, Tvalue>(
			Towner owner, out Action disposeAction, Action<Tvalue, Tvalue> preChangeAction = null, Tvalue defaultValue = default(Tvalue)
			) where Tvalue : class
		{
			return new ChangebleForClass<Towner, Tvalue>(owner, out disposeAction, preChangeAction, defaultValue);
		}
		/// <summary>
		/// создает новый параметр <see cref="ChangableParameter<Towner, Tvalue>"/>, хранящий значения типа значения <paramref name="Tvalue"/> 
		/// </summary>
		/// <typeparam name="Towner">тип владельца параметра</typeparam>
		/// <typeparam name="Tvalue">тип хранимого значения, должен быть типом значения</typeparam>
		/// <param name="owner">хранитель и, скорее всего, создатель параметра</param>
		/// <param name="disposeAction">указатель на метод, выгружающий параметр</param>
		/// <param name="preChangeAction">действие, выполняемое перед сменой значения</param>
		/// <param name="defaultValue">первоначальное значение</param>
		/// <returns>не null-значение</returns>
		public static ChangableParameter<Towner, Tvalue> CreateChangableForStruct<Towner, Tvalue>(
			Towner owner, out Action disposeAction, Action<Tvalue, Tvalue> preChangeAction, Tvalue defaultValue = default(Tvalue)
			) where Tvalue : struct, IEquatable<Tvalue>
		{
			return new ChangebleForStruct<Towner, Tvalue>(owner, out disposeAction, preChangeAction, defaultValue);
		}
		/// <summary>
		/// создает новый параметр <see cref="ChangableParameter<Towner, Tvalue>"/>, хранящий значения enum-типа <paramref name="Tvalue"/> 
		/// </summary>
		/// <typeparam name="Towner">тип владельца параметра</typeparam>
		/// <typeparam name="Tvalue">тип хранимого значения, его базовым типом должен быть <seealso cref="Enum"/></typeparam>
		/// <param name="owner">хранитель и, скорее всего, создатель параметра</param>
		/// <param name="disposeAction">указатель на метод,выгружающий параметр</param>
		/// <param name="preChangeAction">действие, выполняемое перед сменой значения</param>
		/// <param name="defaultValue">первоначальное значение</param>
		/// <returns>не null-значение</returns>
		public static ChangableParameter<Towner, Tvalue> CreateChangableForEnum<Towner, Tvalue>(
			Towner owner, out Action disposeAction, Action<Tvalue, Tvalue> preChangeAction, Tvalue defaultValue = default(Tvalue)
			) where Tvalue : struct, IComparable, IFormattable, IConvertible
		{
			return new ChangableForEnum<Towner, Tvalue>(owner, out disposeAction, preChangeAction, defaultValue);
		}
	}
}