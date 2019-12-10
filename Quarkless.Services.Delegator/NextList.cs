using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quarkless.Services.Delegator
{
	public class NextList<T> : IEnumerable<T>
	{
		private readonly List<T> _items = new List<T>();
		private int _currentPosition = -1;
		public NextList() { }
		public NextList(IEnumerable<T> data)
		{
			_items = data.ToList();
		}

		public T Current => _currentPosition < 0 ? default : _items[_currentPosition];
		public T MoveNext()
		{
			if (_currentPosition >= _items.Count - 1)
				_currentPosition = -1;
			else
				_currentPosition++;
			return Current;
		}

		public T MoveNextRepeater()
		{
			MoveNext();
			if (Current != null) return Current;
			Reset();
			MoveNext();
			return Current;
		}
		public void Reset()
		{
			_currentPosition = -1;
		}
		public T this[int index]
		{
			get => _items[index];
			set => _items.Add(value);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _items.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
