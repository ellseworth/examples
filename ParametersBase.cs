using System;

namespace Common
{
	/// <summary>
	/// параметр, представляющий значение и сообщение его изменении, как единый объект.
	/// объект данного типа не является самостоятельным классом, его должен содержать объект-владелец параметра.
	/// получение значения из данного параметра сводится к его неявному привидению к типу хранимого значения.
	/// про получении нового значения, оно принимается, если не эквивалентно старому (классы сравниваются по ссылкам, структуры - по значениям)
	/// после принятия нового значения вызывается событие <see cref="Changed"/>
	/// </summary>
	/// <typeparam name="Towner">тип владельца параметра</typeparam>
	/// <typeparam name="Tvalue">тип хранимого значения</typeparam>
	public abstract class Parameter<Towner, Tvalue>
	{
		/// <summary>
		/// возвращает хранимое параметром значение
		/// </summary>
		/// <param name="source">приводимый параметр</param>
		/// <returns>текущее значение параметра</returns>
		public static implicit operator Tvalue(Parameter<Towner, Tvalue> source) { return source._value; }

		//владелец параметра, значение закрыто для доступа,
		//так как это противоречит принцыпу использования параметра,
		//как значения какого-либо объекта-владельца параметра
		private Towner _owner;
		//действие, выполняемое перед тем, как изменить значение <старое значение, новое значение>
		private Action<Tvalue, Tvalue> _preChange;

		private Tvalue _value;
		/// <summary>
		/// возвращает или задает значение параметра, если оно изменяется, то вызывается событие <see cref="Changed"/>
		/// </summary>
		protected Tvalue ProtValue
		{
			get { return _value; }
			set
			{
				//проверяем эквивалентность объектов перегруженным методом сравнения
				//если значения одинаковы, ничего не делаем
				if (IsEquals(_value, value)) return;
				//сначала выполняем действия, предусмотренные перед сменой значения
				if (_preChange != null) _preChange(_value, value);
				//запоминаем старое значение, в некоторых случаях это важно
				Tvalue from = _value;
				//запоминаем новое значение
				_value = value;
				//извещаем об изменении
				if (_changed != null) _changed(_owner, from, value);
			}
		}

		//флаг выгрузки параметра
		private bool _isDisposed;
		//выгрузка ресурсов параметра, сами действия выполняются один раз, вне зависимости от количетсва вызовов этого метода
		private void Dispose()
		{
			//предохранитель вызова
			if (_isDisposed) return;
			_isDisposed = true;

			//обнуляем все делегаты и устанавливаем дефолтные значения полей
			_changed = null;
			_preChange = null;
			_value = default(Tvalue);
			_owner = default(Towner);
		}

		//делегат извещения изменения события, явная реализация нужна, чтобы предотвратить двойную подписку
		private Action<Towner, Tvalue, Tvalue> _changed;
		/// <summary>
		/// вызывается, после смены значения в параметре <владелец параметра(для удобства), старое значение, новое значение>
		/// предотвращает двойную подписку одного и того же делегата
		/// </summary>
		public event Action<Towner, Tvalue, Tvalue> Changed
		{
			add { _changed -= value; _changed += value; }
			remove { _changed -= value; }
		}

		/// <summary>
		/// создает параметр, в соответствии с моделью использования данног класса параметр должен создавать его владелец
		/// он же при создании получит возможности управлять этим параметром (установка нового значения, выгрузка параметра)
		/// </summary>
		/// <param name="owner">владелей параметра</param>
		/// <param name="preChange">действие, выполняемое перед сменой значения параметра</param>
		/// <param name="value">первоначально значение параметра, устанавливается , как начальное значение без вызова <see cref="Changed"/></param>
		/// <param name="changeAction">указатель на действие смены значения <новое значение>,
		/// оно должно остатья у владельца параметра после создания</param>
		/// <param name="disposeAction">указатель на действие, выполняющее выгрузку параметра,
		/// оно должно остаться у владельца параметра после создания</param>
		protected Parameter(
			Towner owner, out Action<Tvalue> changeAction, out Action disposeAction,
			Action<Tvalue, Tvalue> preChange = null, Tvalue value = default(Tvalue)
			) : this(owner, out disposeAction, preChange, value)
		{
			changeAction = val => ProtValue = val;
		}

		/// <summary>
		/// создает параметр, в соответствии с моделью использования данного класса параметр должен создавать его владелец
		/// он же при создании получит возможности управлять этим параметром (установка нового значения, выгрузка параметра)
		/// данный конструктор нужен только дл удобства создания наследников, имеющих возможность устанавливать значения параметра публично
		/// </summary>
		/// <param name="owner">владелец параметра</param>
		/// <param name="preChange">действие, выполняемое перед сменой значения параметра</param>
		/// <param name="value">первоначально значение параметра, устанавливается , как начальное значение без вызова <see cref="Changed"/></param>
		/// <param name="disposeAction">указатель на действие, выполняющее выгрузку параметра,
		/// оно должно остаться у владельца параметра после создания</param>
		protected Parameter(
			Towner owner, out Action disposeAction, Action<Tvalue, Tvalue> preChange = null, Tvalue value = default(Tvalue)
			)
		{
			_owner = owner;
			_value = value;
			_preChange = preChange;
			disposeAction = () => Dispose();
		}

		//определяет метод сравнения старого и нового значений
		protected abstract bool IsEquals(Tvalue first, Tvalue second);
	}
	/// <summary>
	/// параметр, представляющий значение и сообщение его изменении, как единый объект.
	/// данный класс дополняет функционал базового <seealso cref="Parameter<Towner, Tvalue>"/>
	/// возможностью изменять значение через свойсотво <see cref="Value"/>
	/// </summary>
	/// <typeparam name="Towner">тип владельца параметра</typeparam>
	/// <typeparam name="Tvalue">тип хранимого значения</typeparam>
	public abstract class ChangableParameter<Towner, Tvalue> : Parameter<Towner, Tvalue>
	{
		/// <summary>
		/// открытое свойство для установки нового значения
		/// </summary>
		public Tvalue Value { set { ProtValue = value; } }

		/// <summary>
		/// создает параметр, значение которого публично доступно для установки,
		/// в соответствии с моделью использования данного класса параметр должен создавать его владелец
		/// он же при создании получит возможности управлять этим параметром (выгрузка параметра)
		/// </summary>
		/// <param name="owner">владелец параметра</param>
		/// <param name="disposeAction">указатель на действие, выполняющее выгрузку параметра,
		/// оно должно остаться у владельца параметра после создания</param>
		/// <param name="preChange">действие, выполняемое перед сменой значения параметра</param>
		/// <param name="value">первоначальное значение параметра, устанавливается, как начальное значение без вызова <see cref="Changed"/></param>
		protected ChangableParameter(
			Towner owner, out Action disposeAction, Action<Tvalue, Tvalue> preChange = null, Tvalue value = default(Tvalue) 
			) : base(owner, out disposeAction, preChange, value) { }
	}
}
