using System;

namespace Common
{
	/// <summary>
	/// создает события, не требующие отписки при выгрузке
	/// </summary>
	internal class DisposableNotifier
	{
		/// <summary>
		/// событие без дополнительных параметров, предотвращает повторную подписку, передает ссылку на источник события
		/// </summary>
		/// <typeparam name="TSource">тип источника события</typeparam>
		public interface INotifier<TSource>
		{
			/// <summary>
			/// добавляет подписчика
			/// </summary>
			void Add(Action<TSource> action);
			/// <summary>
			/// удаляет подписчика
			/// </summary>
			void Remove(Action<TSource> action);
		}
		/// <summary>
		/// событие без с одним дополнительным параметром, предотвращает повторную подписку, передает ссылку на источник события
		/// </summary>
		/// <typeparam name="TSource">тип источника события</typeparam>
		public interface INotifier<TSoutce, V1>
		{
			/// <summary>
			/// добавляет подписчика
			/// </summary>
			void Add(Action<TSoutce, V1> action);
			/// <summary>
			/// удаляет подписчика
			/// </summary>
			void Remove(Action<TSoutce, V1> action);
		}
		/// <summary>
		/// событие без с двумя дополнительными параметроми, предотвращает повторную подписку, передает ссылку на источник события
		/// </summary>
		/// <typeparam name="TSource">тип источника события</typeparam>
		public interface INotifier<TSource, V1, V2>
		{
			/// <summary>
			/// добавляет подписчика
			/// </summary>
			void Add(Action<TSource, V1, V2> action);
			/// <summary>
			/// удаляет подписчика
			/// </summary>
			void Remove(Action<TSource, V1, V2> action);
		}
		/// <summary>
		/// событие без с тремя дополнительными параметроми, предотвращает повторную подписку, передает ссылку на источник события
		/// </summary>
		/// <typeparam name="TSource">тип источника события</typeparam>
		public interface INotifier<TSource, V1, V2, V3>
		{
			/// <summary>
			/// добавляет подписчика
			/// </summary>
			void Add(Action<TSource, V1, V2, V3> action);
			/// <summary>
			/// удаляет подписчика
			/// </summary>
			void Remove(Action<TSource, V1, V2, V3> action);
		}

		//базовый класс для всех событий, содержит управление ссылкой на источник, может быть пустой
		//все наследники соделжат списки подписчиков, которые очищаются при выгрузке
		private abstract class NotifierBase<TSource>
		{
			protected TSource Source { get; private set; }

			public NotifierBase(TSource source) { Source = source; }

			public void Dispose() { Source = default(TSource); DisposeActions(); }
			
			//нужен для выгрузки дополнительно задействованных ресурсов в наследниках
			protected abstract void DisposeActions(); 
		}
		private class Notifier<TSource> : NotifierBase<TSource>, INotifier<TSource>
		{
			private Action<TSource> _noteList;
			
			protected override void DisposeActions() { _noteList = null; }

			#region INodifier<TSource> Members
			public void Add(Action<TSource> action) { Remove(action); _noteList += action; }
			public void Remove(Action<TSource> action) { _noteList -= action; }
			#endregion

			public Notifier(TSource source) : base(source) { }

			public void Invoke() { if (_noteList != null) _noteList(Source); }
		}
		private class Notifier<TSource, V1> : NotifierBase<TSource>, INotifier<TSource, V1>
		{
			private Action<TSource, V1> _noteList;

			protected override void DisposeActions() { _noteList = null; }

			#region INotifier<TSource,V1> Members
			public void Add(Action<TSource, V1> action) { Remove(action); _noteList += action; }
			public void Remove(Action<TSource, V1> action) { _noteList -= action; }
			#endregion

			public Notifier(TSource source) : base(source) { }

			public void Invoke(V1 val1) { if (_noteList != null) _noteList(Source, val1); }
		}
		private class Notifier<TSource, V1, V2> : NotifierBase<TSource>, INotifier<TSource, V1, V2>
		{
			private Action<TSource, V1, V2> _noteList;

			protected override void DisposeActions() { _noteList = null; }

			#region INotifier<TSource,V1,V2> Members
			public void Add(Action<TSource, V1, V2> action) { Remove(action); _noteList += action; }
			public void Remove(Action<TSource, V1, V2> action) { _noteList -= action; }
			#endregion

			public Notifier(TSource source) : base(source) { }

