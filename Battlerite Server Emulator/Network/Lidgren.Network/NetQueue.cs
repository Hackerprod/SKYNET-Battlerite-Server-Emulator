using System;
using System.Diagnostics;

namespace Lidgren.Network
{
	[DebuggerDisplay("Count={Count} Capacity={Capacity}")]
	public sealed class NetQueue<T>
	{
		private T[] m_items;

		private readonly object m_lock;

		private int m_size;

		private int m_head;

		public int Count => m_size;

		public int Capacity => m_items.Length;

		public NetQueue(int initialCapacity)
		{
			m_lock = new object();
			m_items = new T[initialCapacity];
		}

		public void Enqueue(T item)
		{
			lock (m_lock)
			{
				if (m_size == m_items.Length)
				{
					SetCapacity(m_items.Length + 8);
				}
				int num = (m_head + m_size) % m_items.Length;
				m_items[num] = item;
				m_size++;
			}
		}

		public void EnqueueFirst(T item)
		{
			lock (m_lock)
			{
				if (m_size >= m_items.Length)
				{
					SetCapacity(m_items.Length + 8);
				}
				m_head--;
				if (m_head < 0)
				{
					m_head = m_items.Length - 1;
				}
				m_items[m_head] = item;
				m_size++;
			}
		}

		private void SetCapacity(int newCapacity)
		{
			if (m_size == 0 && m_size == 0)
			{
				m_items = new T[newCapacity];
				m_head = 0;
			}
			else
			{
				T[] array = new T[newCapacity];
				if (m_head + m_size - 1 < m_items.Length)
				{
					Array.Copy(m_items, m_head, array, 0, m_size);
				}
				else
				{
					Array.Copy(m_items, m_head, array, 0, m_items.Length - m_head);
					Array.Copy(m_items, 0, array, m_items.Length - m_head, m_size - (m_items.Length - m_head));
				}
				m_items = array;
				m_head = 0;
			}
		}

		public bool TryDequeue(out T item)
		{
			if (m_size == 0)
			{
				item = default(T);
				return false;
			}
			lock (m_lock)
			{
				if (m_size == 0)
				{
					item = default(T);
					return false;
				}
				item = m_items[m_head];
				m_items[m_head] = default(T);
				m_head = (m_head + 1) % m_items.Length;
				m_size--;
				return true;
			}
		}

		public T TryPeek(int offset)
		{
			if (m_size == 0)
			{
				return default(T);
			}
			lock (m_lock)
			{
				if (m_size == 0)
				{
					return default(T);
				}
				return m_items[(m_head + offset) % m_items.Length];
			}
		}

		public bool Contains(T item)
		{
			lock (m_lock)
			{
				int num = m_head;
				for (int i = 0; i < m_size; i++)
				{
					if (m_items[num] == null)
					{
						if (item == null)
						{
							return true;
						}
					}
					else if (m_items[num].Equals(item))
					{
						return true;
					}
					num = (num + 1) % m_items.Length;
				}
			}
			return false;
		}

		public T[] ToArray()
		{
			lock (m_lock)
			{
				T[] array = new T[m_size];
				int num = m_head;
				for (int i = 0; i < m_size; i++)
				{
					array[i] = m_items[num++];
					if (num >= m_items.Length)
					{
						num = 0;
					}
				}
				return array;
			}
		}

		public void Clear()
		{
			lock (m_lock)
			{
				for (int i = 0; i < m_items.Length; i++)
				{
					m_items[i] = default(T);
				}
				m_head = 0;
				m_size = 0;
			}
		}
	}
}