			public void Invoke(V1 val1, V2 val2) { if (_noteList != null) _noteList(Source, val1, val2); }
		}
		private class Notifier<TSource, V1, V2, V3> : NotifierBase<TSource>, INotifier<TSource, V1, V2, V3>
		{
			private Action<TSource, V1, V2, V3> _noteList;

			protected override void DisposeActions() { _noteList = null; }

			#region INotifier<TSource,V1,V2,V3> Members
			public void Add(Action<TSource, V1, V2, V3> action) { Remove(action); _noteList += action; }
			public void Remove(Action<TSource, V1, V2, V3> action) { _noteList -= action; }
			#endregion

			public Notifier(TSource source) : base(source) { }

			public void Invoke(V1 val1, V2 val2, V3 val3) { if (_noteList != null) _noteList(Source, val1, val2, val3); }
		}

		/// <summary>
		/// создает событие без параметров
		/// </summary>
		/// <typeparam name="TSource">тип источника события</typeparam>
		/// <param name="source">источник события, может быть пустым</param>
		/// <param name="invokeAction">ссылка на действие вызова события</param>
		/// <param name="disposeAction">ссылка на действие выгрузки события</param>
		/// <returns>событие без дополнительных параметров</returns>
		public static INotifier<TSource> Create<TSource>(TSource source, out Action invokeAction, out Action disposeAction)
		{
			Notifier<TSource> notifier = new Notifier<TSource>(source);
			invokeAction = notifier.Invoke;
			disposeAction = notifier.Dispose;
			return notifier;
		}
		/// <summary>
		/// создает событие с одним дополнительным параметром
		/// </summary>
		/// <typeparam name="TSource">тип источника события</typeparam>
		/// <typeparam name="V1">тип дополнительного параметра</typeparam>
		/// <param name="source">источник события, может быть пустым</param>
		/// <param name="invokeAction">ссылка на действие вызова события</param>
		/// <param name="disposeAction">ссылка на действие выгрузки события</param>
		/// <returns>событие с одним дополнительным параметром</returns>
		public static INotifier<TSource, V1> Create<TSource, V1>(TSource source,
			out Action<V1> invokeAction, out Action disposeAction)
		{
			Notifier<TSource, V1> notifier = new Notifier<TSource, V1>(source);
			invokeAction = notifier.Invoke;
			disposeAction = notifier.Dispose;
			return notifier;
		}
		/// <summary>
		/// создает событие с двумя дополнительными параметроми
		/// </summary>
		/// <typeparam name="TSource">тип источника события</typeparam>
		/// <typeparam name="V1">тип первого дополнительного параметра</typeparam>
		/// <typeparam name="V2">тип второго дополнительного параметра</typeparam>
		/// <param name="source">источник события, может быть пустым</param>
		/// <param name="invokeAction">ссылка на действие вызова события</param>
		/// <param name="disposeAction">ссылка на действие выгрузки события</param>
		/// <returns>событие с двумя дополнительными параметроми</returns>
		public static INotifier<TSource, V1, V2> Create<TSource, V1, V2>(TSource source,
			out Action<V1, V2> invokeAction, out Action disposeAction)
		{
			Notifier<TSource, V1, V2> notifier = new Notifier<TSource, V1, V2>(source);
			invokeAction = notifier.Invoke;
			disposeAction = notifier.Dispose;
			return notifier;
		}
		/// <summary>
		/// создает событие с тремя дополнительными параметроми
		/// </summary>
		/// <typeparam name="TSource">тип источника события</typeparam>
		/// <typeparam name="V1">тип первого дополнительного параметра</typeparam>
		/// <typeparam name="V2">тип второго дополнительного параметра</typeparam>
		/// <typeparam name="V3">тип третьего дополнительного параметра</typeparam>
		/// <param name="source">источник события, может быть пустым</param>
		/// <param name="invokeAction">ссылка на действие вызова события</param>
		/// <param name="disposeAction">ссылка на действие выгрузки события</param>
		/// <returns>событие с тремя дополнительными параметроми</returns>
		public static INotifier<TSource, V1, V2, V3> Create<TSource, V1, V2, V3>(TSource source,
			out Action<V1, V2, V3> invokeAction, out Action disposeAction)
		{
			Notifier<TSource, V1, V2, V3> notifier = new Notifier<TSource, V1, V2, V3>(source);
			invokeAction = notifier.Invoke;
			disposeAction = notifier.Dispose;
			return notifier;
		}
	}
}